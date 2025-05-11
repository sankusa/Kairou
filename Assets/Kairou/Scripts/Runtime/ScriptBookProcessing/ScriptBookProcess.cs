using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    internal class ScriptBookProcess
    {
        static readonly ObjectPool<ScriptBookProcess> _pool = new(
            createFunc: static () => new ScriptBookProcess()
        );

        public SeriesProcess SeriesProcess { get; private set; }

        ScriptBook _scriptBook;

        readonly List<PageProcess> _unfinishedPageProcesses = new();

        readonly VariableContainer _variables = new();
        public VariableContainer Variables => _variables;

        bool _isRunning;
        internal bool IsTerminated { get; private set; }

        private ScriptBookProcess() {}

        internal static ScriptBookProcess Rent(SeriesProcess parentProcess, ScriptBook scriptBook)
        {
            var process = _pool.Rent();
            process.SetUp(parentProcess, scriptBook);
            return process;
        }

        void SetUp(SeriesProcess parentProcess, ScriptBook scriptBook)
        {
            if (parentProcess == null) throw new ArgumentNullException(nameof(parentProcess));
            if (scriptBook == null) throw new ArgumentNullException(nameof(scriptBook));

            SeriesProcess = parentProcess;
            _scriptBook = scriptBook;
            _variables.GenerateVariables(scriptBook.Variables);
        }

        internal static void Return(ScriptBookProcess process)
        {
            process.Clear();
            _pool.Return(process);
        }

        void Clear()
        {
            SeriesProcess = null;
            _scriptBook = null;

            _unfinishedPageProcesses.Clear();
            _variables.Clear();

            _isRunning = false;
            IsTerminated = false;
        }

        internal PageProcess CreatePageProcess(string pageId)
        {
            var page = _scriptBook.GetPage(pageId);
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
                    await pageProcess.StartAsync(cancellationToken);
                    subsequentProcessInfo = pageProcess.SubsequentProcessInfo;

                    if (subsequentProcessInfo.IsSubsequentPageInfo == false) break;

                    pageProcess = bookProcess.CreatePageProcess(subsequentProcessInfo.PageId);
                }
            }
            finally
            {
                if (isMainSequence) bookProcess.StartTermination(cancellationToken);
            }
            return subsequentProcessInfo;
        }

        // ぺージプロセスの全終了を待機して終了フラグを立てる
        void StartTermination(CancellationToken cancellationToken)
        {
            UniTask.Void(async () =>
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
                    IsTerminated = true;
                }
            });
        }
    }
}