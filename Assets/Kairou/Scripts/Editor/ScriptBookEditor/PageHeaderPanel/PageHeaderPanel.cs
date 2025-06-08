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

        public void Bind(SerializedObject serializedObject, string pagePropertyPath)
        {
            if (IsInitialized == false) return;
            if (serializedObject == null || pagePropertyPath == null)
            {
                _textField.bindingPath = null;
                _textField.Unbind();
                _textField.value = null;
                return;
            }
            _textField.bindingPath = $"{pagePropertyPath}._id";
            _textField.Bind(serializedObject);
        }

        public void Reload() {}
    }
}