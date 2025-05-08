using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Kairou
{
    public class ScriptBookEngine : MonoBehaviour
    {
        [SerializeField] List<ScriptBookOwnerReference> _scriptBookOwners = new();
        [SerializeField] bool _runOnStart = true;
        [SerializeField] ComponentBinding _componentBinding;

        [SerializeField] UnityEvent _onStartRunningAny;
        public UnityEvent OnStartRunningAny => _onStartRunningAny;
        [SerializeField] UnityEvent _onEndRunningAll;
        public UnityEvent OnEndRunningAll => _onEndRunningAll;

        int _runningCount;

        async void Start()
        {
            if (_runOnStart)
            {
                await RunAsync();
            }
        }

        /// <summary>
        /// Run the currently held scriptBooks.
        /// </summary>
        public void Run()
        {
            RunAsync().ForgetWithLogException();
        }

        /// <summary>
        /// Run the currently held scriptBooks.
        /// </summary>
        public async UniTask RunAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken);
            
            try
            {
                IncrementRunnignCount();
                foreach (ScriptBookOwnerReference scriptBookOwnerReference in _scriptBookOwners)
                {
                    if (scriptBookOwnerReference.ScriptBook == null) continue;
                    await RunAsyncInternal(scriptBookOwnerReference.ScriptBook, linkedCts.Token);
                }
            }
            catch (OperationCanceledException e)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(e.Message, e, cancellationToken);
                }
                else if (destroyCancellationToken.IsCancellationRequested)
                {
                    // 破棄時は無視
                }
                throw;
            }
            finally
            {
                DecrementRunnignCount();
            }
        }

        /// <summary>
        /// Run scriptBook.
        /// </summary>
        public void Run(ScriptBook scriptBook)
        {
            RunAsync(scriptBook).ForgetWithLogException();
        }

        /// <summary>
        /// Run scriptBook.
        /// </summary>
        public async UniTask RunAsync(ScriptBook scriptBook, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken);

            try
            {
                IncrementRunnignCount();
                await RunAsyncInternal(scriptBook, linkedCts.Token);
            }
            catch (OperationCanceledException e) when (e.CancellationToken == linkedCts.Token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(e.Message, e, cancellationToken);
                }
                else if (destroyCancellationToken.IsCancellationRequested)
                {
                    // 破棄時は無視
                }
                throw;
            }
            finally
            {
                DecrementRunnignCount();
            }
        }

        async UniTask RunAsyncInternal(ScriptBook scriptBook, CancellationToken linkedToken)
        {
            linkedToken.ThrowIfCancellationRequested();

            try
            {
                var processContext = ProcessContext.Rent();
                processContext.Resolvers.Add(_componentBinding);
                await ProcessRunner.RunMainSequenceAsync(
                    processContext,
                    scriptBook,
                    () => ProcessContext.Return(processContext),
                    linkedToken
                );
            }
            catch (OperationCanceledException e) when (e.CancellationToken != linkedToken)
            {
                // 内発的なキャンセルは無視
            }
        }

        void IncrementRunnignCount()
        {
            if (_runningCount == 0)
            {
                _onStartRunningAny.Invoke();
            }
            _runningCount++;
        }

        void DecrementRunnignCount()
        {
            _runningCount--;
            if (_runningCount == 0)
            {
                _onEndRunningAll.Invoke();
            }
        }
    }
}