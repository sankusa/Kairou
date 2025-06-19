using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    // 基本的にアセットのロードはキャンセルできず、ロードの終了を待ってリリースするのが安全とされている
    // ので、キャンセルはしない設計。Preloadが終わるのを待ってからリリースを行う。
    public class BookPreloader
    {
        static readonly ObjectPool<HashSet<ScriptBook>> _preloadTargetBookPool = new(
            () => new HashSet<ScriptBook>(),
            initialCapacity: 2,
            maxCapacity: 2,
            onReturn: bookSet => bookSet.Clear());

        static int _preloadingBookCounter = 0;

        readonly ScriptBook _book;

        PreloadState _preloadState = PreloadState.Unpreloaded;
        public PreloadState PreloadState => _preloadState;
        readonly List<object> _preloadKeys = new();

        bool _releaseRequired;

        public BookPreloader(ScriptBook book) => _book = book;

        public async UniTask PreloadAsync(object preloadKey)
        {
            _releaseRequired = false;
            _preloadingBookCounter++;

            try
            {
                // 循環参照時の無限ループをブロック
                if (_preloadKeys.Contains(preloadKey)) return;

                _preloadKeys.Add(preloadKey);

                var bookSet = _preloadTargetBookPool.Rent();
                _book.GetChainPreloadTargetBooks(bookSet);
                foreach (ScriptBook book in bookSet)
                {
                    await book.Preloader.PreloadAsync(preloadKey);
                }
                _preloadTargetBookPool.Return(bookSet);

                // コマンドのPreload呼び出し
                if (_preloadState != PreloadState.Unpreloaded) return;

                _preloadState = PreloadState.Preloading;

                for (int i = 0; i < _book.Pages.Count; i++)
                {
                    for (int j = 0; j < _book.Pages[i].Commands.Count; j++)
                    {
                        if(_book.Pages[i].Commands[j] is IPreloadable preloadable)
                        {
                            // 各CommandのPreloadAsyncメソッドのエラーは握りつぶさず上に上げる
                            // 失敗した部分だけ飛ばして処理を継続したい場合はPreloadAsync内で握りつぶす
                            await preloadable.PreloadAsync();
                        }
                    }
                }
                
                _preloadState = PreloadState.Preloaded;
            }
            finally
            {
                _preloadingBookCounter--;

                if (_releaseRequired) ActualRelease();
            }
        }

        public void Release(object preloadKey)
        {
            // 早期リターン+循環参照時の無限ループをブロック
            if (_preloadKeys.Contains(preloadKey) == false) return;

            _preloadKeys.Remove(preloadKey);

            var bookSet = _preloadTargetBookPool.Rent();
            _book.GetChainPreloadTargetBooks(bookSet);
            foreach (ScriptBook book in bookSet)
            {
                book.Preloader.Release(preloadKey);
            }
            _preloadTargetBookPool.Return(bookSet);
            
            if (_preloadKeys.Count != 0) return;
            ScheduleTryActualReleaseAsync().Forget();
        }

        // _preloadKeysが0になるフレームで再びPreloadが呼ばれるようなケースでは
        // リリースを発生させたくないため、リリースを1フレーム後に発生させる
        async UniTaskVoid ScheduleTryActualReleaseAsync()
        {
            await UniTask.Yield();
            if (_preloadKeys.Count == 0) TryActualRelease();
        }

        public void TryActualRelease()
        {
            if (_preloadState == PreloadState.Unpreloaded) return;

            if (_preloadState == PreloadState.Preloading)
            {
                _releaseRequired = true;
                return;
            }

            ActualRelease();
        }

        void ActualRelease()
        {
            for (int i = 0; i < _book.Pages.Count; i++)
            {
                for (int j = 0; j < _book.Pages[i].Commands.Count; j++)
                {
                    if (_book.Pages[i].Commands[j] is IPreloadable preloadable)
                    preloadable.Release();
                }
            }

            _preloadState = PreloadState.Unpreloaded;
        }
    }
}