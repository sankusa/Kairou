using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    public interface IBookSlot
    {
        ScriptBook Book { get; }
        bool ResolveOnRuntime { get; }

        string GetSummary();
    }

    [Serializable]
    public class BookComponentSlot : IBookSlot
    {
        [SerializeField] ScriptBookComponent _bookComponent;
        public ScriptBook Book => _bookComponent == null ? null : _bookComponent.Book;

        public bool ResolveOnRuntime => false;

        public string GetSummary() => _bookComponent == null ? "null" : $"(Component) BookId:[{_bookComponent.Book.Id}]"; 
    }

    [Serializable]
    public class BookAssetSlot : IBookSlot
    {
        [SerializeField] ScriptBookAsset _bookAsset;
        public ScriptBook Book => _bookAsset == null ? null : _bookAsset.Book;

        public bool ResolveOnRuntime => false;

        public string GetSummary() => _bookAsset == null ? "null" : $"(Asset) BookId:[{_bookAsset.Book.Id}]";
    }

    [Serializable]
    public class BookIdSlot : IBookSlot
    {
        [SerializeField] string _bookId;
        public ScriptBook Book => BookReference.Find(_bookId);

        public bool ResolveOnRuntime => true;

        public string GetSummary() => $"(Id) BookId:[{_bookId}]";
    }
}