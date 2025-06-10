using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [CustomPropertyDrawer(typeof(VariableDefinition<>))]
    public class VariableDefinitionDrawer : PropertyDrawer
    {
        const float _labelWidth = 80;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var nameProp = property.FindPropertyRelative("_name");
            var defaultValueProp = property.FindPropertyRelative("_defaultValue");
            var storeProp = property.FindPropertyRelative("_store");
            var storeKeyProp = property.FindPropertyRelative("_storeKey");

            string typeFullName = property.managedReferenceFullTypename;
            var typeArgTypeFullName = typeFullName.AsSpan()[(typeFullName.IndexOf('[') + 2)..typeFullName.LastIndexOf(',')];
            int typeArgNameTypeArgsBracketsStartIndex = typeArgTypeFullName.IndexOf("[[");
            int typeArgNameMainEndIndex = typeArgNameTypeArgsBracketsStartIndex == -1 ? typeArgTypeFullName.Length - 1 : typeArgNameTypeArgsBracketsStartIndex - 1;
            var typeArgNameMain = typeArgTypeFullName[0..typeArgNameMainEndIndex];
            int lastNameSpaceDotindex = typeArgNameMain.LastIndexOf('.');
            var typeArgSimpleName = lastNameSpaceDotindex == -1 ? typeArgTypeFullName : typeArgTypeFullName[(lastNameSpaceDotindex + 1)..];
            var nicifiedTypeName = TypeNameUtil.ConvertToPrimitiveTypeName(typeArgSimpleName.ToString());

            var header = new VisualElement();
            header.SetBorderWidth(1);
            header.SetBorderColor(Color.gray);
            header.Add(new Label(nicifiedTypeName));
            root.Add(header);

            root.Add(new PropertyField(nameProp));
            if (defaultValueProp == null)
            {
                var textField = new TextField();
                textField.label = "Default Value";
                textField.enabledSelf = false;
                textField.value = "default";
                textField.labelElement.style.minWidth = _labelWidth;
                textField.labelElement.style.width = _labelWidth;
                root.Add(textField);
            }
            else
            {
                root.Add(new PropertyField(defaultValueProp));
            }
            
            var storeField = new PropertyField(storeProp);
            root.Add(storeField);
            var storeKeyField = new PropertyField(storeKeyProp);
            root.Add(storeKeyField);

            storeField.RegisterValueChangeCallback(_ =>
            {
                if (storeProp.boolValue)
                {
                    storeKeyField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    storeKeyField.style.display = DisplayStyle.None;
                }
            });

            root.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                root.Query<Label>().Where(x => x.ClassListContains("unity-property-field__label")).ForEach(x =>
                {
                    if (x == null) return;
                    x.style.minWidth = _labelWidth;
                    x.style.width = _labelWidth;
                });
            });

            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var nameProp = property.FindPropertyRelative("_name");
            var defaultValueProp = property.FindPropertyRelative("_defaultValue");
            var storeProp = property.FindPropertyRelative("_store");
            var storeKeyProp = property.FindPropertyRelative("_storeKey");

            var typeRect = new Rect(position.x - 12, position.y, position.width + 12, EditorGUIUtility.singleLineHeight);
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            string typeFullName = property.managedReferenceFullTypename;
            var typeArgTypeFullName = typeFullName.AsSpan()[(typeFullName.IndexOf('[') + 2)..typeFullName.LastIndexOf(',')];
            int typeArgNameTypeArgsBracketsStartIndex = typeArgTypeFullName.IndexOf("[[");
            int typeArgNameMainEndIndex = typeArgNameTypeArgsBracketsStartIndex == -1 ? typeArgTypeFullName.Length - 1 : typeArgNameTypeArgsBracketsStartIndex - 1;
            var typeArgNameMain = typeArgTypeFullName[0..typeArgNameMainEndIndex];
            int lastNameSpaceDotindex = typeArgNameMain.LastIndexOf('.');
            var typeArgSimpleName = lastNameSpaceDotindex == -1 ? typeArgTypeFullName : typeArgTypeFullName[(lastNameSpaceDotindex + 1)..];

            GUI.Box(typeRect, $"{TypeNameUtil.ConvertToPrimitiveTypeName(typeArgSimpleName.ToString())}", GUIStyles.Badge);

            using var _ = new LabelWidthScope(_labelWidth);

            var nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(nameRect, nameProp);

            if (defaultValueProp == null)
            {
                var defaultValueRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                position.yMin += defaultValueRect.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.LabelField(defaultValueRect, "Default Value Non Serializable");
            }
            else
            {
                var defaultValueRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(defaultValueProp, true));
                position.yMin += defaultValueRect.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(defaultValueRect, defaultValueProp, true);
            }

            var storeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(storeRect, storeProp);

            if (storeProp.boolValue)
            {
                var storeKeyRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(storeKeyRect, storeKeyProp);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;

            var defaultValueProp = property.FindPropertyRelative("_defaultValue");
            var storeProp = property.FindPropertyRelative("_store");

            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            if (defaultValueProp == null)
            {
                height += EditorGUIUtility.singleLineHeight;
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                height += EditorGUI.GetPropertyHeight(defaultValueProp, true);
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            if (storeProp.boolValue)
            {
                height += EditorGUIUtility.singleLineHeight;
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }
}