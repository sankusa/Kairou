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
            var commandInfo = commandType.GetCustomAttribute<CommandInfoAttribute>();

            var (commandSetting, commandCategory) = CommandDatabase.Load().FindSetting(commandType);

            var root = new VisualElement();

            var labelBox = new Box();
            if (commandCategory != null) labelBox.style.backgroundColor = commandCategory.SummaryBackgroundColor;
            labelBox.style.borderBottomWidth = 1;
            labelBox.style.borderBottomColor = Color.black;

            var label = new Label(commandInfo == null ? commandType.Name : commandInfo.CommandName);
            if (commandCategory != null) label.style.color = commandCategory.CommandNameColor;

            labelBox.Add(label);

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