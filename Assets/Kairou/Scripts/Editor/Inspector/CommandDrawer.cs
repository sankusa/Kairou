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
            var root = new VisualElement();

            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();

            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && SerializedProperty.EqualContents(iterator, endProperty) == false)
            {
                if (iterator.name == "_enable") continue;
                var field = new PropertyField(iterator.Copy());
                field.Bind(iterator.serializedObject);
                root.Add(field);
                enterChildren = false;
            }

            return root;
        }
    }
}