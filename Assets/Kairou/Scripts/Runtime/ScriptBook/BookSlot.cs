using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    public interface IBookSlot
    {
        ScriptBook Book { get; }
    }

    [Serializable]
    public class BookComponentSlot : IBookSlot
    {
        [SerializeField] ScriptBookComponent _bookComponent;
        public ScriptBook Book => _bookComponent == null ? null : _bookComponent.Book;
    }

    [Serializable]
    public class BookAssetSlot : IBookSlot
    {
        [SerializeField] ScriptBookAsset _bookAsset;
        public ScriptBook Book => _bookAsset == null ? null : _bookAsset.Book;
    }
}