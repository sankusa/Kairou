using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public class CommandMaker : EditorWindow
    {
        enum InstanceResolveWay
        {
            Null,
            Serialize,
            FromResolver,
        }

        enum ArgResolveWay
        {
            Null,
            Serialize,
            Variable,
            FromResolver,
            DefaultValue,
        }

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Assets/Create/" + nameof(Kairou) + "/CommandMaker")]
        static void Open()
        {
            var window = CreateInstance<CommandMaker>();
            window.titleContent = new GUIContent("CommandMaker");
            window.ShowAuxWindow();
        }

        Type _targetType;
        MethodInfo _targetMethod;
        InstanceResolveWay _instanceResolveWay = InstanceResolveWay.Null;
        List<ArgResolveWay> _argResolveWays = new();

        const string _namespaceSessionStateKey = "Kairou_CommandMaker_Namespace";

        public void CreateGUI()
        {
            EditorUtil.TryGetActiveFolderPath(out var locationPath);

            VisualElement root = rootVisualElement;

            // Instantiate UXML
            var panel = m_VisualTreeAsset.Instantiate();
            root.Add(panel);

            var typeDropdown = new TypeAdvancedDropdown(new AdvancedDropdownState());

            /* ---- Main ---- */
            var typeSelectButton = panel.Q<Button>("TypeSelectButton");
            typeSelectButton.clicked += () => typeDropdown.Show(typeSelectButton.parent.layout);
            var typeLabel = panel.Q<Label>("TypeLabel");
            var methodSelectButton = panel.Q<Button>("MethodSelectButton");
            methodSelectButton.style.display = DisplayStyle.None;
            var methodLabel = panel.Q<Label>("MethodLabel");
            var instanceArea = panel.Q<VisualElement>("InstanceMainArea");
            var parameterArea = panel.Q<VisualElement>("ParameterArea");
            var generateButton = panel.Q<Button>("GenerateButton");
            generateButton.enabledSelf = false;

            /* ---- Other ---- */
            var nameGenerateButton = panel.Q<Button>("NameGenerateButton");
            nameGenerateButton.enabledSelf = false;
            var namespaceField = panel.Q<TextField>("NamespaceField");
            namespaceField.value = SessionState.GetString(_namespaceSessionStateKey, "");
            namespaceField.RegisterValueChangedCallback(evt =>
            {
                SessionState.SetString(_namespaceSessionStateKey, evt.newValue);
            });
            var classNameField = panel.Q<TextField>("ClassNameField");
            var locationField = panel.Q<TextField>("LocationField");
            locationField.value = locationPath;
            var fileNameField = panel.Q<TextField>("FileNameField");
            var folderSelectButton = panel.Q<Button>("FolderSelectButton");

            /* ---- イベント登録 ---- */
            nameGenerateButton.clicked += () =>
            {
                string name = $"Command_{_targetType.Name}_{_targetMethod.Name}";
                classNameField.value = name;
                fileNameField.value = name;
            };
            folderSelectButton.clicked += () =>
            {
                string selectedFolder = EditorUtility.OpenFolderPanel("Select Folder", locationField.value, "");
                if (!string.IsNullOrEmpty(selectedFolder) && selectedFolder.StartsWith(Application.dataPath))
                {
                    locationField.value = selectedFolder.Substring(Application.dataPath.Length - "Assets".Length);
                }
            };
            typeDropdown.OnSelected += SetTargetType;
            methodSelectButton.clicked += () =>
            {
                var menu = new GenericMenu();
                foreach (var method in _targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    menu.AddItem(new GUIContent(method.GetDisplayLabel()), false, () =>
                    {
                        SetMethod(method);
                    });
                }
                menu.ShowAsContext();
            };
            generateButton.clicked += () =>
            {
                ProjectWindowUtil.CreateScriptAssetWithContent(
                    locationField.value + "/" + fileNameField.value + ".cs",
                    GenerateCode(_targetType, _targetMethod, _instanceResolveWay, _argResolveWays, namespaceField.value, classNameField.value));
            };

            void SetTargetType(Type type)
            {
                _targetType = type;
                typeLabel.text = _targetType == null ? "" : _targetType.FullName;
                methodSelectButton.style.display = _targetType == null ? DisplayStyle.None : DisplayStyle.Flex;
                SetMethod(null);
            }

            void SetMethod(MethodInfo methodInfo)
            {
                _targetMethod = methodInfo;
                methodLabel.text = _targetMethod.GetDisplayLabel();

                instanceArea.Clear();
                parameterArea.Clear();
                generateButton.enabledSelf = methodInfo != null;
                nameGenerateButton.enabledSelf = methodInfo != null;
                if (methodInfo == null) return;
                if (methodInfo.IsStatic)
                {
                    _instanceResolveWay = InstanceResolveWay.Null;
                    instanceArea.Add(new Label("Static"));
                }
                else
                {
                    _instanceResolveWay = InstanceResolveWay.FromResolver;

                    var allowedInstanceResolveWays = new List<InstanceResolveWay>() {};
                    if (_targetType.IsDefined(typeof(SerializableAttribute), true) || _targetType.IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        allowedInstanceResolveWays.Add(InstanceResolveWay.Serialize);
                    }
                    allowedInstanceResolveWays.Add(InstanceResolveWay.FromResolver);

                    var options = allowedInstanceResolveWays.ToArray();
                    var choices = options.Select(x => x.ToString()).ToList();

                    var dropdown = new EasyDropdownField<InstanceResolveWay>();
                    dropdown.SetOptions(options, choices);
                    dropdown.SetValue(InstanceResolveWay.FromResolver);
                    dropdown.RegisterValueChanged((way) => _instanceResolveWay = way, InstanceResolveWay.FromResolver);
                    
                    instanceArea.Add(dropdown);
                }
                var parameters = _targetMethod.GetParameters();
                _argResolveWays = Enumerable.Repeat(ArgResolveWay.Null, parameters.Length).ToList();
                for (int i = 0; i < parameters.Length; i++)
                {
                    int index = i;
                    var parameter = parameters[i];

                    // 解決方法の選択肢の構築
                    var allowedArgResolveWays = new List<ArgResolveWay>() { ArgResolveWay.Null };
                    if (parameter.ParameterType.IsPrimitive || parameter.ParameterType.IsDefined(typeof(SerializableAttribute), true) || parameter.ParameterType.IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        allowedArgResolveWays.Add(ArgResolveWay.Serialize);
                    }
                    if (VariableTypeCache.GetVariableTargetType().Contains(parameter.ParameterType))
                    {
                        allowedArgResolveWays.Add(ArgResolveWay.Variable);
                    }
                    allowedArgResolveWays.Add(ArgResolveWay.FromResolver);
                    if (parameter.HasDefaultValue)
                    {
                        allowedArgResolveWays.Add(ArgResolveWay.DefaultValue);
                    }

                    var options = allowedArgResolveWays.ToArray();
                    var choices = options.Select(x => x.ToString()).ToList();

                    var dropdown = new EasyDropdownField<ArgResolveWay>();
                    dropdown.SetOptions(options, choices);
                    dropdown.SetValue(ArgResolveWay.Null);
                    dropdown.RegisterValueChanged((way) => _argResolveWays[index] = way, ArgResolveWay.Null);
                    dropdown.label = TypeNameUtil.ConvertToPrimitiveTypeName(parameter.ParameterType.Name) + "  " + parameter.Name;
                    parameterArea.Add(dropdown);
                }
            }
        }

        static string GenerateCode(Type targetType, MethodInfo targetMethod, InstanceResolveWay instanceResolveWay, List<ArgResolveWay> argResolveWays, string namespaceName, string className)
        {
            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var getter = properties.FirstOrDefault(x => x.GetGetMethod() == targetMethod);
            var setter = properties.FirstOrDefault(x => x.GetSetMethod() == targetMethod);

            var builder = new CodeBuilder();
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using Kairou;");
            builder.AppendLine();

            bool hasNamespace = string.IsNullOrEmpty(namespaceName) == false;
            if (hasNamespace)
            {
                builder.AppendLine($"namespace {namespaceName}");
                builder.BeginBlock();
            }

            builder.AppendIndentedLine($"[CommandInfo(\"Generated\", nameof({targetType.FullName}) + \".\" + nameof({targetType.FullName}.{(setter == null ? targetMethod.Name : setter.Name)}))]");
            builder.AppendIndentedLine($"public partial class {className} : Command");
            using (new BlockScope(builder))
            {
                if (instanceResolveWay == InstanceResolveWay.Serialize)
                {
                    builder.AppendIndentedLine($"[SerializeField] {targetType.FullName} instance;");
                }

                for (int i = 0; i < argResolveWays.Count; i++)
                {
                    if (argResolveWays[i] == ArgResolveWay.Serialize)
                    {
                        var parameter = targetMethod.GetParameters()[i];
                        builder.AppendIndentedLine($"[SerializeField] {parameter.ParameterType.FullName} {parameter.Name};");
                    }
                    else if (argResolveWays[i] == ArgResolveWay.Variable)
                    {
                        var parameter = targetMethod.GetParameters()[i];
                        builder.AppendIndentedLine($"[GenerateValidation]");
                        builder.AppendIndentedLine($"[SerializeField] VariableValueGetterKey<{parameter.ParameterType.FullName}> {parameter.Name};");
                    }
                }

                builder.AppendLine();
                builder.AppendIndentedLine("[CommandExecute]");
                builder.AppendIndent();
                builder.Append("void Exeute(");
                int argCount = 0;
                if (instanceResolveWay == InstanceResolveWay.FromResolver)
                {
                    builder.Append("[Inject] ");
                    builder.Append(targetType.FullName);
                    builder.Append(" instance");
                    argCount++;
                }
                
                for (int i = 0; i < argResolveWays.Count; i++)
                {
                    if (argResolveWays[i] == ArgResolveWay.Variable)
                    {
                        if (argCount > 0)
                        {
                            builder.Append(", ");
                        }
                        var parameter = targetMethod.GetParameters()[i];
                        builder.Append($"[From(nameof({parameter.Name}))] ");
                        builder.Append(parameter.ParameterType.FullName);
                        builder.Append(" ");
                        builder.Append(parameter.Name);

                        argCount++;
                    }
                    else if (argResolveWays[i] == ArgResolveWay.FromResolver)
                    {
                        if (argCount > 0)
                        {
                            builder.Append(", ");
                        }
                        var parameter = targetMethod.GetParameters()[i];
                        builder.Append("[Inject] ");
                        builder.Append(parameter.ParameterType.FullName);
                        builder.Append(" ");
                        builder.Append(parameter.Name);
                        argCount++;
                    }
                }
                builder.AppendLine(")");
                using (new BlockScope(builder))
                {
                    if (targetMethod.IsStatic)
                    {
                        builder.AppendIndent();
                        builder.Append(targetType.FullName);
                        builder.Append(".");
                    }
                    else
                    {
                        builder.AppendIndent();
                        builder.Append("instance.");
                    }

                    if (setter != null)
                    {
                        builder.AppendLine($"{setter.Name} = value;");
                    }
                    
                    if (getter == null && setter == null)
                    {
                        builder.Append(targetMethod.Name);
                        builder.Append("(");
                        for (int i = 0; i < argResolveWays.Count; i++)
                        {
                            if (i > 0 && argResolveWays[i] != ArgResolveWay.DefaultValue)
                            {
                                builder.Append(", ");
                            }
                            if (argResolveWays[i] == ArgResolveWay.Null)
                            {
                                builder.Append("null");
                            }
                            else if (argResolveWays[i] == ArgResolveWay.Serialize)
                            {
                                var parameter = targetMethod.GetParameters()[i];
                                builder.Append(parameter.Name);
                            }
                            else if (argResolveWays[i] == ArgResolveWay.Variable)
                            {
                                var parameter = targetMethod.GetParameters()[i];
                                builder.Append(parameter.Name);
                            }
                            else if (argResolveWays[i] == ArgResolveWay.FromResolver)
                            {
                                var parameter = targetMethod.GetParameters()[i];
                                builder.Append(parameter.Name);
                            }
                            else if (argResolveWays[i] == ArgResolveWay.DefaultValue)
                            {

                            }
                        }
                        builder.AppendLine(");");
                    }
                }

                builder.AppendIndentedLine("public override string GetSummary()");
                using (new BlockScope(builder))
                {
                    builder.AppendIndentedLine("string summary = \"\";");
                    int count = 0;
                    for (int i = 0; i < argResolveWays.Count; i++)
                    {
                        if (argResolveWays[i] == ArgResolveWay.Null || argResolveWays[i] == ArgResolveWay.Serialize || argResolveWays[i] == ArgResolveWay.Variable || argResolveWays[i] == ArgResolveWay.FromResolver)
                        {
                            if (count > 0)
                            {
                                builder.AppendIndentedLine("summary += \", \";");
                            }
                        }
                        if (argResolveWays[i] == ArgResolveWay.Null)
                        {
                            builder.AppendIndentedLine($"summary += \"null\";");
                            count++;
                        }
                        else if (argResolveWays[i] == ArgResolveWay.Serialize)
                        {
                            var parameter = targetMethod.GetParameters()[i];
                            builder.AppendIndentedLine($"summary += {parameter.Name}.ToString();");
                            count++;
                        }
                        else if (argResolveWays[i] == ArgResolveWay.Variable)
                        {
                            var parameter = targetMethod.GetParameters()[i];
                            builder.AppendIndentedLine($"summary += {parameter.Name}.GetSummary();");
                            count++;
                        }
                        else if (argResolveWays[i] == ArgResolveWay.FromResolver)
                        {
                            var parameter = targetMethod.GetParameters()[i];
                            builder.AppendIndentedLine($"summary += \"FromResolver\";");
                            count++;
                        }
                    }
                    builder.AppendIndentedLine("return summary;");
                }
            }

            if (hasNamespace)
            {
                builder.EndBlock();
            }

            return builder.ToString();
        }
    }
}