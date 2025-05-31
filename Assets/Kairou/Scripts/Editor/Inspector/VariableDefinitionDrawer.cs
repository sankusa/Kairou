using System;
using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    [CustomPropertyDrawer(typeof(VariableDefinition<>))]
    public class VariableDefinitionDrawer : PropertyDrawer
    {
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

            using var _ = new LabelWidthScope(80);

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