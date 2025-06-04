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
        [SerializeField] RestorableBookHolder _bookHolder = new();
        [SerializeField] int _pageIndex;
        bool ExistsPage => _bookHolder.HasValidBook && _bookHolder.Book.Pages.HasElementAt(_pageIndex);

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
                if (_bookHolder.Book == null) return;

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

        public void SetTarget(BookId bookId, int pageIndex)
        {
            _bookHolder.Reset(bookId);
            _pageIndex = pageIndex;
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            ThrowIfNotInitialized();
            if (_bookHolder.HasValidBook)
            {
                _serializedObject = new SerializedObject(_bookHolder.Owner);
                _bookVariablesProp = _serializedObject
                    .FindProperty(_bookHolder.BookPropertyPath)
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
                    .FindProperty(_bookHolder.BookPropertyPath)
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

        public void OnProjectOrHierarchyChanged()
        {
            if (_bookHolder.RestoreObjectIfNull())
            {
                Reload();
            }
        }

        public void OnUndoRedoPerformed()
        {
            if (IsInitialized == false) return;
            _bookListView.Rebuild();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(VariablePanel)} is not initialized.");
        }
    }
}