using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Profiling;
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

        public void Initialize(VisualElement parent, VisualTreeAsset variablePanelUXML)
        {
            var variablePanel = variablePanelUXML.Instantiate();
            parent.Add(variablePanel);

            var bookTab = variablePanel.Q<Tab>("BookTab");
            _bookListView = bookTab.Q<ListView>();
            _bookListView.onAdd = _ =>
            {
                var menu = new GenericMenu();
                foreach (var type in VariableTypeDictionary.Dic.Keys)
                {
                    menu.AddItem(new GUIContent(TypeNameUtil.ConvertToPrimitiveTypeName(type.Name)), false, () =>
                    {
                        var variable = (VariableDefinition)Activator.CreateInstance(typeof(VariableDefinition<>).MakeGenericType(type));
                        Undo.RecordObject(_serializedObject.targetObject, "Add Variable");
                        (_bookListView.itemsSource as List<VariableDefinition>).Add(variable);
                        EditorUtility.SetDirty(_serializedObject.targetObject);
                        _bookListView.RefreshItems();
                    });
                }
                menu.ShowAsContext();
            };
            _bookListView.onRemove = _ =>
            {
                var list = _bookListView.itemsSource as List<VariableDefinition>;
                if (0 <= _bookListView.selectedIndex && _bookListView.selectedIndex < list.Count)
                {
                    Undo.RecordObject(_serializedObject.targetObject, "Remove Variable");
                    list.RemoveAt(_bookListView.selectedIndex);
                    EditorUtility.SetDirty(_serializedObject.targetObject);
                    _bookListView.RefreshItems();
                }
            };
            _bookListView.makeItem = () =>
            {
                return new SerializableAnythingField<VariableDefinition>();
            };
            _bookListView.bindItem = (element, index) =>
            {
                var field = element as SerializableAnythingField<VariableDefinition>;
                field.Attach((VariableDefinition)_bookListView.itemsSource[index], obj => _serializedObject.UpdateIfRequiredOrScript());
            };
            _bookListView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                var list = _bookListView.itemsSource as List<VariableDefinition>;
                var to = list[toIndex];
                list.RemoveAt(toIndex);
                list.Insert(fromIndex, to);
                Undo.RecordObject(_serializedObject.targetObject, "Move Variable");
                list.RemoveAt(fromIndex);
                list.Insert(toIndex, to);
                EditorUtility.SetDirty(_serializedObject.targetObject);
                _bookListView.selectedIndex = toIndex;
            };

            var pageTab = variablePanel.Q<Tab>("PageTab");
            _pageListView = pageTab.Q<ListView>();
            _pageListView.onAdd = _ =>
            {
                var menu = new GenericMenu();
                foreach (var type in VariableTypeDictionary.Dic.Keys)
                {
                    menu.AddItem(new GUIContent(TypeNameUtil.ConvertToPrimitiveTypeName(type.Name)), false, () =>
                    {
                        var variable = (VariableDefinition)Activator.CreateInstance(typeof(VariableDefinition<>).MakeGenericType(type));
                        Undo.RecordObject(_serializedObject.targetObject, "Add Variable");
                        (_pageListView.itemsSource as List<VariableDefinition>).Add(variable);
                        EditorUtility.SetDirty(_serializedObject.targetObject);
                        _pageListView.RefreshItems();
                    });
                }
                menu.ShowAsContext();
            };
            _pageListView.onRemove = _ =>
            {
                var list = _pageListView.itemsSource as List<VariableDefinition>;
                if (0 <= _pageListView.selectedIndex && _pageListView.selectedIndex < list.Count)
                {
                    Undo.RecordObject(_serializedObject.targetObject, "Remove Variable");
                    list.RemoveAt(_pageListView.selectedIndex);
                    EditorUtility.SetDirty(_serializedObject.targetObject);
                    _pageListView.RefreshItems();
                }
            };
            _pageListView.makeItem = () =>
            {
                return new SerializableAnythingField<VariableDefinition>();
            };
            _pageListView.bindItem = (element, index) =>
            {
                var field = element as SerializableAnythingField<VariableDefinition>;
                field.Attach((VariableDefinition)_pageListView.itemsSource[index], obj => _serializedObject.UpdateIfRequiredOrScript());
            };
            _pageListView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                var list = _pageListView.itemsSource as List<VariableDefinition>;
                var to = list[toIndex];
                list.RemoveAt(toIndex);
                list.Insert(fromIndex, to);
                Undo.RecordObject(_serializedObject.targetObject, "Move Variable");
                list.RemoveAt(fromIndex);
                list.Insert(toIndex, to);
                EditorUtility.SetDirty(_serializedObject.targetObject);
                _pageListView.selectedIndex = toIndex;
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
                _bookListView.bindingPath = null;
                return;
            }

            var bookVariablesProp = serializedObject.FindProperty($"{bookPropertyPath}._variables");
            _bookListView.itemsSource = bookVariablesProp.GetObject() as IList;
        }

        public void BindPageVariable(string pagePropertyPath)
        {
            if (IsInitialized == false) return;
            if (pagePropertyPath == null)
            {
                _pageListView.bindingPath = null;
                return;
            }
            var pageVariablesProp = _serializedObject.FindProperty($"{pagePropertyPath}._variables");
            _pageListView.itemsSource = pageVariablesProp.GetObject() as IList;
        }

        public void Reload() {}

        public void OnUndoRedoPerformed()
        {
            if (IsInitialized == false) return;
            _bookListView.RefreshItems();
            _pageListView.RefreshItems();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(VariablePanel)} is not initialized.");
        }
    }
}