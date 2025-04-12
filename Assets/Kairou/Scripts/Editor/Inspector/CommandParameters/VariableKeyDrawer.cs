using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou
{
    [CustomPropertyDrawer(typeof(VariableKey<>))]
    public class VariableKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var variableNameProp = property.FindPropertyRelative("_variableName");
            var targetScopeProp = property.FindPropertyRelative("_targetScope");

            Rect propertyRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(property, label, true));
            position.yMin += propertyRect.height + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(propertyRect, property, true);
            if (GUI.Button(position, "Select Variable Name"))
            {
                var command = CommandUtilForEditor.GetContainingCommand(property);
                if (command != null)
                {
                    var menu = new GenericMenu();
                    if (targetScopeProp.enumValueIndex == (int)TargetVariableScope.None || targetScopeProp.enumValueIndex == (int)TargetVariableScope.Page)
                    {
                        foreach (var variable in command.ParentPage?.Variables)
                        {
                            menu.AddItem(new GUIContent("Page/" + variable.Name), false, () =>
                            {
                                variableNameProp.stringValue = variable.Name;
                                property.serializedObject.ApplyModifiedProperties();
                            });
                        }
                    }
                    if (targetScopeProp.enumValueIndex == (int)TargetVariableScope.None || targetScopeProp.enumValueIndex == (int)TargetVariableScope.Book)
                    {
                        foreach (var variable in command.ParentPage?.ParentBook?.Variables)
                        {
                            menu.AddItem(new GUIContent("Book/" + variable.Name), false, () =>
                            {
                                variableNameProp.stringValue = variable.Name;
                                property.serializedObject.ApplyModifiedProperties();
                            });
                        }
                    }
                    menu.ShowAsContext();
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            height += EditorGUI.GetPropertyHeight(property, label, true);
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight;
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