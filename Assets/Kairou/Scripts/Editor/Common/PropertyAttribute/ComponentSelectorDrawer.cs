using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kairou
{
    [CustomPropertyDrawer(typeof(ComponentSelectorAttribute))]
    public class ComponentSelectorDrawer : PropertyDrawer
    {
        static readonly (float size, bool isFixed)[] _guiSizes = { (2f, false), (20f, true) };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                throw new ArgumentException($"{nameof(property)} must be {nameof(SerializedPropertyType)} {nameof(SerializedPropertyType.ObjectReference)}");
            }
            if (property.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                var component = property.objectReferenceValue as Component;

                var rects = RectUtil.SplitRect(position, EditorGUIUtility.standardVerticalSpacing, sizes: _guiSizes);

                EditorGUI.PropertyField(rects[0], property, label, true);

                Component[] components = component.gameObject.GetComponents<Component>();
                int componentIndex = Array.IndexOf(components, component);
                string[] displayOptions = components.Select(x => x.GetType().Name).ToArray();
                int newComponentIndex = EditorGUI.Popup(rects[1], componentIndex, displayOptions);
                if (newComponentIndex != componentIndex)
                {
                    property.objectReferenceValue = components[newComponentIndex];
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}