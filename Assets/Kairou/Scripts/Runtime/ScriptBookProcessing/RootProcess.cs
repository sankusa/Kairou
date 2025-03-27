using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public class RootProcess
    {
        static readonly ObjectPool<RootProcess> _pool = new(
            createFunc: () => new RootProcess(),
            onRent: process =>
            {
                process.SetUp();
            },
            onReturn: process =>
            {
                process.Clear();
            }
        );

        public static RootProcess Rent() => _pool.Rent();
        public static void Return(RootProcess process) => _pool.Return(process);

        readonly List<ScriptBookProcess> _bookProcesses = new();

        CancellationTokenSource _cts;

        bool _isStarted;

        public CompositeObjectResolver ObjectResolver { get; } = new();

        private RootProcess() {}

        void SetUp()
        {
            _cts = new();
        }

        public void AddScriptBookProcess(ScriptBook scriptBook)
        {
            ScriptBookProcess process = ScriptBookProcess.Rent();
            process.SetUp(this, scriptBook);
            _bookProcesses.Add(process);
        }

        internal async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (_isStarted) throw new InvalidOperationException($"{nameof(RootProcess)} is already started.");
            _isStarted = true;

            cancellationToken.ThrowIfCancellationRequested();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

            while(_bookProcesses.Count > 0)
            {
                ScriptBookProcess process = _bookProcesses[0];
                try
                {
                    await process.StartAsync(linkedCts.Token);
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
                catch (OperationCanceledException)
                {
                    // ScriptBookProcess内部の事情によりキャンセルされた場合は握り潰して処理を続行
                }
                finally
                {
                    ReleaseScriptBookProcess(process);
                }
            }
        }

        public void Cancel() => _cts.Cancel();

        void Clear()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            
            ReleaseAllScriptBookProcess();
            _isStarted = false;

            ObjectResolver.Clear();
        }

        void ReleaseScriptBookProcess(ScriptBookProcess process)
        {
            _bookProcesses.Remove(process);
            ScriptBookProcess.Return(process);
        }

        void ReleaseAllScriptBookProcess()
        {
            foreach (ScriptBookProcess process in _bookProcesses)
            {
                ScriptBookProcess.Return(process);
            }
            _bookProcesses.Clear();
        }
    }
}