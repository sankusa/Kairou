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

        public void SetTarget(string pagePropertyPath)
        {
            if (IsInitialized)
            {
                _textField.bindingPath = $"{pagePropertyPath}._id";
            }
        }

        public void Reload() {}
    }
}