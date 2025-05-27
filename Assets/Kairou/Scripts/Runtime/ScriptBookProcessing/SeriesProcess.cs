using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    internal class SeriesProcess
    {
        static readonly ObjectPool<SeriesProcess> _pool = new(
            createFunc: static () => new SeriesProcess()
        );

        public RootProcess RootProcess { get; private set; }

        readonly List<BookProcess> _unfinishedBookProcesses = new();

        bool _isRunning;
        internal bool IsTerminated { get; private set; }

        private SeriesProcess() {}

        internal static SeriesProcess Rent(RootProcess parentProcess)
        {
            var process = _pool.Rent();
            process.SetUp(parentProcess);
            return process;
        }

        void SetUp(RootProcess parentProcess)
        {
            if (parentProcess == null) throw new ArgumentNullException(nameof(parentProcess));

            RootProcess = parentProcess;
        }

        internal static void Return(SeriesProcess process)
        {
            process.Clear();
            _pool.Return(process);
        }

        void Clear()
        {
            _unfinishedBookProcesses.Clear();
            _isRunning = false;
            IsTerminated = false;
        }

        internal BookProcess CreateBookProcess(ScriptBook book)
        {
            var bookProcess = BookProcess.Rent(this, book);
            _unfinishedBookProcesses.Add(bookProcess);
            return bookProcess;
        }

        internal static async UniTask<SubsequentProcessInfo> RunSeriesLoopAsync(PageProcess pageProcess, CancellationToken cancellationToken)
        {
            var bookProcess = pageProcess.BookProcess;
            var seriesProcess = bookProcess.SeriesProcess;
            SubsequentProcessInfo subsequentProcessInfo;
            
            bool isMainSequence = seriesProcess._isRunning == false;
            seriesProcess._isRunning = true;

            try
            {
                while(true)
                {
                    subsequentProcessInfo = await BookProcess.RunBookLoopAsync(pageProcess, cancellationToken);
                    
                    if (subsequentProcessInfo.IsSubsequentBookInfo == false) break;

                    bookProcess = seriesProcess.CreateBookProcess(subsequentProcessInfo.Book);
                    if (subsequentProcessInfo.HasPageId)
                    {
                        pageProcess = bookProcess.CreatePageProcess(subsequentProcessInfo.PageId);
                    }
                    else
                    {
                        pageProcess = bookProcess.CreateEntryPageProcess();
                    }
                }
            }
            finally
            {
                if (isMainSequence) seriesProcess.StartTermination(cancellationToken);
            }
            return subsequentProcessInfo;
        }

        void StartTermination(CancellationToken cancellationToken)
        {
            StartTerminationAsync(cancellationToken).Forget();
        }

        async UniTask StartTerminationAsync(CancellationToken cancellationToken)
        {
            try
            {
                while(true)
                {
                    for (int i = _unfinishedBookProcesses.Count - 1; i >= 0; i--)
                    {
                        var p = _unfinishedBookProcesses[i];
                        if (_unfinishedBookProcesses[i].IsTerminated)
                        {
                            _unfinishedBookProcesses.Remove(p);
                            BookProcess.Return(p);
                        }
                    }
                    if (_unfinishedBookProcesses.Count == 0) break;
                    await UniTask.Yield(cancellationToken);
                }
            }
            finally
            {
                IsTerminated = true;
            }
        }
    }
}