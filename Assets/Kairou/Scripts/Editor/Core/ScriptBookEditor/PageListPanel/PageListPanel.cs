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

        public void Initialize(VisualElement parent, VisualTreeAsset pageListPanelUXML, Action<int> onSelectionChanged, Action onCollectionChanged)
        {
            Initialize_Core(parent, pageListPanelUXML, onSelectionChanged, onCollectionChanged);
            SetScriptBookOwner(_scriptBookOwnerObject as IScriptBookOwner);
        }

        void Initialize_Core(VisualElement parent, VisualTreeAsset pageListPanelUXML, Action<int> onSelectionChanged, Action onCollectionChanged)
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
                var selectedPage = _listView.selectedItem as Page;
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
        }

        public void SetScriptBookOwner(IScriptBookOwner scriptBookOwner)
        {
            ThrowIfNotInitialized();
            _scriptBookOwnerObject = scriptBookOwner?.AsObject();
            Reload();
        }

        public void Reload()
        {
            ThrowIfNotInitialized();

            if (ScriptBookOwner == null)
            {
                _listView.itemsSource = null;
                _listView.enabledSelf = false;
            }
            else
            {
                _listView.itemsSource = ScriptBookOwner.ScriptBook.Pages as IList;
                _listView.enabledSelf = true;
            }

            _listView.selectedIndex = -1;
        }

        public void OnUndoRedoPerformed()
        {
            ThrowIfNotInitialized();
            _listView.Rebuild();
        }

        void ThrowIfNotInitialized()
        {
            if (_listView == null) throw new InvalidOperationException($"{nameof(PageListPanel)} is not initialized.");
        }
    }
}