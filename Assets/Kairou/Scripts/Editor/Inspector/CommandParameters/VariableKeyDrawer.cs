using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [CustomPropertyDrawer(typeof(VariableKey<>))]
    public class VariableKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var key = property.GetObject() as VariableKey;

            var variableNameProp = property.FindPropertyRelative("_variableName");
            var targetScopeProp = property.FindPropertyRelative("_targetScope");

            Rect propertyRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(property, label, true));
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
                            return variable.TargetType == key.TargetType;
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
        // public override VisualElement CreatePropertyGUI(SerializedProperty property)
        // {
        //     var nameProp = property.FindPropertyRelative("_name");
        //     var targetScopeProp = property.FindPropertyRelative("_targetScope");

        //     var parent = new VisualElement();
        //     parent.AddToClassList(BaseField<object>.alignedFieldUssClassName);
        //     // parent.AddToClassList("unity-base-field");
        //     parent.style.flexDirection = FlexDirection.Row;
        //     var label = new Label(property.displayName);
        //     label.AddToClassList("unity-base-field");
        //     label.AddToClassList("unity-base-field__label");
        //     parent.Add(label);
        //     var fields = new VisualElement();
        //     fields.AddToClassList("unity-base-field__input");
        //     // fields.AddToClassList("unity-property-field__input");
        //     fields.style.flexDirection = FlexDirection.Row;
        //     var targetScopeField = new PropertyField(targetScopeProp, "");
        //     targetScopeField.style.minWidth = 60;
        //     targetScopeField.style.maxWidth = 60;
        //     targetScopeField.style.flexShrink = 0;
        //     // targetScopeField.AddToClassList("unity-base-text-field__input");
        //     fields.Add(targetScopeField);
        //     var nameField = new PropertyField(nameProp, "");
        //     nameField.style.flexGrow = 1;
        //     fields.Add(nameField);
        //     fields.Add(new Button() { text = "▼" } );
        //     parent.Add(fields);
        //     return parent;
        // }
    }
}