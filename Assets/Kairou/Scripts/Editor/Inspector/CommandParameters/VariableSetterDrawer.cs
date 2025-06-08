// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Kairou.Editor;
// using UnityEditor;
// using UnityEngine;

// namespace Kairou
// {
//     [CustomPropertyDrawer(typeof(VariableSetter))]
//     public class VariableSetterDrawer : PropertyDrawer
//     {
//         static readonly Type[] _variableTypes;
//         static readonly string[] _variableTypeNames;

//         static VariableSetterDrawer()
//         {
//             _variableTypes = VariableTypeCache
//                 .GetVariableType()
//                 .Prepend(null)
//                 .ToArray();
//             _variableTypeNames = VariableTypeCache
//                 .GetVariableType()
//                 .Select(x => TypeNameUtil.ConvertToPrimitiveTypeName(x.Name))
//                 .Prepend("null")
//                 .ToArray();
//         }

//         public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
//         {
//             var variableSetter = property.GetObject() as VariableSetter;

//             int typeIndex = _variableTypes.IndexOf(variableSetter?.TargetType);

//             var headerRect = new Rect(rect) {height = EditorGUIUtility.singleLineHeight};
//             EditorGUI.BeginChangeCheck();
//             typeIndex = EditorGUI.Popup(headerRect, label.text, typeIndex, _variableTypeNames);
//             if (EditorGUI.EndChangeCheck())
//             {
//                 Undo.RecordObject(property.serializedObject.targetObject, $"{nameof(VariableSetter)} Type Chenged");
//                 if (typeIndex == 0)
//                 {
//                     variableSetter = null;
//                 }
//                 else
//                 {
//                     variableSetter = Activator.CreateInstance(typeof(VariableSetter<>).MakeGenericType(_variableTypes[typeIndex])) as VariableSetter;
//                 }
//                 property.managedReferenceValue = variableSetter;
//             }
//             rect.yMin += headerRect.height + EditorGUIUtility.standardVerticalSpacing;

//             EditorGUI.indentLevel++;

//             if (variableSetter == null) return;

//             var variableProp = property.FindPropertyRelative("_variable");
//             var operatorProp = property.FindPropertyRelative("_operator");
//             var valueProp = property.FindPropertyRelative("_value");

//             // variable
//             var variableRect = new Rect(rect) { height = EditorGUI.GetPropertyHeight(variableProp, true) };
//             EditorGUI.PropertyField(variableRect, variableProp, true);
//             rect.yMin += variableRect.height + EditorGUIUtility.standardVerticalSpacing;
            
//             // 比較演算子
//             var operatorRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };
//             VariableSetter.AssignOperator[] operators = variableSetter.GenerateAllowedOperators();
//             int operatorIndex = Array.IndexOf(operators, variableSetter.Operator);
//             if(operatorIndex == -1) operatorIndex = 0;
//             operatorIndex = EditorGUI.Popup(operatorRect, "Operator", operatorIndex, operators.Select(x => x.GetOperatorString()).ToArray());
//             rect.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

//             operatorProp.enumValueIndex = (int)operators[operatorIndex];

//             // value
//             var valueRect = new Rect(rect) { height = EditorGUI.GetPropertyHeight(valueProp, true) };
//             EditorGUI.PropertyField(valueRect, valueProp, true);
//             rect.yMin += valueRect.height + EditorGUIUtility.standardVerticalSpacing;

//             EditorGUI.indentLevel--;
//         }

//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
//             float height = 0;
//             height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//             if (property.managedReferenceValue != null)
//             {
//                 var variableProp = property.FindPropertyRelative("_variable");
//                 height += EditorGUI.GetPropertyHeight(variableProp, true) + EditorGUIUtility.standardVerticalSpacing;
//                 var operatorProp = property.FindPropertyRelative("_operator");
//                 height += EditorGUI.GetPropertyHeight(operatorProp, true) + EditorGUIUtility.standardVerticalSpacing;
//                 var valueProp = property.FindPropertyRelative("_value");
//                 height += EditorGUI.GetPropertyHeight(valueProp, true) + EditorGUIUtility.standardVerticalSpacing;
//             }
//             return height;
//         }
//     }
// }