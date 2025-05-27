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
        [SerializeReference, SerializeReferencePopup] List<ILiveBookSlot> _bookSlots = new();
        [SerializeField] bool _runOnStart = true;
        [SerializeField] ComponentBinding _componentBinding;

        [SerializeField] UnityEvent _onStartRunningAny;
        public UnityEvent OnStartRunningAny => _onStartRunningAny;
        [SerializeField] UnityEvent _onEndRunningAll;
        public UnityEvent OnEndRunningAll => _onEndRunningAll;

        int _runningCount;

        void Awake()
        {
            foreach (var slot in _bookSlots)
            {
                slot.OnEnable();
            }
        }

        void OnDestroy()
        {
            foreach (var slot in _bookSlots)
            {
                slot.OnDisable();
            }
        }

        async void Start()
        {
            if (_runOnStart)
            {
                await RunAsync();
            }
        }

        /// <summary>
        /// Run the currently held books.
        /// </summary>
        public void Run()
        {
            RunAsync().Forget();
        }

        /// <summary>
        /// Run the currently held books.
        /// </summary>
        public async UniTask RunAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dct =  destroyCancellationToken;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(dct, cancellationToken);
            
            try
            {
                IncrementRunnignCount();
                foreach (var slot in _bookSlots)
                {
                    if (slot.Book == null) continue;
                    await RunAsyncInternal(slot.Book, linkedCts.Token);
                }
            }
            catch (OperationCanceledException e)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(e.Message, e, cancellationToken);
                }
                else if (dct.IsCancellationRequested)
                {
                    // 破棄時は無視
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                DecrementRunnignCount();
            }
        }

        /// <summary>
        /// Run book.
        /// </summary>
        public void Run(ScriptBook book)
        {
            RunAsync(book).Forget();
        }

        /// <summary>
        /// Run book.
        /// </summary>
        public async UniTask RunAsync(ScriptBook book, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dct =  destroyCancellationToken;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(dct, cancellationToken);

            try
            {
                IncrementRunnignCount();
                await RunAsyncInternal(book, linkedCts.Token);
            }
            catch (OperationCanceledException e) when (e.CancellationToken == linkedCts.Token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(e.Message, e, cancellationToken);
                }
                else if (dct.IsCancellationRequested)
                {
                    // 破棄時は無視
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                DecrementRunnignCount();
            }
        }

        async UniTask RunAsyncInternal(ScriptBook book, CancellationToken linkedToken)
        {
            linkedToken.ThrowIfCancellationRequested();

            try
            {
                var processContext = ProcessContext.Rent();
                processContext.Resolvers.Add(_componentBinding);
                await ProcessRunner.RunMainSequenceAsync(
                    processContext,
                    book,
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