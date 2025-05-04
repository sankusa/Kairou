using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public class RootProcess
    {
        public enum ProcessState
        {
            UnInitialized,
            Ready,
            Running,
            MainSequenceFinished,
            AllBookProcessFinished,
        }

        static readonly ObjectPool<RootProcess> _pool = new(
            createFunc: static () => new RootProcess(),
            onRent: static process =>
            {

            },
            onReturn: static process =>
            {
                process.Clear();
            }
        );

        public static RootProcess Rent() => _pool.Rent();
        public static void Return(RootProcess process) => _pool.Return(process);

        readonly List<ScriptBookProcess> _pendingBookProcesses = new();
        readonly List<ScriptBookProcess> _executingBookProcesses = new();

        CancellationTokenSource _cts;

        ProcessState _state = ProcessState.UnInitialized;

        public CompositeObjectResolver ObjectResolver { get; } = new();

        Action<RootProcess> _onAllBookProcessFinished;

        private RootProcess() {}

        public void SetUp(Action<RootProcess> onAllBookProcessFinished = null)
        {
            if (_state != ProcessState.UnInitialized) throw new InvalidOperationException($"{nameof(RootProcess)} is already initialized.");

            _cts = new();

            _onAllBookProcessFinished = onAllBookProcessFinished;

            _state = ProcessState.Ready;
        }

        public void AddScriptBookProcess(ScriptBook scriptBook)
        {
            ScriptBookProcess process = ScriptBookProcess.Rent();
            process.SetUp(this, scriptBook, 0, p =>
            {
                _executingBookProcesses.Remove(p);
            });
            _pendingBookProcesses.Add(process);
        }

        internal async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (_state != ProcessState.Ready) throw new InvalidOperationException($"{nameof(RootProcess)} is not ready.");
            _state = ProcessState.Running;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

            try
            {
                while(_pendingBookProcesses.Count > 0)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();

                    ScriptBookProcess process = _pendingBookProcesses[0];
                    _pendingBookProcesses.RemoveAt(0);
                    _executingBookProcesses.Add(process);
                    try
                    {
                        await process.StartAsync(linkedCts.Token);
                    }
                    catch (OperationCanceledException e) when (e.CancellationToken != linkedCts.Token)
                    {
                        // ScriptBookProcess内部の事情によりキャンセルされた場合は握り潰して処理を続行
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

        // 非同期実行中ぺージの全終了を検知。自分からプールに帰る。
        void StartAsync_RunEndTask(CancellationToken linkedToken)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    await UniTask.WaitUntil(() => _executingBookProcesses.Count == 0 && _pendingBookProcesses.Count == 0, cancellationToken: linkedToken);
                }
                finally
                {
                    _state = ProcessState.AllBookProcessFinished;
                    _onAllBookProcessFinished?.Invoke(this);
                    Return(this);
                }
            });
        }

        public void Cancel()
        {
            if (_state != ProcessState.Running) throw new InvalidOperationException($"{nameof(RootProcess)} is not running.");
            _cts.Cancel();
        }

        void Clear()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            
            _pendingBookProcesses.Clear();
            _executingBookProcesses.Clear();

            _state = ProcessState.UnInitialized;

            ObjectResolver.Clear();
        }
    }
}