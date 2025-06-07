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

        VisualElement _header;
        ScrollView _bodyRoot;

        Action _onCommandChanged;

        bool IsInitialized => _bodyRoot != null;

        public void Initialize(VisualElement parent, Action onCommandChanged)
        {
            /* header */ {
                _header = new VisualElement();
                _header.name = "Header";

                /* header_header header_body */ {
                    var header_header = new VisualElement();
                    header_header.style.flexDirection = FlexDirection.Row;
                    header_header.style.borderBottomWidth = 1;
                    header_header.style.borderBottomColor = Color.black;

                    var header_body = new VisualElement();
                    header_body.style.display = DisplayStyle.None;
                    header_body.style.borderBottomWidth = 1;
                    header_body.style.borderBottomColor = Color.black;

                    /* header_header elements */ {
                        var nameLabel = new Label();
                        nameLabel.name = "NameLabel";

                        var typeFullNameLabel = new Label();
                        typeFullNameLabel.name = "TypeFullNameLabel";
                        typeFullNameLabel.style.color = new Color(1, 1, 1, 0.2f);
                        typeFullNameLabel.style.display = DisplayStyle.None;
                        typeFullNameLabel.style.flexShrink = 1;

                        var infoToggle = new ToolbarToggle();
                        infoToggle.name = "InfoToggle";
                        infoToggle.style.height = 15;
                        infoToggle.Add(new Image() { image = GUICommon.InfoIcon });
                        infoToggle.RegisterValueChangedCallback(evt =>
                        {
                            typeFullNameLabel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                            header_body.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                        });

                        var delayedFieldToggle = new ToolbarToggle();
                        delayedFieldToggle.name = "DelayedField";
                        delayedFieldToggle.text = "DelayedField";
                        delayedFieldToggle.viewDataKey = "DelayedField";
                        delayedFieldToggle.style.height = 15;
                        delayedFieldToggle.RegisterValueChangedCallback(evt =>
                        {
                            if (evt.newValue)
                            {
                                PropertyFieldToDelayedField(_bodyRoot.Query<PropertyField>().First());
                            }
                        });

                        header_header.Add(nameLabel);
                        header_header.Add(new VisualElement() { style = { flexGrow = 1 } });
                        header_header.Add(typeFullNameLabel);
                        header_header.Add(infoToggle);
                        header_header.Add(delayedFieldToggle);
                    }

                    /* header_body elements */ {
                        var scriptField = new ObjectField("Script");
                        scriptField.name = "ScriptField";
                        scriptField.enabledSelf = false;

                        header_body.Add(scriptField);
                    }

                    _header.Add(header_header);
                    _header.Add(header_body);
                }
                parent.Add(_header);
            }

            /* body*/ {
                _bodyRoot = new ScrollView() { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
                _bodyRoot.style.flexGrow = 1;
                parent.Add(_bodyRoot);
            }

            _onCommandChanged = onCommandChanged;
            Reload();
        }

        static void PropertyFieldToDelayedField(PropertyField propertyField)
        {
            if (propertyField == null) return;
            propertyField.Query<TextField>().ForEach(x => x.isDelayed = true);
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
            if (IsInitialized == false) return;

            _bodyRoot.Clear();

            if (ExistsTargetCommand)
            {
                var serializedObject = new SerializedObject(_bookHolder.Owner);
                var bookProp = serializedObject.FindProperty(_bookHolder.BookPropertyPath);
                var commandProp = bookProp
                    .FindPropertyRelative("_pages")
                    .GetArrayElementAtIndex(_pageIndex)
                    .FindPropertyRelative("_commands")
                    .GetArrayElementAtIndex(_commandIndex);

                Command command = commandProp.GetObject() as Command;
                Type commandType = command.GetType();
                var commandProfile = CommandDatabase.Load().GetProfile(commandType);

                var header = _header.Q<VisualElement>("Header");
                header.style.backgroundColor = commandProfile.BackgoundColor;

                var nameLabel = header.Q<Label>("NameLabel");
                nameLabel.text = commandProfile.Name;
                nameLabel.style.color = commandProfile.LabelColor;

                var typeFullNameLabel = header.Q<Label>("TypeFullNameLabel");
                typeFullNameLabel.text = commandType.FullName;

                var scriptField = _header.Q<ObjectField>("ScriptField");
                scriptField.value = commandProfile.Script;

                int typeNameStartIndex = commandProp.type.IndexOf('<') + 1;
                int typeNameEndIndex = commandProp.type.LastIndexOf('>');
                string typeName = commandProp.type[typeNameStartIndex..typeNameEndIndex];
                var propertyField = new PropertyField(commandProp, typeName);
                _bodyRoot.Add(propertyField);
                propertyField.BindProperty(commandProp);
                propertyField.TrackSerializedObjectValue(commandProp.serializedObject, _ => _onCommandChanged?.Invoke());
                propertyField.style.display = DisplayStyle.Flex;

                var delayedFieldToggle = header.Q<ToolbarToggle>("DelayedField");
                if (delayedFieldToggle.value)
                {
                    // 色々試したけどisDelayedが反映されなかったが、少し遅らせたらちゃんと反映された
                    EditorApplication.delayCall += () =>
                    {
                        PropertyFieldToDelayedField(propertyField);
                    };
                }

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
            if (IsInitialized == false) return;
            Reload();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(CommandPanel)} is not initialized.");
        }
    }
}