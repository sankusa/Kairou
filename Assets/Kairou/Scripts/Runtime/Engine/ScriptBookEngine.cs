using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
#if KAIROU_VCONTAINER_SUPPORT
using VContainer;
#endif

namespace Kairou
{
    public class ScriptBookEngine : MonoBehaviour
    {
        readonly struct TokenCombiner : IDisposable
        {
            readonly CancellationTokenSource _linkedCts;
            readonly CancellationTokenRegistration _destroyTokenRegistration;
            readonly CancellationTokenRegistration _externalTokenRegistration;
            readonly CancellationToken _resultToken;

            public readonly CancellationToken Token => _resultToken;

            public TokenCombiner(CancellationToken destroyToken, CancellationToken externalToken)
            {
                if (externalToken.CanBeCanceled)
                {
                    _linkedCts = new CancellationTokenSource();
                    _externalTokenRegistration = externalToken.RegisterWithoutCaptureExecutionContext(static x => ((CancellationTokenSource)x).Cancel(), _linkedCts);
                    _destroyTokenRegistration = destroyToken.RegisterWithoutCaptureExecutionContext(static x => ((CancellationTokenSource)x).Cancel(), _linkedCts);
                    _resultToken = _linkedCts.Token;
                }
                else
                {
                    _linkedCts = null;
                    _externalTokenRegistration = default;
                    _destroyTokenRegistration = default;
                    _resultToken = destroyToken;
                }
            }

            public void Dispose()
            {
                _linkedCts?.Dispose();
                _externalTokenRegistration.Dispose();
                _destroyTokenRegistration.Dispose();
            }
        }

        [SerializeReference, SerializeReferencePopup] List<ILiveBookSlot> _bookSlots = new();
        [SerializeField] bool _runOnStart = true;
        [SerializeField] ComponentBinding _componentBinding;

        [SerializeField] UnityEvent _onStartRunningAny;
        public UnityEvent OnStartRunningAny => _onStartRunningAny;
        [SerializeField] UnityEvent _onEndRunningAll;
        public UnityEvent OnEndRunningAll => _onEndRunningAll;

        int _runningCount;

#if KAIROU_VCONTAINER_SUPPORT
        [VContainer.Inject]
        VContainer.IObjectResolver _vcontainerResolver;
#endif
#if KAIROU_EXTENJECT_SUPPORT
        [Zenject.Inject]
        Zenject.DiContainer _extenjectDiContainer;
#endif

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

            var dct = destroyCancellationToken;
            var tokenConbiner = new TokenCombiner(dct, cancellationToken);
            
            try
            {
                IncrementRunningCount();
                foreach (var slot in _bookSlots)
                {
                    if (slot.Book == null) continue;
                    await RunAsyncInternal(slot.Book, tokenConbiner.Token);
                }
            }
            catch (OperationCanceledException e) when (e.CancellationToken != tokenConbiner.Token)
            {
                // 内発的キャンセルは無視
            }
            catch (OperationCanceledException e) when (e.CancellationToken == tokenConbiner.Token)
            {
                if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
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
                DecrementRunningCount();
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

            var dct = destroyCancellationToken;
            var tokenConbiner = new TokenCombiner(dct, cancellationToken);

            try
            {
                IncrementRunningCount();
                await RunAsyncInternal(book, tokenConbiner.Token);
            }
            catch (OperationCanceledException e) when (e.CancellationToken != tokenConbiner.Token)
            {
                // 内発的キャンセルは無視
            }
            catch (OperationCanceledException e) when (e.CancellationToken == tokenConbiner.Token)
            {
                if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
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
                DecrementRunningCount();
            }
        }

        UniTask RunAsyncInternal(ScriptBook book, CancellationToken linkedToken)
        {
            var rootProcess = RootProcess.Rent();
            rootProcess.ObjectResolver.Add(_componentBinding);
#if KAIROU_VCONTAINER_SUPPORT
            if (_vcontainerResolver != null)
            {
                rootProcess.ObjectResolver.SetVContainerResolver(_vcontainerResolver);
            }
#endif
#if KAIROU_EXTENJECT_SUPPORT
            if (_extenjectDiContainer != null)
            {
                rootProcess.ObjectResolver.SetExtenjectDiContainer(_extenjectDiContainer);
            }
#endif

            return ProcessRunner.RunMainSequenceAsync(
                rootProcess,
                book,
                static rootProcess => RootProcess.Return(rootProcess),
                linkedToken
            );
        }

        void IncrementRunningCount()
        {
            if (_runningCount == 0)
            {
                _onStartRunningAny.Invoke();
            }
            _runningCount++;
        }

        void DecrementRunningCount()
        {
            _runningCount--;
            if (_runningCount == 0)
            {
                _onEndRunningAll.Invoke();
            }
        }
    }
}