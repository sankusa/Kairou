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
        [SerializeField] Object _scriptBookOwnerObject;
        IScriptBookOwner ScriptBookOwner => _scriptBookOwnerObject as IScriptBookOwner;

        ListView _listView;

        bool IsInitialized => _listView != null;

        public void Initialize(VisualElement parent, VisualTreeAsset pageListPanelUXML, Action<int> onSelectionChanged, Action onCollectionChanged)
        {
            var pageListPanel = pageListPanelUXML.Instantiate();
            parent.Add(pageListPanel);

            _listView = pageListPanel.Q<ListView>("PageList");
            _listView.bindItem = (element, i) =>
            {
                element.Q<Label>().text = ScriptBookOwner.ScriptBook.Pages[i].Name;
            };

            _listView.onAdd = _ =>
            {
                if (ScriptBookOwner == null) return;
                ScriptBookOwnerUtilForEditor.AddSetUpPage(ScriptBookOwner);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
            };

            _listView.onRemove = _ =>
            {
                if (ScriptBookOwner == null) return;
                var selectedPage = (Page)_listView.selectedItem;
                ScriptBookOwnerUtilForEditor.RemovePage(ScriptBookOwner, selectedPage);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
            };

            _listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                if (ScriptBookOwner == null) return;
                // Since there is no event to overwrite the swap process with a custom implementation,
                // the move is first reverted, recorded with Undo, and then moved again.
                ScriptBookOwner.ScriptBook.MovePage(toIndex, fromIndex);
                ScriptBookOwnerUtilForEditor.MovePage(ScriptBookOwner, fromIndex, toIndex);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
                // When dragging and swapping ListView elements, selectedIndicesChanged is not triggered, so it is manually triggered here instead.
                onSelectionChanged?.Invoke(toIndex);
            };

            _listView.selectedIndicesChanged += pageIndices =>
            {
                if (ScriptBookOwner == null) return;
                var selectedPageIndex = pageIndices.FirstOrDefault();
                onSelectionChanged?.Invoke(selectedPageIndex);
            };

            Reload();
        }

        public void SetTarget(IScriptBookOwner scriptBookOwner)
        {
            _scriptBookOwnerObject = scriptBookOwner?.AsObject();
            if (IsInitialized) Reload();
        }

        void Reload()
        {
            ThrowIfNotInitialized();

            if (ScriptBookOwner == null)
            {
                _listView.itemsSource = null;
                _listView.enabledSelf = false;
            }
            else
            {
                _listView.itemsSource = (IList)ScriptBookOwner.ScriptBook.Pages;
                _listView.enabledSelf = true;
            }

            _listView.selectedIndex = 0;
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