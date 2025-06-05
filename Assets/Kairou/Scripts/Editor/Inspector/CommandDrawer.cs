using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [CustomPropertyDrawer(typeof(Command))]
    public class CommandDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Command command = property.GetObject() as Command;
            Type commandType = command.GetType();

            var commandProfile = CommandDatabase.Load().GetProfile(commandType);

            var root = new VisualElement();

            var labelBox = new VisualElement();
            labelBox.style.flexDirection = FlexDirection.Row;
            labelBox.style.backgroundColor = commandProfile.SummaryBackgoundColor;
            labelBox.style.borderBottomWidth = 1;
            labelBox.style.borderBottomColor = Color.black;

            var label = new Label(commandProfile.Name);
            label.style.color = commandProfile.NameColor;

            // var typeFullNameLabel = new Label(commandType.FullName);
            // typeFullNameLabel.style.color = new Color(1, 1, 1, 0.2f);

            labelBox.Add(label);
            // labelBox.Add(new VisualElement() { style = { flexGrow = 1 } });
            // labelBox.Add(typeFullNameLabel);

            root.Add(labelBox);

            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();

            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && SerializedProperty.EqualContents(iterator, endProperty) == false)
            {
                var field = new PropertyField(iterator.Copy());
                field.Bind(iterator.serializedObject);
                root.Add(field);
                enterChildren = false;
            }

            return root;
        }
    }
}