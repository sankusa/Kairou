using System;
using UnityEditor;
using UnityEngine;

namespace Kairou
{
    [CustomPropertyDrawer(typeof(VariableDefinition<>))]
    public class VariableDefinitionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var nameProp = property.FindPropertyRelative("_name");
            var variableProp = property.FindPropertyRelative("_defaultValue");
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

            GUI.Box(typeRect, $"{TypeNameUtil.ConvertToPrimitiveTypeName(typeArgSimpleName.ToString())}", new GUIStyle("Badge"));

            using var _ = new LabelWidthScope(80);

            var nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(nameRect, nameProp);

            var variableRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(variableRect, variableProp, true);

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

            var variableProp = property.FindPropertyRelative("_defaultValue");
            var storeProp = property.FindPropertyRelative("_store");

            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(variableProp, true);
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