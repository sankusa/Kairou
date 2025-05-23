using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    internal class PageProcess
    {
        public enum ProcessState
        {
            UnInitialized,
            Ready,
            Running,
            MainSequenceFinished,
            Terminated,
        }

        static readonly ObjectPool<PageProcess> _pool = new(
            createFunc: static () => new PageProcess()
        );
        
        public BookProcess BookProcess { get; private set; }
        Page _page;

        readonly ProcessInterface _processInterface;

        readonly VariableContainer _variables = new();
        public VariableContainer Variables => _variables;

        readonly Stack<Block>_blockStack = new();

        ProcessState _state = ProcessState.UnInitialized;
        internal bool IsTerminated => _state == ProcessState.Terminated;

        int _currentCommandIndex;
        public int NextCommandIndex { get; private set; }
        
        public SubsequentProcessInfo SubsequentProcessInfo { get; set; }

        int _asyncExecutingCommandCounter;

        CancellationTokenSource _cts;

        private PageProcess()
        {
            _processInterface = new ProcessInterface(this);
        }

        internal static PageProcess Rent(BookProcess parentProcess, Page page)
        {
            var process = _pool.Rent();
            process.SetUp(parentProcess, page);
            return process;
        }

        void SetUp(BookProcess parentProcess, Page page)
        {
            if (_state != ProcessState.UnInitialized) throw new InvalidOperationException($"{nameof(PageProcess)} is already initialized.");

            if (parentProcess == null) throw new ArgumentNullException(nameof(parentProcess));
            // if (page == null) throw new ArgumentNullException(nameof(page));

            BookProcess = parentProcess;
            _page = page;
            if (_page != null)
            {
                _variables.GenerateVariables(page.Variables);
            }
            _cts = new();

            _state = ProcessState.Ready;
        }

        internal static void Return(PageProcess process)
        {
            process.Clear();
            _pool.Return(process);
        }

        void Clear()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            BookProcess = null;
            _page = null;
            _variables.Clear();
            _blockStack.Clear();

            _state = ProcessState.UnInitialized;

            _currentCommandIndex = 0;
            NextCommandIndex = 0;

            _asyncExecutingCommandCounter = 0;

            SubsequentProcessInfo = default;
        }

        internal async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (_state != ProcessState.Ready) throw new InvalidOperationException($"{nameof(PageProcess)} is not ready.");
            _state = ProcessState.Running;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

            try
            {
                if (_page == null) return;
                while (_page.Commands.HasElementAt(NextCommandIndex))
                {
                    Command command = null;
                    try
                    {
                        linkedCts.Token.ThrowIfCancellationRequested();

                        _currentCommandIndex = NextCommandIndex;
                        NextCommandIndex = _currentCommandIndex + 1;

                        command = _page.Commands[_currentCommandIndex];
                        if (command is AsyncCommand asyncCommand)
                        {
                            await StartAsync_ExecuteAsyncCommandAsync(asyncCommand, linkedCts.Token);
                        }
                        else
                        {
                            command.InvokeExecute(_processInterface);
                        }
                    }
                    catch (OperationCanceledException e) when (e.CancellationToken != linkedCts.Token)
                    {
                        // Command内部の事情によりキャンセルされた場合は握り潰して処理を続行
                    }
                    catch
                    {
                        Debug.LogError(CreateErrorMessage(command, _currentCommandIndex));
                        throw;
                    }

                    // ブロックの範囲を越えていたらブロックを破棄
                    while(true)
                    {
                        var block = PeekBlock();
                        if(block == null) break;
                        if(block.StartIndex <= NextCommandIndex && NextCommandIndex <= block.EndIndex) break;
                        PopBlock().Dispose();
                    }
                }
            }
            catch (OperationCanceledException e) when (e.CancellationToken == linkedCts.Token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(e.Message, e, cancellationToken);
                }
                else if (_cts.IsCancellationRequested)
                {
                    throw new OperationCanceledException(e.Message, e, _cts.Token);
                }
                throw;
            }
            finally
            {
                _state = ProcessState.MainSequenceFinished;
                StartTermination(linkedCts.Token);
            }
        }

        async UniTask StartAsync_ExecuteAsyncCommandAsync(AsyncCommand asyncCommand, CancellationToken linkedToken)
        {
            if (asyncCommand.AsyncCommandParameter.Await)
            {
                await asyncCommand.InvokeExecuteAsync(_processInterface, linkedToken);
            }
            else
            {
                UniTask awaiter = UniTask.Create(async () =>
                {
                    _asyncExecutingCommandCounter++;
                    try
                    {
                        await asyncCommand.InvokeExecuteAsync(_processInterface, linkedToken);
                    }
                    finally
                    {
                        _asyncExecutingCommandCounter--;
                    }
                });
                if (asyncCommand.AsyncCommandParameter.UniTaskStoreVariable.IsEmpty())
                {
                    int commandIndex = _currentCommandIndex;
                    awaiter.Forget(e =>
                    {
                        if (e is OperationCanceledException) return;
                        Debug.LogError(CreateErrorMessage(asyncCommand, commandIndex));
                        Debug.LogException(e);
                    });
                }
                else
                {
                    asyncCommand.AsyncCommandParameter.UniTaskStoreVariable.Find(_processInterface).SetValue(awaiter);
                }
            }
        }

        void StartTermination(CancellationToken cancellationToken)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    await UniTask.WaitUntil(() => _asyncExecutingCommandCounter == 0, cancellationToken: cancellationToken);
                }
                finally
                {
                    _state = ProcessState.Terminated;
                }
            });
        }

        public void GoToIndex(int commandIndex)
        {
            NextCommandIndex = commandIndex;
        }

        public void GoToEnd()
        {
            NextCommandIndex = _page.Commands.Count;
        }

        public void PushBlock(Block block) => _blockStack.Push(block);

        public Block PopBlock() => _blockStack.Count > 0 ? _blockStack.Pop() : null;

        public Block PeekBlock() => _blockStack.Count > 0 ? _blockStack.Peek() : null;

        public bool TryPopBlock<TBlock>(out TBlock block) where TBlock : Block
        {
            block = null;
            if (_blockStack.Count == 0) return false;
            if (_blockStack.Peek() is not TBlock tBlock) return false;
            _blockStack.Pop();
            block = tBlock;
            return true;
        }

        string CreateErrorMessage(Command command, int commandIndex)
        {
            return $"Error: PageId[{_page.Id}], CommandIndex[{commandIndex}], CommandType[{command?.GetType()}]";
        }
    }
}