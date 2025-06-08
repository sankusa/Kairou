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
        TextField _bookIdField;
        bool IsInitialized => _bookIdField != null;

        public void Initialize(VisualElement parent, VisualTreeAsset bookHeaderPanelUXML)
        {
            var bookHeaderPanel = bookHeaderPanelUXML.Instantiate();
            parent.Add(bookHeaderPanel);

            _bookIdField = bookHeaderPanel.Q<TextField>();

            Reload();
        }

        public void Bind(SerializedObject serializedObject, string bookPropertyPath)
        {
            if (IsInitialized == false) return;
            if (serializedObject == null || bookPropertyPath == null)
            {
                _bookIdField.bindingPath = null;
                _bookIdField.Unbind();
                _bookIdField.value = null;
                return;
            }
            _bookIdField.bindingPath = $"{bookPropertyPath}._id";
            _bookIdField.Bind(serializedObject);
        }

        public void Reload()
        {
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(BookHeaderPanel)} is not initialized.");
        }
    }
}