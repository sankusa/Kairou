using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public class RootProcess
    {
        static readonly ObjectPool<RootProcess> _pool = new(
            createFunc: static () => new RootProcess(),
            initialCapacity: 2,
            maxCapacity: 16,
            initialElements: 1
        );

        public CompositeObjectResolver ObjectResolver { get; } = new();

        readonly List<SeriesProcess> _unfinishedSeriesProcesses = new(2);

        bool _isRunning;
        internal bool IsTerminated { get; private set; }

        internal static RootProcess Rent()
        {
            var process = _pool.Rent();
            process.SetUp();
            return process;
        }

        void SetUp()
        {
            
        }

        internal static void Return(RootProcess process)
        {
            process.Clear();
            _pool.Return(process);
        }

        void Clear()
        {
            ObjectResolver.Clear();
            _unfinishedSeriesProcesses.Clear();
            _isRunning = false;
            IsTerminated = false;
        }

        internal SeriesProcess CreateSeriesProcess()
        {
            var seriesProcess = SeriesProcess.Rent(this);
            _unfinishedSeriesProcesses.Add(seriesProcess);
            return seriesProcess;
        }

        internal static async UniTask RunRootLoopAsync(PageProcess pageProcess, CancellationToken cancellationToken)
        {
            var bookProcess = pageProcess.BookProcess;
            var seriesProcess = bookProcess.SeriesProcess;
            var rootProcess = seriesProcess.RootProcess;
            
            bool isMainSequence = rootProcess._isRunning == false;
            rootProcess._isRunning = true;

            try
            {
                while(true)
                {
                    var subsequentProcessInfo = await SeriesProcess.RunSeriesLoopAsync(pageProcess, cancellationToken);

                    if (subsequentProcessInfo.IsSubsequentSeriesInfo == false) break;

                    seriesProcess = rootProcess.CreateSeriesProcess();
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
                if (isMainSequence) rootProcess.StartTerminationAsync(cancellationToken).Forget();
            }
        }

        async UniTask StartTerminationAsync(CancellationToken cancellationToken)
        {
            try
            {
                while(true)
                {
                    for (int i = _unfinishedSeriesProcesses.Count - 1; i >= 0; i--)
                    {
                        var p = _unfinishedSeriesProcesses[i];
                        if (p.IsTerminated)
                        {
                            _unfinishedSeriesProcesses.Remove(p);
                            SeriesProcess.Return(p);
                        }
                    }
                    if (_unfinishedSeriesProcesses.Count == 0) break;
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