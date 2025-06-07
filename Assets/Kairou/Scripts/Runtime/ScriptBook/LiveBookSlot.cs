using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    public interface ILiveBookSlot
    {
        ScriptBook Book { get; }

        void OnEnable();
        void OnDisable();
    }

    [Serializable]
    public class LiveBookComponentSlot : ILiveBookSlot
    {
        [SerializeField] ScriptBookComponent _bookComponent;
        public ScriptBook Book => _bookComponent == null ? null : _bookComponent.Book;

        [SerializeField] bool _autoPreload = true;

        public void OnEnable()
        {
            if (Book == null) return;
            BookReference.Add(Book);
            if (_autoPreload)
            {
                Book.Preloader.PreloadAsync(this).Forget();
            }
        }

        public void OnDisable()
        {
            if (Book == null) return;
            if (_autoPreload)
            {
                Book.Preloader.Release(this);
            }
        }
    }

    [Serializable]
    public class LiveBookAssetSlot : ILiveBookSlot
    {
        [SerializeField] ScriptBookAsset _bookAsset;
        public ScriptBook Book => _bookAsset == null ? null : _bookAsset.Book;

        [SerializeField] bool _autoPreload = true;

        public void OnEnable()
        {
            if (Book == null) return;
            BookReference.Add(Book);
            if (_autoPreload)
            {
                Book.Preloader.PreloadAsync(this).Forget();
            }
        }

        public void OnDisable()
        {
            if (Book == null) return;
            if (_autoPreload)
            {
                Book.Preloader.Release(this);
            }
        }
    }

    [Serializable]
    public class LiveBookIdSlot : ILiveBookSlot
    {
        [SerializeField] string _bookId;
        public ScriptBook Book => BookReference.Find(_bookId);

        public void OnEnable()
        {

        }

        public void OnDisable()
        {

        }
    }
}