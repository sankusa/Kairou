// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;

// namespace Kairou.Editor
// {
//     [CustomPropertyDrawer(typeof(FlexibleParameter<>))]
//     public class FlexibleParameterDrawer : PropertyDrawer
//     {
//         const int paddingWidth = 4;

//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             var command = CommandUtilForEditor.GetContainingCommand(property);
//             var flexibleParameter = property.GetObject() as FlexibleParameter;

//             var resolveTypeProp = property.FindPropertyRelative("_resolveType");
            
//             var valueProp = property.FindPropertyRelative("_value");
//             var variableProp = property.FindPropertyRelative("_variable");

//             float indentWidth = GUICommon.GetIndentWidth(EditorGUI.indentLevel);
//             float indentedLabelWidth = EditorGUIUtility.labelWidth - indentWidth - paddingWidth;

//             using var backgroundColorScope = new BackgroundColorScope(new Color(0.8f, 0.8f, 0.8f));

//             if (label != GUIContent.none)
//             {
//                 var headerRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
//                 headerRect.xMin += indentWidth;
//                 position.yMin += headerRect.height - 1;
//                 using (new BackgroundColorScope(new Color(0.6f, 0.6f, 0.6f)))
//                 {
//                     GUI.Box(headerRect, "", GUIStyles.GroupBox);
//                 }

//                 string typeName = $"<color=grey>{nameof(FlexibleParameter)}<{TypeNameUtil.ConvertToPrimitiveTypeName(flexibleParameter.TargetType.Name)}></color>";

//                 headerRect = new RectOffset(paddingWidth, paddingWidth, 0, 0).Remove(headerRect);
//                 GUI.skin.label.CalcMinMaxWidth(label, out float _, out float labelWidth);
//                 GUIStyles.MiniLabel.CalcMinMaxWidth(new GUIContent(typeName), out float _, out float typeNameWidth);
//                 List<Rect> rects = RectUtil.SplitRect(headerRect, 0, sizes: new[] { (labelWidth, true), (1, false), (typeNameWidth, true) });
                
//                 using (new IndentLevelScope(0))
//                 {
//                     EditorGUI.LabelField(rects[0], label);
//                     EditorGUI.LabelField(rects[2], new GUIContent(typeName), GUIStyles.MiniLabel);
//                 }
//             }

//             var resolveTypeRect = new Rect(position) { height = EditorGUI.GetPropertyHeight(resolveTypeProp, true) + EditorGUIUtility.standardVerticalSpacing * 2 };
//             resolveTypeRect.xMin += indentWidth;
//             position.yMin += resolveTypeRect.height - 1;
//             GUI.Box(resolveTypeRect, "", GUIStyles.GroupBox);

//             resolveTypeRect = new RectOffset(paddingWidth, 2, (int)EditorGUIUtility.standardVerticalSpacing, 0).Remove(resolveTypeRect);
//             using (new IndentLevelScope(1))
//             {
//                 using var labelWidthScope = new LabelWidthScope(indentedLabelWidth);
//                 EditorGUI.PropertyField(resolveTypeRect, resolveTypeProp, true);
//             }
            
//             var resolveType = (FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex;
//             if ((FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex == FlexibleParameter.ResolveType.Value)
//             {
//                 var valueRect = new Rect(position) { height = EditorGUI.GetPropertyHeight(valueProp, true) + EditorGUIUtility.standardVerticalSpacing * 2 };
//                 valueRect.xMin += indentWidth;
//                 position.yMin += valueRect.height - 1;
//                 GUI.Box(valueRect, "", GUIStyles.GroupBox);

//                 valueRect = new RectOffset(paddingWidth, 2, (int)EditorGUIUtility.standardVerticalSpacing, 0).Remove(valueRect);
//                 using (new IndentLevelScope(1))
//                 {
//                     using var labelWidthScope = new LabelWidthScope(indentedLabelWidth);
//                     EditorGUI.PropertyField(valueRect, valueProp, true);
//                 }
//             }
//             else if ((FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex == FlexibleParameter.ResolveType.Variable)
//             {
//                 var variableRect = new Rect(position) { height = EditorGUI.GetPropertyHeight(variableProp, GUIContent.none, true) + EditorGUIUtility.standardVerticalSpacing };
//                 position.yMin += variableRect.height;
//                 EditorGUI.PropertyField(variableRect, variableProp, GUIContent.none, true);
//             }
//         }

//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             var resolveTypeProp = property.FindPropertyRelative("_resolveType");
//             var valueProp = property.FindPropertyRelative("_value");
//             var variableProp = property.FindPropertyRelative("_variable");
//             float height = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2 - 1;
//             if ((FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex == FlexibleParameter.ResolveType.Value)
//             {
//                 height += EditorGUI.GetPropertyHeight(valueProp, true) + EditorGUIUtility.standardVerticalSpacing * 2 - 1;
//             }
//             else if ((FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex == FlexibleParameter.ResolveType.Variable)
//             {
//                 height += EditorGUI.GetPropertyHeight(variableProp, GUIContent.none, true) - 1;
//             }
//             return height;
//         }
//     }
// }