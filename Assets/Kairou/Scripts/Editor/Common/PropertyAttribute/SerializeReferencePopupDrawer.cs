using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Kairou
{
    [CustomPropertyDrawer(typeof(SerializeReferencePopupAttribute))]
    public class SerializeReferencePopupDrawer : PropertyDrawer
    {
        List<Type> _subclasses;
        string[] _subclassFullNames;
        string[] _subclassDisplayNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitializeIfNeeded(property);

            var popupRect = new Rect(position);
            popupRect.height = EditorGUIUtility.singleLineHeight;
            popupRect.xMin += EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing - EditorGUI.indentLevel * 15f;

            int currentIndex = Array.IndexOf(_subclassFullNames, property.managedReferenceFullTypename);
            int selectedIndex = EditorGUI.Popup(popupRect, currentIndex, _subclassDisplayNames);
            if (selectedIndex != currentIndex)
            {
                Type selectedType = _subclasses[selectedIndex];
                property.managedReferenceValue = selectedType == null ? null : Activator.CreateInstance(selectedType);
                return;
            }

            EditorGUI.PropertyField(position, property, label, true); 
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        void InitializeIfNeeded(SerializedProperty property)
        {
            if (_subclasses != null) return;

            string[] fieldTypeName = property.managedReferenceFieldTypename.Split(' ');
            Type fieldType = Assembly.Load(fieldTypeName[0]).GetType(fieldTypeName[1]);

            _subclasses = new List<Type>() {null};
            _subclasses.AddRange(TypeCache.GetTypesDerivedFrom(fieldType));

            _subclassFullNames = _subclasses
                .Select(type => type == null ? "" : $"{type.Assembly.FullName.Split(',')[0]} {type.FullName}")
                .ToArray();
            _subclassDisplayNames = _subclasses
                .Select(type => {
                    if(type == null) return "<null>";
                    return type.Name;
                })
                .ToArray();
        }
    }
}