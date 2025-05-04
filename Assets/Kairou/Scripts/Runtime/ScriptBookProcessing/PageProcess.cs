using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public class PageProcess
    {
        public enum ProcessState
        {
            UnInitialized,
            Ready,
            Running,
            MainSequenceFinished,
            AllCommandFinished,
        }

        static readonly ObjectPool<PageProcess> _pool = new(
            createFunc: static () => new PageProcess(),
            onRent: static process =>
            {

            },
            onReturn: static process =>
            {
                process.Clear();
            }
        );

        public static PageProcess Rent() => _pool.Rent();
        public static void Return(PageProcess process) => _pool.Return(process);

        public ScriptBookProcess BookProcess { get; private set; }

        Page _page;

        readonly VariableContainer _variables = new();
        public VariableContainer Variables => _variables;

        CancellationTokenSource _cts;

        ProcessState _state = ProcessState.UnInitialized;

        int _currentCommandIndex;
        public int NextCommandIndex { get; set; }

        private int _asyncExecutingCommandCounter;
        private Action<PageProcess> _onAllCommandFinished;

        private PageProcess() {}

        public void SetUp(ScriptBookProcess parentProcess, Page page, Action<PageProcess> onAllCommandFinished = null)
        {
            if (_state != ProcessState.UnInitialized) throw new InvalidOperationException($"{nameof(PageProcess)} is already initialized.");

            if (parentProcess == null) throw new ArgumentNullException(nameof(parentProcess));
            if (page == null) throw new ArgumentNullException(nameof(page));

            BookProcess = parentProcess;
            _page = page;
            _variables.GenerateVariables(page.Variables);
            _cts = new();

            _onAllCommandFinished = onAllCommandFinished;

            _state = ProcessState.Ready;
        }

        internal async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (_state != ProcessState.Ready) throw new InvalidOperationException($"{nameof(PageProcess)} is not ready.");
            _state = ProcessState.Running;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

            try
            {
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
                            command.InvokeExecute(this);
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
                StartAsync_RunEndTask(linkedCts.Token);
            }
        }

        async UniTask StartAsync_ExecuteAsyncCommandAsync(AsyncCommand asyncCommand, CancellationToken linkedToken)
        {
            if (asyncCommand.AsyncCommandParameter.Await)
            {
                await asyncCommand.InvokeExecuteAsync(this, linkedToken);
            }
            else
            {
                UniTask awaiter = UniTask.Create(async () =>
                {
                    _asyncExecutingCommandCounter++;
                    try
                    {
                        await asyncCommand.InvokeExecuteAsync(this, linkedToken);
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
                    asyncCommand.AsyncCommandParameter.UniTaskStoreVariable.Find(this).SetValue(awaiter);
                }
            }
        }

        // 非同期実行中コマンドの全終了を検知。自分からプールに帰る。
        void StartAsync_RunEndTask(CancellationToken linkedToken)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    await UniTask.WaitUntil(() => _asyncExecutingCommandCounter == 0, cancellationToken: linkedToken);
                }
                finally
                {
                    _state = ProcessState.AllCommandFinished;
                    _onAllCommandFinished?.Invoke(this);
                    Return(this);
                }
            });
        }

        public void Cancel()
        {
            if (_state != ProcessState.Running) throw new InvalidOperationException($"{nameof(PageProcess)} is not running.");
            _cts.Cancel();
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
            _onAllCommandFinished = null;
        }

        string CreateLogExceptionHeader(bool isAsync, Command command)
        {
            return $"{(isAsync ? "[Async] " : "")} {command.GetType().Name}";
        }
    }
}