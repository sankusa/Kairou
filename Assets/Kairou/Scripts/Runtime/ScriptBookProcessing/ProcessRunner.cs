using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public class ProcessRunner
    {
        public static async UniTask RunMainSequenceAsync(ProcessContext context, ScriptBook scriptBook, Action onTerminated, CancellationToken cancellationToken)
        {
            var rootProcess = RootProcess.Rent(context);
            var seriesProcess = rootProcess.CreateSeriesProcess();
            var bookProcess = seriesProcess.CreateBookProcess(scriptBook);
            var pageProcess = bookProcess.CreateEntryPageProcess();
            
            try
            {
                await RunProcessCoreLoopAsync(pageProcess, cancellationToken);
            }
            finally
            {
                UniTask.Void(async () =>
                {
                    try
                    {
                        await UniTask.WaitUntil(() => rootProcess.IsTerminated, cancellationToken: cancellationToken);
                    }
                    finally
                    {
                        RootProcess.Return(rootProcess);
                        onTerminated?.Invoke();
                    }
                });
            }
        }

        internal static async UniTask RunPageAsBookProcessSubSequenceAsync(PageProcess pageProcess, string pageId, CancellationToken cancellationToken)
        {
            var newPageProcess = pageProcess.BookProcess.CreatePageProcess(pageId);
            await RunProcessCoreLoopAsync(newPageProcess, cancellationToken);
        }

        internal static async UniTask RunBookAsSeriesProcessSubSequenceAsync(PageProcess pageProcess, ScriptBook book, string pageId, CancellationToken cancellationToken)
        {
            var newBookProcess = pageProcess.BookProcess.SeriesProcess.CreateBookProcess(book);
            PageProcess newPageProcess;
            if (string.IsNullOrEmpty(pageId))
            {
                newPageProcess = newBookProcess.CreateEntryPageProcess();
            }
            else
            {
                newPageProcess = newBookProcess.CreatePageProcess(pageId);
            }
            await RunProcessCoreLoopAsync(newPageProcess, cancellationToken);
        }

        internal static async UniTask RunBookAsRootProcessSubSequenceAsync(PageProcess pageProcess, ScriptBook book, string pageId, CancellationToken cancellationToken)
        {
            var newSeriesProcess = pageProcess.BookProcess.SeriesProcess.RootProcess.CreateSeriesProcess();
            var newBookProcess = newSeriesProcess.CreateBookProcess(book);
            PageProcess newPageProcess;
            if (string.IsNullOrEmpty(pageId))
            {
                newPageProcess = newBookProcess.CreateEntryPageProcess();
            }
            else
            {
                newPageProcess = newBookProcess.CreatePageProcess(pageId);
            }
            await RunProcessCoreLoopAsync(newPageProcess, cancellationToken);
        }

        // 実行関数の中核
        // ページプロセス再生＋後続処理の指定があればプロセスを生成して後続処理を行う。これを後続処理の指定が無くなるまで繰り返す
        static async UniTask RunProcessCoreLoopAsync(PageProcess pageProcess, CancellationToken cancellationToken)
        {
            await RootProcess.RunRootLoopAsync(pageProcess, cancellationToken);
        }
    }
}