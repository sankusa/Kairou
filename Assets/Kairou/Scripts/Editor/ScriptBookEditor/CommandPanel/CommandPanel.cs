using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    [Serializable]
    public class CommandPanel
    {
        [SerializeField] RestorableBookHolder _bookHolder = new();
        [SerializeField] int _pageIndex;
        [SerializeField] int _commandIndex;
        bool ExistsTargetCommand => _bookHolder.HasValidBook && _bookHolder.Book.ExistsCommandAt(_pageIndex, _commandIndex);

        VisualElement _parent;

        Action _onCommandChanged;

        bool IsInitialized => _parent != null;

        public void Initialize(VisualElement parent, Action onCommandChanged)
        {
            var scrollView = new ScrollView() { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scrollView.style.marginRight = 4;
            _parent = scrollView;
            parent.Add(_parent);
            _onCommandChanged = onCommandChanged;
            Reload();
        }

        public void SetTarget(BookId bookId, int pageIndex, int commandIndex)
        {
            _bookHolder.Reset(bookId);
            _pageIndex = pageIndex;
            _commandIndex = commandIndex;
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            ThrowIfNotInitialized();

            _parent.Clear();

            if (ExistsTargetCommand)
            {
                var serializedObject = new SerializedObject(_bookHolder.Owner);
                var bookProp = serializedObject.FindProperty(_bookHolder.BookPropertyPath);
                var commandProp = bookProp
                    .FindPropertyRelative("_pages")
                    .GetArrayElementAtIndex(_pageIndex)
                    .FindPropertyRelative("_commands")
                    .GetArrayElementAtIndex(_commandIndex);

                int typeNameStartIndex = commandProp.type.IndexOf('<') + 1;
                int typeNameEndIndex = commandProp.type.LastIndexOf('>');
                string typeName = commandProp.type[typeNameStartIndex..typeNameEndIndex];
                var propertyField = new PropertyField(commandProp, typeName);
                _parent.Add(propertyField);
                propertyField.BindProperty(commandProp);
                propertyField.TrackSerializedObjectValue(commandProp.serializedObject, _ => _onCommandChanged?.Invoke());
                propertyField.style.display = DisplayStyle.Flex;

                // var container = new IMGUIContainer(() =>
                // {
                //     if (serializedObject.targetObject == null) return;
                //     serializedObject.UpdateIfRequiredOrScript();
                //     // using var _ = new LabelWidthScope(120);
                //     EditorGUI.BeginChangeCheck();
                //     EditorGUILayout.PropertyField(commandProp, new GUIContent(typeName), true);
                //     serializedObject.ApplyModifiedProperties();
                //     if (EditorGUI.EndChangeCheck())
                //     {
                //         _onCommandChanged?.Invoke();
                //     }
                // });

                // _parent.Add(container);
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
            ThrowIfNotInitialized();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(CommandPanel)} is not initialized.");
        }
    }
}