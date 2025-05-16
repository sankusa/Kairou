using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    [Serializable]
    public class PageListPanel
    {
        public delegate void PageSpecificAction(BookId bookId, int pageIndex);

        [SerializeField] RestorableBookHolder _bookHolder = new();

        ListView _listView;

        bool IsInitialized => _listView != null;

        public void Initialize(VisualElement parent, VisualTreeAsset pageListPanelUXML, PageSpecificAction onSelectionChanged, Action onCollectionChanged)
        {
            var pageListPanel = pageListPanelUXML.Instantiate();
            parent.Add(pageListPanel);

            _listView = pageListPanel.Q<ListView>("PageList");
            _listView.bindItem = (element, i) =>
            {
                element.Q<Label>().text = _bookHolder.Book.Pages[i].Id;
            };

            _listView.onAdd = _ =>
            {
                if (_bookHolder.Book == null) return;
                BookUtilForEditor.AddSetUpPage(_bookHolder.Owner, _bookHolder.Book);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
            };

            _listView.onRemove = _ =>
            {
                if (_bookHolder.Book == null) return;
                var selectedPage = (Page)_listView.selectedItem;
                BookUtilForEditor.RemovePage(_bookHolder.Owner, _bookHolder.Book, selectedPage);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
            };

            _listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                if (_bookHolder.Book == null) return;
                // Since there is no event to overwrite the swap process with a custom implementation,
                // the move is first reverted, recorded with Undo, and then moved again.
                _bookHolder.Book.MovePage(toIndex, fromIndex);
                BookUtilForEditor.MovePage(_bookHolder.Owner, _bookHolder.Book, fromIndex, toIndex);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
                // When dragging and swapping ListView elements, selectedIndicesChanged is not triggered, so it is manually triggered here instead.
                onSelectionChanged?.Invoke(_bookHolder.BookId, toIndex);
            };

            _listView.selectedIndicesChanged += pageIndices =>
            {
                if (_bookHolder.Book == null) return;
                var selectedPageIndex = pageIndices.FirstOrDefault();
                onSelectionChanged?.Invoke(_bookHolder.BookId, selectedPageIndex);
            };

            Reload();
        }

        public void SetTarget(BookId bookId)
        {
            _bookHolder.Reset(bookId);
            if (IsInitialized) Reload();
        }

        void Reload()
        {
            ThrowIfNotInitialized();

            if (_bookHolder.Book == null)
            {
                _listView.itemsSource = null;
                _listView.enabledSelf = false;
            }
            else
            {
                _listView.itemsSource = _bookHolder.Book.Pages as IList;
                _listView.enabledSelf = true;
            }

            _listView.selectedIndex = 0;
        }

        public void OnProjectOrHierarchyChanged()
        {
            if (_bookHolder.RestoreObjectIfNull())
            {
                Reload();
            }
        }

        public void OnUndoRedoPerformed()
        {
            ThrowIfNotInitialized();
            _listView.Rebuild();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(PageListPanel)} is not initialized.");
        }
    }
}