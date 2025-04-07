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
        [SerializeField] Object _scriptBookOwnerObject;
        [SerializeField] int _pageIndex;
        [SerializeField] int _commandIndex;
        IScriptBookOwner ScriptBookOwner => (IScriptBookOwner)_scriptBookOwnerObject;
        bool ExistsTargetCommand => ScriptBookOwner != null
            && ScriptBookOwner.ScriptBook.ExistsCommandAt(_pageIndex, _commandIndex);

        VisualElement _parent;

        Action _onCommandChanged;

        bool IsInitialized => _parent != null;

        public void Initialize(VisualElement parent, Action onCommandChanged)
        {
            _parent = new ScrollView();
            parent.Add(_parent);
            _onCommandChanged = onCommandChanged;
            Reload();
        }

        public void SetTarget(IScriptBookOwner scriptBookOwner, int pageIndex, int commandIndex)
        {
            _scriptBookOwnerObject = scriptBookOwner?.AsObject();
            _pageIndex = pageIndex;
            _commandIndex = commandIndex;
            if (IsInitialized) Reload();
        }

        public void Reload() {
            ThrowIfNotInitialized();

            _parent.Clear();

            if (ExistsTargetCommand)
            {
                var serializedObject = new SerializedObject(_scriptBookOwnerObject);
                var scriptBookProp = serializedObject.FindProperty(ScriptBookOwner.ScriptBookPropertyPath);
                var commandProp = scriptBookProp
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
            }
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(CommandPanel)} is not initialized.");
        }
    }
}