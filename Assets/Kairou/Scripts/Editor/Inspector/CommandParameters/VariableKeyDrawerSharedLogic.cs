using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public static class VariableKeyDrawerSharedLogic
    {
        const int paddingWidth = 4;

        static bool _changed = false;

        public static VisualElement CreatePropertyGUI(SerializedProperty property, Type baseType, Type targetType, Func<VariableDefinition, bool> definitionFilterPredicate, Func<Command, (VariableDefinition, FoundVariableScope)> definitionFindFunc)
        {
            var root = new VisualElement();

            var command = CommandUtilForEditor.GetContainingCommand(property);

            var variableNameProp = property.FindPropertyRelative("_variableName");
            var targetScopeProp = property.FindPropertyRelative("_targetScope");

            string typeName = $"{baseType.Name}<{TypeNameUtil.ConvertToPrimitiveTypeName(targetType.Name)}>";

            var mainBox = new VisualElement();
            mainBox.name = "MainBox";
            root.Add(mainBox);
            mainBox.style.marginLeft = 3;
            mainBox.style.marginRight = 3;
            mainBox.SetBorderColor(new Color(0.1f, 0.1f, 0.1f));
            mainBox.SetBorderWidth(1);

            var header = new VisualElement();
            header.name = "Header";
            header.style.flexDirection = FlexDirection.Row;
            header.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            var propertynameLabel = new Label(ObjectNames.NicifyVariableName(property.name));
            propertynameLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            header.Add(propertynameLabel);
            header.Add(new VisualElement() { style = { flexGrow = 1 } });
            var typeNameLabel = new Label(typeName);
            typeNameLabel.style.color = Color.grey;
            typeNameLabel.style.fontSize = 10;
            typeNameLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            header.Add(typeNameLabel);
            mainBox.Add(header);

            var targetScopeField = new PropertyField(targetScopeProp);
            mainBox.Add(targetScopeField);

            var variableNameBox = new VisualElement();
            variableNameBox.style.flexDirection = FlexDirection.Row;
            mainBox.Add(variableNameBox);
            var variableNameField = new PropertyField(variableNameProp);
            variableNameField.style.flexGrow = 1;
            variableNameBox.Add(variableNameField);

            var button = new Button();
            button.iconImage = GUICommon.DropdownIcon;
            button.style.width = 20;
            button.style.marginLeft = 1;
            button.style.paddingLeft = 1;
            button.style.paddingRight = 1;
            button.clicked += () =>
            {
                new VariableSelectGenericMenu(
                    command,
                    (TargetVariableScope)targetScopeProp.enumValueIndex,
                    variable =>
                    {
                        return definitionFilterPredicate.Invoke(variable);
                    },
                    variable =>
                    {
                        variableNameProp.stringValue = variable.Name;
                        property.serializedObject.ApplyModifiedProperties();
                        _changed = true;
                    }
                ).ShowAsContext();
            };
            variableNameBox.Add(button);

            var availableVariableBox = new VisualElement();
            availableVariableBox.style.flexDirection = FlexDirection.Row;
            var resultImage = new Image();
            availableVariableBox.Add(resultImage);
            var resultLabel = new Label();
            resultLabel.style.color = Color.grey;
            availableVariableBox.Add(resultLabel);
            mainBox.Add(availableVariableBox);

            UpdateAvailableVariableResult(resultImage, resultLabel, definitionFindFunc, command);

            root.TrackSerializedObjectValue(property.serializedObject, _ =>
            {
                UpdateAvailableVariableResult(resultImage, resultLabel, definitionFindFunc, command);
            });

            return root;
        }

        static void UpdateAvailableVariableResult(Image resultImage, Label resultLabel, Func<Command, (VariableDefinition, FoundVariableScope)> definitionFindFunc, Command command)
        {
            var (foundDefinition, foundScope) = definitionFindFunc(command);
            if (foundDefinition == null)
            {
                resultImage.image = GUICommon.InvalidIcon;
                resultLabel.text = "Variable not found";
            }
            else
            {
                resultImage.image = GUICommon.ValidIcon;
                resultLabel.text = $"{foundScope} {TypeNameUtil.ConvertToPrimitiveTypeName(foundDefinition.TargetType.Name)} {foundDefinition.Name}";
            }
        }

        public static void OnGUI(Rect position, SerializedProperty property, GUIContent label, Type baseType, Type targetType, Func<VariableDefinition, bool> definitionFilterPredicate, Func<Command, (VariableDefinition, FoundVariableScope)> definitionFindFunc)
        {
            var command = CommandUtilForEditor.GetContainingCommand(property);
            var (foundDefinition, foundScope) = definitionFindFunc(command);

            var variableNameProp = property.FindPropertyRelative("_variableName");
            var targetScopeProp = property.FindPropertyRelative("_targetScope");

            float originalIndentWidth = GUICommon.GetIndentWidth(EditorGUI.indentLevel);
            using var indentLevelScope = new IndentLevelScope(0);
            position.xMin += originalIndentWidth;
            
            using var backgroundColorScope = new BackgroundColorScope(new Color(0.8f, 0.8f, 0.8f));

            if (label != GUIContent.none)
            {
                var headerRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
                position.yMin += headerRect.height - 1;
                using (new BackgroundColorScope(new Color(0.6f, 0.6f, 0.6f)))
                {
                    GUI.Box(headerRect, "", GUIStyles.GroupBox);
                }

                string typeName = $"<color=grey>{baseType.Name}<{TypeNameUtil.ConvertToPrimitiveTypeName(targetType.Name)}></color>";

                headerRect = new RectOffset(paddingWidth, paddingWidth, 0, 0).Remove(headerRect);
                GUI.skin.label.CalcMinMaxWidth(label, out float _, out float labelWidth);
                GUIStyles.MiniLabel.CalcMinMaxWidth(new GUIContent(typeName), out float _, out float typeNameWidth);
                List<Rect> rects = RectUtil.SplitRect(headerRect, 0, sizes: new[] { (labelWidth, true), (1, false), (typeNameWidth, true) });
                
                EditorGUI.LabelField(rects[0], label);
                EditorGUI.LabelField(rects[2], new GUIContent(typeName), GUIStyles.MiniLabel);
            }

            var mainRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3 };
            position.yMin += mainRect.height - 1;
            GUI.Box(mainRect, "", GUIStyles.GroupBox);

            mainRect = new RectOffset(paddingWidth, 2, (int)EditorGUIUtility.standardVerticalSpacing, 0).Remove(mainRect);
            var line1Rect = new Rect(mainRect) { height = EditorGUIUtility.singleLineHeight };
            mainRect.yMin += line1Rect.height + EditorGUIUtility.standardVerticalSpacing;
            var line2Rect = new Rect(mainRect) { height = EditorGUIUtility.singleLineHeight };
            mainRect.yMin += line2Rect.height + EditorGUIUtility.standardVerticalSpacing;

            using (new LabelWidthScope(EditorGUIUtility.labelWidth - originalIndentWidth - paddingWidth))
            {
                using var indentLevelScope2 = new IndentLevelScope(1);

                // line1
                EditorGUI.PropertyField(line1Rect, targetScopeProp);

                // line2
                List<Rect> line2rects = RectUtil.SplitRect(line2Rect, 0, sizes: new (float size, bool isFixed)[] { (1, false), (18, true) });
                EditorGUI.PropertyField(line2rects[0], variableNameProp);
                if (GUI.Button(line2rects[1], GUIContent.none))
                {
                    new VariableSelectGenericMenu(
                        command,
                        (TargetVariableScope)targetScopeProp.enumValueIndex,
                        variable =>
                        {
                            return definitionFilterPredicate.Invoke(variable);
                        },
                        variable =>
                        {
                            variableNameProp.stringValue = variable.Name;
                            property.serializedObject.ApplyModifiedProperties();
                            _changed = true;
                        }
                    ).ShowAsContext();
                }
                GUI.DrawTexture(new RectOffset(3, 3, 3, 3).Remove(line2rects[1]), GUICommon.DropdownIcon);
            }

            var foundDefinitionRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            position.yMin += foundDefinitionRect.height + EditorGUIUtility.standardVerticalSpacing;

            var foundDefinitionIconRect = new Rect(foundDefinitionRect);
            foundDefinitionIconRect.xMin += GUICommon.GetIndentWidth(1) - 1;

            var foundDefinitionLabelRect = new Rect(foundDefinitionRect);
            foundDefinitionLabelRect.xMin += 20 + GUICommon.GetIndentWidth(1);

            GUI.Box(foundDefinitionRect, "", GUIStyles.GroupBox);
            
            if (foundDefinition == null)
            {
                EditorGUI.LabelField(foundDefinitionIconRect, new GUIContent(GUICommon.InvalidIcon));
                EditorGUI.LabelField(foundDefinitionLabelRect, $"<color=grey>Variable not found</color>", GUIStyles.RichTextLabel);
            }
            else
            {   
                EditorGUI.LabelField(foundDefinitionIconRect, new GUIContent(GUICommon.ValidIcon));
                EditorGUI.LabelField(foundDefinitionLabelRect, $"<color=grey>{foundScope} {TypeNameUtil.ConvertToPrimitiveTypeName(foundDefinition.TargetType.Name)} {foundDefinition.Name}</color>", GUIStyles.RichTextLabel);
            }

            if (_changed)
            {
                _changed = false;
                GUI.changed = true;
            }
        }

        public static float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            if (label != GUIContent.none) height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing - 3;
            height += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing + 3;
            return height;
        }
    }
}