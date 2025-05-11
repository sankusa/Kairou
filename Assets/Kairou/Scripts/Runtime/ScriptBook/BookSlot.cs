using System;
using UnityEngine;

namespace Kairou
{
    public interface IBookSlot
    {
        public abstract ScriptBook Book { get; }
    }

    [Serializable]
    public class BookComponentSlot : IBookSlot
    {
        [SerializeField] ScriptBookComponent _bookComponent;
        public ScriptBook Book => _bookComponent?.Book;
    }

    [Serializable]
    public class BookAssetSlot : IBookSlot
    {
        [SerializeField] ScriptBookAsset _bookAsset;
        public ScriptBook Book => _bookAsset?.Book;
    }
}