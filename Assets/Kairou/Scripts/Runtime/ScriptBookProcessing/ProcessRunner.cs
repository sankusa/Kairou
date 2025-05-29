using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public class ProcessRunner
    {
        public static UniTask RunMainSequenceAsync(RootProcess rootProcess, ScriptBook scriptBook, Action<RootProcess> onTerminated, CancellationToken cancellationToken)
        {
            var seriesProcess = rootProcess.CreateSeriesProcess();
            var bookProcess = seriesProcess.CreateBookProcess(scriptBook);
            var pageProcess = bookProcess.CreateEntryPageProcess();
            
            return RunProcessCoreLoopAsync(pageProcess, onTerminated, cancellationToken);
        }

        internal static UniTask RunPageAsBookProcessSubSequenceAsync(PageProcess pageProcess, string pageId, CancellationToken cancellationToken)
        {
            var newPageProcess = pageProcess.BookProcess.CreatePageProcess(pageId);
            return RunProcessCoreLoopAsync(newPageProcess, null, cancellationToken);
        }

        internal static UniTask RunBookAsSeriesProcessSubSequenceAsync(PageProcess pageProcess, ScriptBook book, string pageId, CancellationToken cancellationToken)
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
            return RunProcessCoreLoopAsync(newPageProcess, null, cancellationToken);
        }

        internal static UniTask RunBookAsRootProcessSubSequenceAsync(PageProcess pageProcess, ScriptBook book, string pageId, CancellationToken cancellationToken)
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
            return RunProcessCoreLoopAsync(newPageProcess, null, cancellationToken);
        }

        // 実行関数の中核
        // ページプロセス再生＋後続処理の指定があればプロセスを生成して後続処理を行う。これを後続処理の指定が無くなるまで繰り返す
        static async UniTask RunProcessCoreLoopAsync(PageProcess pageProcess, Action<RootProcess> onTerminatedIfMainSequence, CancellationToken cancellationToken)
        {
            var bookProcess = pageProcess.BookProcess;
            var seriesProcess = bookProcess.SeriesProcess;
            var rootProcess = seriesProcess.RootProcess;

            SubsequentProcessInfo subsequentInfo;
            
            bool isMainRootSequence = rootProcess.TryActivateRunningFlag();

            try
            {
                while(true)
                {
                    bool isMainSeriesSequence = seriesProcess.TryActivateRunningFlag();

                    try
                    {
                        while(true)
                        {
                            bool isMainBookProcess = bookProcess.TryActivateRunningFlag();

                            try
                            {
                                while (true)
                                {
                                    // プリロード終了まで待機
                                    if (bookProcess.Book.Preloader.PreloadState != PreloadState.Preloaded)
                                    {
                                        while (bookProcess.Book.Preloader.PreloadState != PreloadState.Preloaded)
                                        {
                                            await UniTask.Yield(cancellationToken);
                                        }
                                    }

                                    await pageProcess.StartAsync(cancellationToken);
                                    subsequentInfo = pageProcess.SubsequentProcessInfo;

                                    if (subsequentInfo.IsSubsequentPageInfo == false) break;

                                    pageProcess = bookProcess.CreatePageProcess(subsequentInfo.PageId);
                                }
                            }
                            finally
                            {
                                if (isMainBookProcess) bookProcess.StartTerminationAsync(cancellationToken).Forget();
                            }
                            
                            if (subsequentInfo.IsSubsequentBookInfo == false) break;

                            bookProcess = seriesProcess.CreateBookProcess(subsequentInfo.Book);
                            if (subsequentInfo.HasPageId)
                            {
                                pageProcess = bookProcess.CreatePageProcess(subsequentInfo.PageId);
                            }
                            else
                            {
                                pageProcess = bookProcess.CreateEntryPageProcess();
                            }
                        }
                    }
                    finally
                    {
                        if (isMainSeriesSequence) seriesProcess.StartTerminationAsync(cancellationToken).Forget();
                    }

                    if (subsequentInfo.IsSubsequentSeriesInfo == false) break;
                    
                    seriesProcess = rootProcess.CreateSeriesProcess();
                    bookProcess = seriesProcess.CreateBookProcess(subsequentInfo.Book);
                    if (subsequentInfo.HasPageId)
                    {
                        pageProcess = bookProcess.CreatePageProcess(subsequentInfo.PageId);
                    }
                    else
                    {
                        pageProcess = bookProcess.CreateEntryPageProcess();
                    }
                    
                }
            }
            finally
            {
                if (isMainRootSequence) rootProcess.StartTerminationAsync(onTerminatedIfMainSequence, cancellationToken).Forget();
            }
        }
    }
}