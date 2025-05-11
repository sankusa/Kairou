using System;
using System.Linq;
using Kairou.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou
{
    [Serializable]
    public class BookHeaderPanel
    {
        [SerializeField] RestorableBookHolder _bookHolder = new();

        ObjectField _objectField;
        bool IsInitialized => _objectField != null;

        public void Initialize(VisualElement parent, VisualTreeAsset bookHeaderPanelUXML)
        {
            var pageListPanel = bookHeaderPanelUXML.Instantiate();
            parent.Add(pageListPanel);

            _objectField = pageListPanel.Q<ObjectField>();

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

            _objectField.value = _bookHolder.Owner;
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
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(PageListPanel)} is not initialized.");
        }
    }
}