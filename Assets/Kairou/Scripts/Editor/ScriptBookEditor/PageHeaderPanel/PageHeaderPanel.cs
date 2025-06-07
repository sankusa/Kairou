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
    public class PageHeaderPanel
    {
        [SerializeField] RestorableBookHolder _bookHolder = new();
        [SerializeField] int _pageIndex;
        bool ExistsTargetPage => _bookHolder.HasValidBook && _bookHolder.Book.Pages.HasElementAt(_pageIndex);

        TextField _textField;
        bool IsInitialized => _textField != null;

        public void Initialize(VisualElement parent, VisualTreeAsset pageHeaderPanelUXML, Action onPageChanged)
        {
            var pageHeaderPanel = pageHeaderPanelUXML.Instantiate();
            parent.Add(pageHeaderPanel);

            _textField = pageHeaderPanel.Q<TextField>();
            _textField.RegisterValueChangedCallback(evt => onPageChanged?.Invoke());

            Reload();
        }

        public void SetTarget(BookId bookId, int pageIndex)
        {
            _bookHolder.Reset(bookId);
            _pageIndex = pageIndex;
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            if (IsInitialized == false) return;

            _textField.Unbind();

            if (ExistsTargetPage)
            {
                var serializedObject = new SerializedObject(_bookHolder.Owner);
                var bookProp = serializedObject.FindProperty(_bookHolder.BookPropertyPath);
                var pagesProp = bookProp.FindPropertyRelative("_pages");
                var targetPageProp = pagesProp.GetArrayElementAtIndex(_pageIndex);

                _textField.BindProperty(targetPageProp.FindPropertyRelative("_id"));
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
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(PageHeaderPanel)} is not initialized.");
        }
    }
}