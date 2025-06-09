using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [CustomPropertyDrawer(typeof(VariableKey<>))]
    public class VariableKeyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var key = property.GetObject() as VariableKey;
            return VariableKeyDrawerSharedLogic.CreatePropertyGUI(property, typeof(VariableKey), key.TargetType, key.IsValidDefinition, (command) => key.FindDefinition(command));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var key = property.GetObject() as VariableKey;
            VariableKeyDrawerSharedLogic.OnGUI(position, property, label, typeof(VariableKey), key.TargetType, key.IsValidDefinition, (command) => key.FindDefinition(command));
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return VariableKeyDrawerSharedLogic.GetPropertyHeight(property, label);
        }
    }
}