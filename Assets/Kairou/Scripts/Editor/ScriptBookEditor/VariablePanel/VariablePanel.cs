using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    public class VariablePanel
    {
        [SerializeField] Object _scriptBookOwnerObject;
        [SerializeField] int _pageIndex;
        IScriptBookOwner ScriptBookOwner => _scriptBookOwnerObject as IScriptBookOwner;
        bool ExistsPage => _scriptBookOwnerObject != null && ScriptBookOwner.ScriptBook.Pages.HasElementAt(_pageIndex);

        ListView _bookListView;
        ListView _pageListView;

        bool IsInitialized => _bookListView != null;

        SerializedObject _serializedObject;
        SerializedProperty _bookVariablesProp;
        SerializedProperty _pageVariablesProp;

        public void Initialize(VisualElement parent, VisualTreeAsset variablePanelUXML)
        {
            var variablePanel = variablePanelUXML.Instantiate();
            parent.Add(variablePanel);

            var bookTab = variablePanel.Q<Tab>("BookTab");
            _bookListView = bookTab.Q<ListView>();
            _bookListView.onAdd = _ =>
            {
                if (ScriptBookOwner == null) return;

                var menu = new GenericMenu();
                foreach (var type in VariableTypeDictionary.Dic.Keys)
                {
                    menu.AddItem(new GUIContent(TypeNameUtil.ConvertToPrimitiveTypeName(type.Name)), false, () =>
                    {
                        var variable = (VariableDefinition)Activator.CreateInstance(typeof(VariableDefinition<>).MakeGenericType(type));
                        _serializedObject.Update();
                        _bookVariablesProp.InsertArrayElementAtIndex(_bookVariablesProp.arraySize);
                        _bookVariablesProp.GetArrayElementAtIndex(_bookVariablesProp.arraySize - 1).managedReferenceValue = variable;
                        _serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            };

            var pageTab = variablePanel.Q<Tab>("PageTab");
            _pageListView = pageTab.Q<ListView>();
            _pageListView.onAdd = _ =>
            {
                if (ExistsPage == false) return;

                var menu = new GenericMenu();
                foreach (var type in VariableTypeDictionary.Dic.Keys)
                {
                    menu.AddItem(new GUIContent(TypeNameUtil.ConvertToPrimitiveTypeName(type.Name)), false, () =>
                    {
                        var variable = (VariableDefinition)Activator.CreateInstance(typeof(VariableDefinition<>).MakeGenericType(type));
                        _serializedObject.Update();
                        _pageVariablesProp.InsertArrayElementAtIndex(_pageVariablesProp.arraySize);
                        _pageVariablesProp.GetArrayElementAtIndex(_pageVariablesProp.arraySize - 1).managedReferenceValue = variable;
                        _serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            };

            Reload();
        }

        public void SetTarget(IScriptBookOwner scriptBookOwner, int pageIndex)
        {
            _scriptBookOwnerObject = scriptBookOwner?.AsObject();
            _pageIndex = pageIndex;
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            ThrowIfNotInitialized();
            if (_scriptBookOwnerObject != null)
            {
                _serializedObject = new SerializedObject(_scriptBookOwnerObject);
                _bookVariablesProp = _serializedObject
                    .FindProperty(ScriptBookOwner.ScriptBookPropertyPath)
                    .FindPropertyRelative("_variables");
                _bookListView.BindProperty(_bookVariablesProp);
            }
            else
            {
                _serializedObject = null;
                _bookVariablesProp = null;
                _bookListView.Unbind();

            }

            if (ExistsPage)
            {
                _pageVariablesProp = _serializedObject
                    .FindProperty(ScriptBookOwner.ScriptBookPropertyPath)
                    .FindPropertyRelative("_pages")
                    .GetArrayElementAtIndex(_pageIndex)
                    .FindPropertyRelative("_variables");
                _pageListView.BindProperty(_pageVariablesProp);
            }
            else
            {
                _pageVariablesProp = null;
                _pageListView.Unbind();
            }
        }

        public void OnUndoRedoPerformed()
        {
            ThrowIfNotInitialized();
            _bookListView.Rebuild();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(VariablePanel)} is not initialized.");
        }
    }
}