using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [CustomPropertyDrawer(typeof(VariableValueSetterKey<>))]
    public class VariableValueSetterKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var key = property.GetObject() as VariableValueSetterKey;
            VariableKeyDrawerSharedLogic.OnGUI(position, property, label, typeof(VariableValueSetterKey), key.TargetType, key.IsValidDefinition, (command) => key.FindDefinition(command));
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return VariableKeyDrawerSharedLogic.GetPropertyHeight(property, label);
        }
    }
}