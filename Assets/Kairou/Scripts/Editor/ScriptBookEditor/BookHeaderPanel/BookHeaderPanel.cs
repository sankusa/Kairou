using System;
using System.Linq;
using Kairou.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou
{
    [Serializable]
    public class BookHeaderPanel
    {
        [SerializeField] RestorableBookHolder _bookHolder = new();

        TextField _bookIdField;
        bool IsInitialized => _bookIdField != null;

        public void Initialize(VisualElement parent, VisualTreeAsset bookHeaderPanelUXML)
        {
            var bookHeaderPanel = bookHeaderPanelUXML.Instantiate();
            parent.Add(bookHeaderPanel);

            _bookIdField = bookHeaderPanel.Q<TextField>();

            Reload();
        }

        public void SetTarget(BookId bookId)
        {
            _bookHolder.Reset(bookId);
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            if (IsInitialized == false) return;

            _bookIdField.Unbind();

            if (_bookHolder.HasValidBook)
            {
                var serializedObject = new SerializedObject(_bookHolder.Owner);
                var bookProp = serializedObject.FindProperty(_bookHolder.BookPropertyPath);
                _bookIdField.BindProperty(bookProp.FindPropertyRelative("_id"));
            }
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
            
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(BookHeaderPanel)} is not initialized.");
        }
    }
}