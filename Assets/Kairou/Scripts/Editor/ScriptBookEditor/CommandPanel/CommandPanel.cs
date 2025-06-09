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
        VisualElement _header;
        ScrollView _bodyRoot;
        PropertyField _propertyField;

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
                                PropertyFieldToDelayedField(_propertyField);
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
                _propertyField = new PropertyField();
                //_propertyField.RegisterValueChangeCallback(evt => _onCommandChanged?.Invoke());
                _propertyField.style.display = DisplayStyle.Flex;
                _bodyRoot.Add(_propertyField);
                parent.Add(_bodyRoot);
            }

            _onCommandChanged = onCommandChanged;
        }

        static void PropertyFieldToDelayedField(PropertyField propertyField)
        {
            if (propertyField == null) return;
            propertyField.Query<TextField>().ForEach(x => x.isDelayed = true);
            propertyField.Query<FloatField>().ForEach(x => x.isDelayed = true);
            propertyField.Query<IntegerField>().ForEach(x => x.isDelayed = true);
            propertyField.Query<DoubleField>().ForEach(x => x.isDelayed = true);
            propertyField.Query<LongField>().ForEach(x => x.isDelayed = true);
            propertyField.Query<Hash128Field>().ForEach(x => x.isDelayed = true);
            propertyField.Query<UnsignedLongField>().ForEach(x => x.isDelayed = true);
            propertyField.Query<UnsignedIntegerField>().ForEach(x => x.isDelayed = true);
        }

        public void Bind(SerializedObject serializedObject, string commandPropertyPath)
        {
            if (IsInitialized == false) return;
            _propertyField.Unbind();
            if (serializedObject == null || commandPropertyPath == null)
            {
                _propertyField.bindingPath = null;
                _propertyField.style.display = DisplayStyle.None;
                UpdateHeader(null);
                return;
            }

            var commandProp = serializedObject.FindProperty(commandPropertyPath);
            _propertyField.bindingPath = commandProp.propertyPath;
            _propertyField.Bind(serializedObject);
            _propertyField.style.display = DisplayStyle.Flex;
            _propertyField.TrackSerializedObjectValue(serializedObject, _ => _onCommandChanged?.Invoke());

            UpdateHeader(commandProp);
        }

        void UpdateHeader(SerializedProperty commandProp)
        {
            var header = _header.Q<VisualElement>("Header");
            var nameLabel = header.Q<Label>("NameLabel");
            var typeFullNameLabel = header.Q<Label>("TypeFullNameLabel");
            var scriptField = _header.Q<ObjectField>("ScriptField");
            var delayedFieldToggle = header.Q<ToolbarToggle>("DelayedField");

            if (commandProp != null)
            {
                Command command = commandProp.GetObject() as Command;
                Type commandType = command.GetType();
                var commandProfile = CommandDatabase.Load().GetProfile(commandType);

                header.style.backgroundColor = commandProfile.BackgoundColor;
                nameLabel.text = commandProfile.Name;
                nameLabel.style.color = commandProfile.LabelColor;
                typeFullNameLabel.text = commandType.FullName;
                scriptField.value = commandProfile.Script;
                if (delayedFieldToggle.value)
                {
                    // 色々試したけどisDelayedが反映されなかったが、少し遅らせたらちゃんと反映された
                    EditorApplication.delayCall += () =>
                    {
                        PropertyFieldToDelayedField(_propertyField);
                    };
                }
                _header.style.display = DisplayStyle.Flex;
            }
            else
            {
                _header.style.display = DisplayStyle.None;
            }
        }

        public void Reload() {}
    }
}