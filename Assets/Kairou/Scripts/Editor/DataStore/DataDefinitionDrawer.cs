using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kairou.DataStore.Editor
{
    [CustomPropertyDrawer(typeof(DataDefinition))]
    public class DataDefinitionDrawer : PropertyDrawer
    {
        static string[] _typeIdArray = DataTypeCache.GetTypes().Select(x => x.TypeId).ToArray();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var keyProp = property.FindPropertyRelative("_key");
            var typeIdProp = property.FindPropertyRelative("_typeId");
            var defaultValueProp = property.FindPropertyRelative("_defaultValue");
            
            var keyRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var typeIdRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var valueRect = new Rect(position);
            
            EditorGUI.PropertyField(keyRect, keyProp);

            int typeIdIndex = Array.IndexOf(_typeIdArray, typeIdProp.stringValue);
            typeIdIndex = EditorGUI.Popup(typeIdRect, typeIdProp.displayName, typeIdIndex, _typeIdArray);
            if (typeIdIndex != -1)
            {
                typeIdProp.stringValue = _typeIdArray[typeIdIndex];
            }

            var type = DataTypeCache.Find(typeIdProp.stringValue);
            if (type != null)
            {
                defaultValueProp.stringValue = type.DrawField(valueRect, "Default Value", defaultValueProp.stringValue);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeIdProp = property.FindPropertyRelative("_typeId");
            var defaultValueProp = property.FindPropertyRelative("_defaultValue");
            var type = DataTypeCache.Find(typeIdProp.stringValue);
            float height = 0;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (type != null) height += type.GetFieldHeight(defaultValueProp.stringValue);
            return height;
        }
    }
}