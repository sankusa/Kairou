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

        public void SetTarget(string bookPropertyPath)
        {
            if (IsInitialized)
            {
                _bookIdField.bindingPath = $"{bookPropertyPath}._id";
            }
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