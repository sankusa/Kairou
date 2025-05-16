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

        public Page EntryPage => _pages.Count > 0 ? _pages[0] : null;

        [SerializeReference] List<VariableDefinition> _variables = new();
        public List<VariableDefinition> Variables => _variables;

        void ISerializationCallbackReceiver.OnBeforeSerialize() {}

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            foreach (Page page in _pages)
            {
                ((IPageInternalForBook)page).SetParentBook(this);
            }
        }

        public Page GetPage(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) return EntryPage;
            foreach (Page page in _pages)
            {
                if (page.Id == pageId) return page;
            }
            throw new KeyNotFoundException(pageId);
        }

        internal void AddPage(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            if (_pages.Contains(page)) throw new ArgumentException(nameof(page) + " is already added.");
            _pages.Add(page);
            ((IPageInternalForBook)page).SetParentBook(this);
        }

        internal void RemovePage(Page page)
        {
            if (page == null) return;
            if (_pages.Contains(page) == false) return;
            _pages.Remove(page);
            ((IPageInternalForBook)page).SetParentBook(null);
        }

        internal void RemovePageAt(int pageIndex)
        {
            if (_pages.HasElementAt(pageIndex) == false) return;
            var page = _pages[pageIndex];
            _pages.RemoveAt(pageIndex);
            ((IPageInternalForBook)page).SetParentBook(null);
        }

        internal void MovePage(int fromIndex, int toIndex)
        {
            _pages.Move(fromIndex, toIndex);
        }
    }
}