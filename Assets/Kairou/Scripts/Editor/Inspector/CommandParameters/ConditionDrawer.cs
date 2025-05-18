using System;
using System.Collections.Generic;
using System.Linq;
using Kairou.Editor;
using UnityEditor;
using UnityEngine;

namespace Kairou
{
    [CustomPropertyDrawer(typeof(Condition))]
    public class ConditionDrawer : PropertyDrawer
    {
        static readonly Type[] _variableTypes;
        static readonly string[] _variableTypeNames;

        static ConditionDrawer()
        {
            _variableTypes = VariableTypeCache
                .GetVariableType()
                .Prepend(null)
                .ToArray();
            _variableTypeNames = VariableTypeCache
                .GetVariableType()
                .Select(x => TypeNameUtil.ConvertToPrimitiveTypeName(x.Name))
                .Prepend("null")
                .ToArray();
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var condition = property.GetObject() as Condition;

            int typeIndex = _variableTypes.IndexOf(condition?.TargetType);

            var headerRect = new Rect(rect) {height = EditorGUIUtility.singleLineHeight};
            EditorGUI.BeginChangeCheck();
            typeIndex = EditorGUI.Popup(headerRect, label.text, typeIndex, _variableTypeNames);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(property.serializedObject.targetObject, $"{nameof(Condition)} Type Chenged");
                if (typeIndex == 0)
                {
                    condition = null;
                }
                else
                {
                    condition = Activator.CreateInstance(typeof(Condition<>).MakeGenericType(_variableTypes[typeIndex])) as Condition;
                }
                property.managedReferenceValue = condition;
            }
            rect.yMin += headerRect.height + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel++;

            if (condition == null) return;

            var value1Prop = property.FindPropertyRelative("_value1");
            var operatorProp = property.FindPropertyRelative("_operator");
            var value2Prop = property.FindPropertyRelative("_value2");

            // value1
            var value1Rect = new Rect(rect) { height = EditorGUI.GetPropertyHeight(value1Prop, true) };
            EditorGUI.PropertyField(value1Rect, value1Prop, true);
            rect.yMin += value1Rect.height + EditorGUIUtility.standardVerticalSpacing;
            
            // 比較演算子
            var operatorRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };
            Condition.CompareOperator[] operators;
            if(typeof(IComparable).IsAssignableFrom(condition.TargetType))
            {
                operators = Condition.OperatorsForCompareable;
            }
            else
            {
                operators = Condition.OperatorsForNotCompareable;
            }
            int operatorIndex = Array.IndexOf(operators, condition.Operator);
            if(operatorIndex == -1) operatorIndex = 0;
            operatorIndex = EditorGUI.Popup(operatorRect, "Operator", operatorIndex, operators.Select(x => x.GetOperatorString()).ToArray());
            rect.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            operatorProp.enumValueIndex = (int)operators[operatorIndex];

            // value2
            var value2Rect = new Rect(rect) { height = EditorGUI.GetPropertyHeight(value2Prop, true) };
            EditorGUI.PropertyField(value2Rect, value2Prop, true);
            rect.yMin += value2Rect.height + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = 0;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (property.managedReferenceValue != null)
            {
                var value1Prop = property.FindPropertyRelative("_value1");
                height += EditorGUI.GetPropertyHeight(value1Prop, true) + EditorGUIUtility.standardVerticalSpacing;
                var operatorProp = property.FindPropertyRelative("_operator");
                height += EditorGUI.GetPropertyHeight(operatorProp, true) + EditorGUIUtility.standardVerticalSpacing;
                var value2Prop = property.FindPropertyRelative("_value2");
                height += EditorGUI.GetPropertyHeight(value2Prop, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
    }
}