using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    [Serializable]
    public class VariablePanel
    {
        ListView _bookListView;
        ListView _pageListView;

        bool IsInitialized => _bookListView != null;

        SerializedObject _serializedObject;
        SerializedProperty _bookVariablesProp;
        SerializedProperty _pageVariablesProp;

        public void Initialize(VisualElement parent, VisualTreeAsset variablePanelUXML, Action onValueChanged)
        {
            var variablePanel = variablePanelUXML.Instantiate();
            parent.Add(variablePanel);

            var bookTab = variablePanel.Q<Tab>("BookTab");
            _bookListView = bookTab.Q<ListView>();
            _bookListView.makeItem = () =>
            {
                var item = new PropertyField();
                item.RegisterValueChangeCallback(_ => onValueChanged?.Invoke());
                return item;
            };
            _bookListView.onAdd = _ =>
            {
                var menu = new GenericMenu();
                foreach (var type in VariableTypeDictionary.Dic.Keys)
                {
                    menu.AddItem(new GUIContent(TypeNameUtil.ConvertToPrimitiveTypeName(type.Name)), false, () =>
                    {
                        var variable = (VariableDefinition)Activator.CreateInstance(typeof(VariableDefinition<>).MakeGenericType(type));
                        _serializedObject.UpdateIfRequiredOrScript();
                        _bookVariablesProp.InsertArrayElementAtIndex(_bookVariablesProp.arraySize);
                        _bookVariablesProp.GetArrayElementAtIndex(_bookVariablesProp.arraySize - 1).managedReferenceValue = variable;
                        _serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            };

            var pageTab = variablePanel.Q<Tab>("PageTab");
            _pageListView = pageTab.Q<ListView>();
            _pageListView.makeItem = () =>
            {
                var item = new PropertyField();
                item.RegisterValueChangeCallback(_ => onValueChanged?.Invoke());
                return item;
            };
            _pageListView.onAdd = _ =>
            {
                var menu = new GenericMenu();
                foreach (var type in VariableTypeDictionary.Dic.Keys)
                {
                    menu.AddItem(new GUIContent(TypeNameUtil.ConvertToPrimitiveTypeName(type.Name)), false, () =>
                    {
                        var variable = (VariableDefinition)Activator.CreateInstance(typeof(VariableDefinition<>).MakeGenericType(type));
                        _serializedObject.UpdateIfRequiredOrScript();
                        _pageVariablesProp.InsertArrayElementAtIndex(_pageVariablesProp.arraySize);
                        _pageVariablesProp.GetArrayElementAtIndex(_pageVariablesProp.arraySize - 1).managedReferenceValue = variable;
                        _serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            };

            Reload();
        }

        public void BindBookVariable(SerializedObject serializedObject, string bookPropertyPath)
        {
            if (IsInitialized == false) return;
            _bookListView.Unbind();

            _serializedObject = serializedObject;
            if (_serializedObject == null || bookPropertyPath == null)
            {
                _bookVariablesProp = null;
                _bookListView.bindingPath = null;
                return;
            }

            _bookVariablesProp = serializedObject.FindProperty($"{bookPropertyPath}._variables");
            _bookListView.bindingPath = _bookVariablesProp.propertyPath;
            _bookListView.Bind(_serializedObject);
        }

        public void BindPageVariable(string pagePropertyPath)
        {
            if (IsInitialized == false) return;
            _pageListView.Unbind();
            if (pagePropertyPath == null)
            {
                _pageVariablesProp = null;
                _pageListView.bindingPath = null;
                return;
            }
            _pageVariablesProp = _serializedObject.FindProperty($"{pagePropertyPath}._variables");
            _pageListView.bindingPath = _pageVariablesProp.propertyPath;
            _pageListView.Bind(_serializedObject);
        }

        public void RebindBookVariable()
        {
            BindBookVariable(_serializedObject, _bookVariablesProp.propertyPath.Substring(0, _bookVariablesProp.propertyPath.LastIndexOf('.')));
        }

        public void RebindPageVariable()
        {
            BindPageVariable(_pageVariablesProp.propertyPath.Substring(0, _pageVariablesProp.propertyPath.LastIndexOf('.')));
        }

        public void Reload() {}

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(VariablePanel)} is not initialized.");
        }
    }
}