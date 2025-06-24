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

        readonly List<BookProcess> _unfinishedBookProcesses = new(2);

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

        public async UniTask StartTerminationAsync(Action<RootProcess> onTerminated, CancellationToken cancellationToken)
        {
            try
            {
                while(true)
                {
                    for (int i = _unfinishedBookProcesses.Count - 1; i >= 0; i--)
                    {
                        var p = _unfinishedBookProcesses[i];
                        if (p.IsTerminated)
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
                onTerminated?.Invoke(this);
            }
        }
    }
}