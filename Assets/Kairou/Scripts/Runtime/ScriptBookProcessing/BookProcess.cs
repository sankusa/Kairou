using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    internal class BookProcess
    {
        static readonly ObjectPool<BookProcess> _pool = new(
            createFunc: static () => new BookProcess(),
            initialCapacity: 2,
            maxCapacity: 32,
            initialElements: 2
        );

        public SeriesProcess SeriesProcess { get; private set; }

        ScriptBook _book;

        readonly List<PageProcess> _unfinishedPageProcesses = new(2);

        readonly VariableContainer _variables = new();
        public VariableContainer Variables => _variables;

        bool _isRunning;
        internal bool IsTerminated { get; private set; }

        private BookProcess() {}

        internal static BookProcess Rent(SeriesProcess parentProcess, ScriptBook book)
        {
            var process = _pool.Rent();
            process.SetUp(parentProcess, book);
            return process;
        }

        void SetUp(SeriesProcess parentProcess, ScriptBook book)
        {
            if (parentProcess == null) throw new ArgumentNullException(nameof(parentProcess));
            if (book == null) throw new ArgumentNullException(nameof(book));

            SeriesProcess = parentProcess;
            _book = book;
            _variables.GenerateVariables(book.Variables);

            book.Preloader.PreloadAsync(this).Forget();
        }

        internal static void Return(BookProcess process)
        {
            process.Clear();
            _pool.Return(process);
        }

        void Clear()
        {
            SeriesProcess = null;
            _book = null;

            _unfinishedPageProcesses.Clear();
            _variables.Clear();

            _isRunning = false;
            IsTerminated = false;
        }

        internal PageProcess CreatePageProcess(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) throw new ArgumentNullException(nameof(pageId));
            
            var page = _book.GetPage(pageId);
            return CreatePageProcessInternal(page);
        }

        internal PageProcess CreateEntryPageProcess()
        {
            return CreatePageProcessInternal(_book.EntryPage);
        }

        PageProcess CreatePageProcessInternal(Page page)
        {
            var pageProcess = PageProcess.Rent(this, page);
            _unfinishedPageProcesses.Add(pageProcess);
            return pageProcess;
        }

        internal static async UniTask<SubsequentProcessInfo> RunBookLoopAsync(PageProcess pageProcess, CancellationToken cancellationToken)
        {
            var bookProcess = pageProcess.BookProcess;
            SubsequentProcessInfo subsequentProcessInfo;
            
            bool isMainSequence = bookProcess._isRunning == false;
            bookProcess._isRunning = true;

            try
            {
                while (true)
                {
                    // プリロード終了まで待機
                    if (bookProcess._book.Preloader.PreloadState != PreloadState.Preloaded)
                    {
                        while (bookProcess._book.Preloader.PreloadState != PreloadState.Preloaded)
                        {
                            await UniTask.Yield(cancellationToken);
                        }
                    }

                    await pageProcess.StartAsync(cancellationToken);
                    subsequentProcessInfo = pageProcess.SubsequentProcessInfo;

                    if (subsequentProcessInfo.IsSubsequentPageInfo == false) break;

                    pageProcess = bookProcess.CreatePageProcess(subsequentProcessInfo.PageId);
                }
            }
            finally
            {
                if (isMainSequence) bookProcess.StartTerminationAsync(cancellationToken).Forget();
            }
            return subsequentProcessInfo;
        }

        async UniTask StartTerminationAsync(CancellationToken cancellationToken)
        {
            try
            {
                while(true)
                {
                    for (int i = _unfinishedPageProcesses.Count - 1; i >= 0; i--)
                    {
                        var p = _unfinishedPageProcesses[i];
                        if (p.IsTerminated)
                        {
                            _unfinishedPageProcesses.Remove(p);
                            PageProcess.Return(p);
                        }
                    }
                    if (_unfinishedPageProcesses.Count == 0) break;
                    await UniTask.Yield(cancellationToken);
                }
            }
            finally
            {
                _book.Preloader.Release(this);
                IsTerminated = true;
            }
        }
    }
}