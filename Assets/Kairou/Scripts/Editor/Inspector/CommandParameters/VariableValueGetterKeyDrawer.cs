using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [CustomPropertyDrawer(typeof(VariableValueGetterKey<>))]
    public class VariableValueGetterKeyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var key = property.GetObject() as VariableValueGetterKey;
            return VariableKeyDrawerSharedLogic.CreatePropertyGUI(property, typeof(VariableKey), key.TargetType, key.IsValidDefinition, (command) => key.FindDefinition(command));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var key = property.GetObject() as VariableValueGetterKey;
            VariableKeyDrawerSharedLogic.OnGUI(position, property, label, typeof(VariableValueGetterKey), key.TargetType, key.IsValidDefinition, (command) => key.FindDefinition(command));
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return VariableKeyDrawerSharedLogic.GetPropertyHeight(property, label);
        }
    }
}