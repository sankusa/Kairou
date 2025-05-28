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
            createFunc: static () => new SeriesProcess(),
            initialCapacity: 2,
            maxCapacity: 16,
            initialElements: 1
        );

        public RootProcess RootProcess { get; private set; }

        readonly List<BookProcess> _unfinishedBookProcesses = new(2);

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

        public bool TryActivateRunningFlag()
        {
            if (_isRunning) return false;
            _isRunning = true;
            return true;
        }

        internal BookProcess CreateBookProcess(ScriptBook book)
        {
            var bookProcess = BookProcess.Rent(this, book);
            _unfinishedBookProcesses.Add(bookProcess);
            return bookProcess;
        }

        public async UniTask StartTerminationAsync(CancellationToken cancellationToken)
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