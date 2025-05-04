using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public class ScriptBookProcess
    {
        public enum ProcessState
        {
            UnInitialized,
            Ready,
            Running,
            MainSequenceFinished,
            AllPageProcessFinished,
        }

        static readonly ObjectPool<ScriptBookProcess> _pool = new(
            createFunc: static () => new ScriptBookProcess(),
            onRent: static process =>
            {

            },
            onReturn: static process =>
            {
                process.Clear();
            }
        );

        public static ScriptBookProcess Rent() => _pool.Rent();
        public static void Return(ScriptBookProcess process) => _pool.Return(process);

        ProcessState _state = ProcessState.UnInitialized;

        public RootProcess RootProcess { get; private set; }

        ScriptBook _scriptBook;

        readonly List<PageProcess> _pendingPageProcesses = new();
        readonly List<PageProcess> _executingPageProcesses = new();

        readonly VariableContainer _variables = new();
        public VariableContainer Variables => _variables;

        CancellationTokenSource _cts;

        Action<ScriptBookProcess> _onAllPageProcessFinished;

        private ScriptBookProcess() {}

        internal void SetUp(RootProcess parentProcess, ScriptBook scriptBook, int pageIndex = 0, Action<ScriptBookProcess> onAllPageProcessFinished = null)
        {
            if (_state != ProcessState.UnInitialized) throw new InvalidOperationException($"{nameof(ScriptBookProcess)} is already initialized.");

            if (parentProcess == null) throw new ArgumentNullException(nameof(parentProcess));
            if (scriptBook == null) throw new ArgumentNullException(nameof(scriptBook));

            RootProcess = parentProcess;
            _scriptBook = scriptBook;
            _variables.GenerateVariables(scriptBook.Variables);
            _cts = new();

            _onAllPageProcessFinished = onAllPageProcessFinished;

            _state = ProcessState.Ready;

            // 先頭ページのみを追加
            if (scriptBook.Pages.HasElementAt(pageIndex)) AddPageProcess(scriptBook.Pages[pageIndex]);
        }

        public void AddPageProcess(Page page)
        {
            if (_state != ProcessState.Ready && _state != ProcessState.Running) throw new InvalidOperationException($"{nameof(ScriptBookProcess)} is not ready.");

            if (page == null) throw new ArgumentNullException(nameof(page));
            if (_scriptBook.Pages.Contains(page) == false) throw new ArgumentException("Page does not belong to the current ScriptBook.", nameof(page));
            
            PageProcess process = PageProcess.Rent();
            process.SetUp(this, page, p =>
            {
                _executingPageProcesses.Remove(p);
            });
            _pendingPageProcesses.Add(process);
        }

        internal async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (_state != ProcessState.Ready) throw new InvalidOperationException($"{nameof(ScriptBookProcess)} is not ready.");
            _state = ProcessState.Running;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

            try
            {
                while (_pendingPageProcesses.Count > 0)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();

                    PageProcess process = _pendingPageProcesses[0];
                    _pendingPageProcesses.RemoveAt(0);
                    _executingPageProcesses.Add(process);
                    try
                    {
                        await process.StartAsync(linkedCts.Token);
                    }    
                    catch (OperationCanceledException e) when (e.CancellationToken != linkedCts.Token)
                    {
                        // PageProcess内部の事情によりキャンセルされた場合は握り潰して処理を続行
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
                    await UniTask.WaitUntil(() => _executingPageProcesses.Count == 0 && _pendingPageProcesses.Count == 0, cancellationToken: linkedToken);
                }
                finally
                {
                    _state = ProcessState.AllPageProcessFinished;
                    _onAllPageProcessFinished?.Invoke(this);
                    Return(this);
                }
            });
        }

        public void Cancel()
        {
            if (_state != ProcessState.Running) throw new InvalidOperationException($"{nameof(ScriptBookProcess)} is not running.");
            _cts.Cancel();
        }

        void Clear()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            RootProcess = null;
            _scriptBook = null;
            _pendingPageProcesses.Clear();
            _executingPageProcesses.Clear();
            _variables.Clear();

            _state = ProcessState.UnInitialized;
        }
    }
}