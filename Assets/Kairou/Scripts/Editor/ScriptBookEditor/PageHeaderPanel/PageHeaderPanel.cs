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
        SerializedObject _serializedObject;
        SerializedProperty _pageProp;
        SerializedProperty _idProp;
        bool IsInitialized => _textField != null;

        public void Initialize(VisualElement parent, VisualTreeAsset pageHeaderPanelUXML, Action onPageChanged)
        {
            var pageHeaderPanel = pageHeaderPanelUXML.Instantiate();
            parent.Add(pageHeaderPanel);

            _textField = pageHeaderPanel.Q<TextField>();
            _textField.RegisterValueChangedCallbackWithoutOnRegister(evt =>
            {
                if (_idProp != null)
                {
                    _idProp.stringValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                }
                onPageChanged?.Invoke();
            });

            Reload();
        }

        public void Bind(SerializedObject serializedObject, string pagePropertyPath)
        {
            if (IsInitialized == false) return;
            _serializedObject = serializedObject;
            if (_serializedObject == null || pagePropertyPath == null)
            {
                _pageProp = null;
                _idProp = null;
                _textField.value = null;
                return;
            }
            _pageProp = serializedObject.FindProperty(pagePropertyPath);
            _idProp = _pageProp.FindPropertyRelative("_id");
            _textField.value = _idProp.stringValue;
        }

        public void Reload() {}
    }
}