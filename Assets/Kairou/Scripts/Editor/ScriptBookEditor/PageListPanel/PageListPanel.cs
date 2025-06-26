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
        [SerializeField] RestorableBookHolder _bookHolder = new();

        ListView _listView;

        bool IsInitialized => _listView != null;

        public void Initialize(VisualElement parent, VisualTreeAsset pageListPanelUXML, Action<int> onSelectionChanged, Action onCollectionChanged, bool resetViewData)
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
                _listView.RefreshItems();
                onCollectionChanged?.Invoke();
            };

            _listView.onRemove = _ =>
            {
                if (_bookHolder.Book == null) return;
                var selectedPage = (Page)_listView.selectedItem;
                BookUtilForEditor.RemovePage(_bookHolder.Owner, _bookHolder.Book, selectedPage);
                _listView.RefreshItems();
                onCollectionChanged?.Invoke();

                if (_bookHolder.Book.Pages.HasElementAt(_listView.selectedIndex) == false)
                {
                    _listView.selectedIndex = _bookHolder.Book.Pages.Count - 1;
                }
                else
                {
                    onSelectionChanged?.Invoke(_listView.selectedIndex);
                }
            };

            _listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                if (_bookHolder.Book == null) return;
                // Since there is no event to overwrite the swap process with a custom implementation,
                // the move is first reverted, recorded with Undo, and then moved again.
                _bookHolder.Book.MovePage(toIndex, fromIndex);
                BookUtilForEditor.MovePage(_bookHolder.Owner, _bookHolder.Book, fromIndex, toIndex);
                // _listView.RefreshItems();
                onCollectionChanged?.Invoke();
                // When dragging and swapping ListView elements, selectedIndicesChanged is not triggered, so it is manually triggered here instead.
                // onSelectionChanged?.Invoke(_bookHolder.BookId, toIndex);
                _listView.selectedIndex = toIndex;
            };

            // _listView.selectedIndicesChanged += pageIndices =>
            // {
            //     _selectedPageIndex = _listView.selectedIndex;
            //     if (_bookHolder.Book == null) return;
            //     var selectedPageIndex = pageIndices.FirstOrDefault();
            //     onSelectionChanged?.Invoke(_bookHolder.BookId, selectedPageIndex);
            // };

            _listView.schedule.Execute(() =>
            {
                if (resetViewData) _listView.selectedIndex = -1;
                _listView.selectedIndicesChanged += indices =>
                {
                    if (_bookHolder.Book == null) return;
                    if (_listView.selectedIndex != -1)
                    {
                        onSelectionChanged?.Invoke(_listView.selectedIndex);
                    }
                };
            })
            .ExecuteLater(1);

            Reload();
        }

        public void SetTarget(BookId bookId)
        {
            if (bookId != _bookHolder.BookId) _listView.SetSelectionWithoutNotify(new int[] { 0 });
            _bookHolder.Reset(bookId);
            if (IsInitialized) Reload(_listView.selectedIndex);
        }

        public void Reload()
        {
            Reload(_listView.selectedIndex);
        }

        public void Reload(int selectedPageIndex)
        {
            if(IsInitialized == false) return;

            if (_bookHolder.HasValidBook)
            {
                _listView.itemsSource = _bookHolder.Book.Pages as IList;
                _listView.enabledSelf = true;
            }
            else
            {
                _listView.itemsSource = null;
                _listView.enabledSelf = false;
            }

            _listView.SetSelectionWithoutNotify(new int[] {selectedPageIndex});
            _listView.RefreshItems();
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
            if (IsInitialized == false) return;
            _listView.RefreshItems();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(PageListPanel)} is not initialized."); 
        }
    }
}