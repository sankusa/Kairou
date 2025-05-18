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
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var key = property.GetObject() as VariableValueGetterKey;

            var variableNameProp = property.FindPropertyRelative("_variableName");
            var targetScopeProp = property.FindPropertyRelative("_targetScope");

            var propertyRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(property, label, true));
            position.yMin += propertyRect.height + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(propertyRect, property, true);
            if (property.isExpanded)
            {
                var variableNameSelectButtonRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                variableNameSelectButtonRect.xMin += (EditorGUI.indentLevel + 1) * 15f;
                if (GUI.Button(variableNameSelectButtonRect, "Select Variable Name"))
                {
                    var command = CommandUtilForEditor.GetContainingCommand(property);
                    new VariableSelectGenericMenu(
                        command,
                        (TargetVariableScope)targetScopeProp.enumValueIndex,
                        variable =>
                        {
                            return TypeConverterCache.CanConvert(variable.TargetType, key.TargetType);
                        },
                        variable =>
                        {
                            variableNameProp.stringValue = variable.Name;
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    ).ShowAsContext();
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            height += EditorGUI.GetPropertyHeight(property, label, true);
            if (property.isExpanded)
            {
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUIUtility.singleLineHeight;
            }
            return height;
        }
    }
}