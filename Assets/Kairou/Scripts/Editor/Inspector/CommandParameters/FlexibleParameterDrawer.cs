using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [CustomPropertyDrawer(typeof(FlexibleParameter<>))]
    public class FlexibleParameterDrawer : PropertyDrawer
    {
        const int paddingWidth = 4;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var command = CommandUtilForEditor.GetContainingCommand(property);
            var flexibleParameter = property.GetObject() as FlexibleParameter;
            string typeName = $"{nameof(FlexibleParameter)}<{TypeNameUtil.ConvertToPrimitiveTypeName(flexibleParameter.TargetType.Name)}>";

            var resolveTypeProp = property.FindPropertyRelative("_resolveType");
            
            var valueProp = property.FindPropertyRelative("_value");
            var variableProp = property.FindPropertyRelative("_variable");

            var mainBox = new VisualElement();
            root.Add(mainBox);
            mainBox.style.marginLeft = 3;
            mainBox.style.marginRight = 3;
            mainBox.SetBorderColor(new Color(0.1f, 0.1f, 0.1f));
            mainBox.SetBorderWidth(1);

            var header = new VisualElement();
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

            var resolveTypeField = new PropertyField(resolveTypeProp);
            mainBox.Add(resolveTypeField);

            VisualElement valueField = null;
            if (valueProp == null)
            {
                var textField = new TextField();
                textField.label = "Value";
                textField.enabledSelf = false;
                textField.value = "default";
                mainBox.Add(textField);
                valueField = textField;
            }
            else if (valueProp.propertyType == SerializedPropertyType.String)
            {
                var textField = new TextField();
                textField.label = "Value";
                textField.multiline = true;
                textField.BindProperty(valueProp);
                mainBox.Add(textField);
                valueField = textField;
            }
            else
            {
                valueField = new PropertyField(valueProp);
                mainBox.Add(valueField);
            }
            
            var variableField = new PropertyField(variableProp);
            root.Add(variableField);

            variableField.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var mainBox = variableField.Q<VisualElement>("MainBox");
                if (mainBox == null) return;
                var header = variableField.Q<VisualElement>("Header");
                mainBox.style.borderTopWidth = 0;
                header.style.display = DisplayStyle.None;
            });

            resolveTypeField.RegisterValueChangeCallback(evt =>
            {
                if (resolveTypeProp.enumValueIndex == (int)FlexibleParameter.ResolveType.Value)
                {
                    mainBox.style.borderBottomWidth = 1;
                    valueField.style.display = DisplayStyle.Flex;
                    variableField.style.display = DisplayStyle.None;
                }
                else if (resolveTypeProp.enumValueIndex == (int)FlexibleParameter.ResolveType.Variable)
                {
                    mainBox.style.borderBottomWidth = 0;
                    valueField.style.display = DisplayStyle.None;
                    variableField.style.display = DisplayStyle.Flex;
                }
            });

            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var command = CommandUtilForEditor.GetContainingCommand(property);
            var flexibleParameter = property.GetObject() as FlexibleParameter;

            var resolveTypeProp = property.FindPropertyRelative("_resolveType");
            
            var valueProp = property.FindPropertyRelative("_value");
            var variableProp = property.FindPropertyRelative("_variable");

            float indentWidth = GUICommon.GetIndentWidth(EditorGUI.indentLevel);
            float indentedLabelWidth = EditorGUIUtility.labelWidth - indentWidth - paddingWidth;

            using var backgroundColorScope = new BackgroundColorScope(new Color(0.8f, 0.8f, 0.8f));

            if (label != GUIContent.none)
            {
                var headerRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
                headerRect.xMin += indentWidth;
                position.yMin += headerRect.height - 1;
                using (new BackgroundColorScope(new Color(0.6f, 0.6f, 0.6f)))
                {
                    GUI.Box(headerRect, "", GUIStyles.GroupBox);
                }

                string typeName = $"<color=grey>{nameof(FlexibleParameter)}<{TypeNameUtil.ConvertToPrimitiveTypeName(flexibleParameter.TargetType.Name)}></color>";

                headerRect = new RectOffset(paddingWidth, paddingWidth, 0, 0).Remove(headerRect);
                GUI.skin.label.CalcMinMaxWidth(label, out float _, out float labelWidth);
                GUIStyles.MiniLabel.CalcMinMaxWidth(new GUIContent(typeName), out float _, out float typeNameWidth);
                List<Rect> rects = RectUtil.SplitRect(headerRect, 0, sizes: new[] { (labelWidth, true), (1, false), (typeNameWidth, true) });
                
                using (new IndentLevelScope(0))
                {
                    EditorGUI.LabelField(rects[0], label);
                    EditorGUI.LabelField(rects[2], new GUIContent(typeName), GUIStyles.MiniLabel);
                }
            }

            var resolveTypeRect = new Rect(position) { height = EditorGUI.GetPropertyHeight(resolveTypeProp, true) + EditorGUIUtility.standardVerticalSpacing * 2 };
            resolveTypeRect.xMin += indentWidth;
            position.yMin += resolveTypeRect.height - 1;
            GUI.Box(resolveTypeRect, "", GUIStyles.GroupBox);

            resolveTypeRect = new RectOffset(paddingWidth, 2, (int)EditorGUIUtility.standardVerticalSpacing, 0).Remove(resolveTypeRect);
            using (new IndentLevelScope(1))
            {
                using var labelWidthScope = new LabelWidthScope(indentedLabelWidth);
                EditorGUI.PropertyField(resolveTypeRect, resolveTypeProp, true);
            }
            
            var resolveType = (FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex;
            if ((FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex == FlexibleParameter.ResolveType.Value)
            {
                var valueRect = new Rect(position) { height = EditorGUI.GetPropertyHeight(valueProp, true) + EditorGUIUtility.standardVerticalSpacing * 2 };
                valueRect.xMin += indentWidth;
                position.yMin += valueRect.height - 1;
                GUI.Box(valueRect, "", GUIStyles.GroupBox);

                valueRect = new RectOffset(paddingWidth, 2, (int)EditorGUIUtility.standardVerticalSpacing, 0).Remove(valueRect);
                using (new IndentLevelScope(1))
                {
                    using var labelWidthScope = new LabelWidthScope(indentedLabelWidth);
                    EditorGUI.PropertyField(valueRect, valueProp, true);
                }
            }
            else if ((FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex == FlexibleParameter.ResolveType.Variable)
            {
                var variableRect = new Rect(position) { height = EditorGUI.GetPropertyHeight(variableProp, GUIContent.none, true) + EditorGUIUtility.standardVerticalSpacing };
                position.yMin += variableRect.height;
                EditorGUI.PropertyField(variableRect, variableProp, GUIContent.none, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var resolveTypeProp = property.FindPropertyRelative("_resolveType");
            var valueProp = property.FindPropertyRelative("_value");
            var variableProp = property.FindPropertyRelative("_variable");
            float height = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2 - 1;
            if ((FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex == FlexibleParameter.ResolveType.Value)
            {
                height += EditorGUI.GetPropertyHeight(valueProp, true) + EditorGUIUtility.standardVerticalSpacing * 2 - 1;
            }
            else if ((FlexibleParameter.ResolveType)resolveTypeProp.enumValueIndex == FlexibleParameter.ResolveType.Variable)
            {
                height += EditorGUI.GetPropertyHeight(variableProp, GUIContent.none, true) - 1;
            }
            return height;
        }
    }
}