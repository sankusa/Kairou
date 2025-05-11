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
        
        public ScriptBookProcess BookProcess { get; private set; }
        Page _page;

        readonly ProcessInterface _processInterface;

        readonly VariableContainer _variables = new();
        public VariableContainer Variables => _variables;

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

        internal static PageProcess Rent(ScriptBookProcess parentProcess, Page page)
        {
            var process = _pool.Rent();
            process.SetUp(parentProcess, page);
            return process;
        }

        void SetUp(ScriptBookProcess parentProcess, Page page)
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
                    try
                    {
                        linkedCts.Token.ThrowIfCancellationRequested();

                        _currentCommandIndex = NextCommandIndex;
                        NextCommandIndex = _currentCommandIndex + 1;

                        Command command = _page.Commands[_currentCommandIndex];
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
                    awaiter.ForgetWithLogException(CreateLogExceptionHeader(true, asyncCommand));
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

        string CreateLogExceptionHeader(bool isAsync, Command command)
        {
            return $"{(isAsync ? "[Async] " : "")} {command.GetType().Name}";
        }

        public void GoToIndex(int commandIndex)
        {
            NextCommandIndex = commandIndex;
        }

        public void GoToEnd()
        {
            NextCommandIndex = _page.Commands.Count;
        }
    }
}