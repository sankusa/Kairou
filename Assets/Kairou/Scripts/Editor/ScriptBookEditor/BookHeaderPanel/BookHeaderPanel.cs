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
        SerializedObject _serializedObject;
        SerializedProperty _bookProp;
        SerializedProperty _idProp;
        bool IsInitialized => _bookIdField != null;

        public void Initialize(VisualElement parent, VisualTreeAsset bookHeaderPanelUXML)
        {
            var bookHeaderPanel = bookHeaderPanelUXML.Instantiate();
            parent.Add(bookHeaderPanel);

            _bookIdField = bookHeaderPanel.Q<TextField>();
            _bookIdField.RegisterValueChangedCallback(evt =>
            {
                if (_idProp != null)
                {
                    _idProp.stringValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                }
            });

            Reload();
        }

        public void Bind(SerializedObject serializedObject, string bookPropertyPath)
        {
            if (IsInitialized == false) return;
            _serializedObject = serializedObject;
            if (serializedObject == null || bookPropertyPath == null)
            {
                _bookProp = null;
                _idProp = null;
                _bookIdField.bindingPath = null;
                _bookIdField.value = null;
                return;
            }
            _bookProp = serializedObject.FindProperty(bookPropertyPath);
            _idProp = _bookProp.FindPropertyRelative("_id");
            _bookIdField.value = _idProp.stringValue;
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