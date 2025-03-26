using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public class ScriptBookProcess
    {
        static readonly ObjectPool<ScriptBookProcess> _pool = new(
            createFunc: () => new ScriptBookProcess(),
            onRent: process =>
            {

            },
            onReturn: process =>
            {
                process.Clear();
            }
        );

        public static ScriptBookProcess Rent() => _pool.Rent();
        public static void Return(ScriptBookProcess process) => _pool.Return(process);

        public RootProcess RootProcess { get; private set; }

        ScriptBook _scriptBook;

        readonly List<PageProcess> _pageProcesses = new();

        CancellationTokenSource _cts;

        bool _isStarted;

        private ScriptBookProcess() {}

        internal void SetUp(RootProcess parentProcess, ScriptBook scriptBook, int pageIndex = 0)
        {
            if (parentProcess == null) throw new ArgumentNullException(nameof(parentProcess));
            if (scriptBook == null) throw new ArgumentNullException(nameof(scriptBook));

            RootProcess = parentProcess;
            _scriptBook = scriptBook;
            _cts = new();

            // 先頭ページのみを追加
            if (scriptBook.Pages.HasElementAt(pageIndex)) AddPageProcess(scriptBook.Pages[pageIndex]);
        }

        public void AddPageProcess(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            if (_scriptBook.Pages.Contains(page) == false) throw new ArgumentException(nameof(page) + " is already added.");
            
            PageProcess process = PageProcess.Rent();
            process.SetUp(this, page);
            _pageProcesses.Add(process);
        }

        internal async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (_isStarted) throw new InvalidOperationException($"{nameof(ScriptBookProcess)} is already started.");
            _isStarted = true;

            cancellationToken.ThrowIfCancellationRequested();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

            while (_pageProcesses.Count > 0)
            {
                PageProcess process = _pageProcesses[0];
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
                    // PageProcess内部の事情によりキャンセルされた場合は握り潰して処理を続行
                }
                finally
                {
                    ReleasePageProcess(process);
                }
            }
        }

        public void Cancel() => _cts.Cancel();

        void Clear()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            RootProcess = null;
            _scriptBook = null;
            ReleaseAllPageProcess();
            _isStarted = false;
        }

        void ReleasePageProcess(PageProcess pageProcess)
        {
            _pageProcesses.Remove(pageProcess);
            PageProcess.Return(pageProcess);
        }

        void ReleaseAllPageProcess()
        {
            foreach (PageProcess process in _pageProcesses)
            {
                PageProcess.Return(process);
            }
            _pageProcesses.Clear();
        }
    }
}