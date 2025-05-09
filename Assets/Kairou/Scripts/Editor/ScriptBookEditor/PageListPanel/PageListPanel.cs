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
        public delegate void PageSpecificAction(ScriptBookId scriptBookId, int pageIndex);

        [SerializeField] RestorableScriptBookHolder _bookHolder = new();

        ListView _listView;

        bool IsInitialized => _listView != null;

        public void Initialize(VisualElement parent, VisualTreeAsset pageListPanelUXML, PageSpecificAction onSelectionChanged, Action onCollectionChanged)
        {
            var pageListPanel = pageListPanelUXML.Instantiate();
            parent.Add(pageListPanel);

            _listView = pageListPanel.Q<ListView>("PageList");
            _listView.bindItem = (element, i) =>
            {
                element.Q<Label>().text = _bookHolder.ScriptBook.Pages[i].Id;
            };

            _listView.onAdd = _ =>
            {
                if (_bookHolder.ScriptBook == null) return;
                ScriptBookUtilForEditor.AddSetUpPage(_bookHolder.Owner, _bookHolder.ScriptBook);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
            };

            _listView.onRemove = _ =>
            {
                if (_bookHolder.ScriptBook == null) return;
                var selectedPage = (Page)_listView.selectedItem;
                ScriptBookUtilForEditor.RemovePage(_bookHolder.Owner, _bookHolder.ScriptBook, selectedPage);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
            };

            _listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                if (_bookHolder.ScriptBook == null) return;
                // Since there is no event to overwrite the swap process with a custom implementation,
                // the move is first reverted, recorded with Undo, and then moved again.
                _bookHolder.ScriptBook.MovePage(toIndex, fromIndex);
                ScriptBookUtilForEditor.MovePage(_bookHolder.Owner, _bookHolder.ScriptBook, fromIndex, toIndex);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
                // When dragging and swapping ListView elements, selectedIndicesChanged is not triggered, so it is manually triggered here instead.
                onSelectionChanged?.Invoke(_bookHolder.ScriptBookId, toIndex);
            };

            _listView.selectedIndicesChanged += pageIndices =>
            {
                if (_bookHolder.ScriptBook == null) return;
                var selectedPageIndex = pageIndices.FirstOrDefault();
                onSelectionChanged?.Invoke(_bookHolder.ScriptBookId, selectedPageIndex);
            };

            Reload();
        }

        public void SetTarget(ScriptBookId scriptBookId)
        {
            _bookHolder.Reset(scriptBookId);
            if (IsInitialized) Reload();
        }

        void Reload()
        {
            ThrowIfNotInitialized();

            if (_bookHolder.ScriptBook == null)
            {
                _listView.itemsSource = null;
                _listView.enabledSelf = false;
            }
            else
            {
                _listView.itemsSource = _bookHolder.ScriptBook.Pages;
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