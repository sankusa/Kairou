using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public class ScriptBook : ISerializationCallbackReceiver
    {
        [SerializeField] List<Page> _pages = new();
        public IReadOnlyList<Page> Pages => _pages;

        void ISerializationCallbackReceiver.OnBeforeSerialize() {}

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            foreach (Page page in _pages)
            {
                (page as IPageInternalForScriptBook).SetParentBook(this);
            }
        }

        internal void AddPage(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            if (_pages.Contains(page)) throw new ArgumentException(nameof(page) + " is already added.");
            _pages.Add(page);
            (page as IPageInternalForScriptBook).SetParentBook(this);
        }

        internal void RemovePage(Page page)
        {
            if (page == null) return;
            if (_pages.Contains(page) == false) return;
            _pages.Remove(page);
            (page as IPageInternalForScriptBook).SetParentBook(null);
        }

        internal void RemovePageAt(int pageIndex)
        {
            if (_pages.HasElementAt(pageIndex) == false) return;
            var page = _pages[pageIndex];
            _pages.RemoveAt(pageIndex);
            (page as IPageInternalForScriptBook).SetParentBook(null);
        }

        internal void MovePage(int fromIndex, int toIndex)
        {
            _pages.Move(fromIndex, toIndex);
        }
    }
}