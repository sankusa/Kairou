using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Kairou.Editor
{
    public class TypeAdvancedDropdownItem : AdvancedDropdownItem
    {
        public Type Type { get; set; }
        public TypeAdvancedDropdownItem(Type type, string name) : base(name)
        {
            Type = type;
        }
    }

    public class TypeAdvancedDropdown : AdvancedDropdown
    {
        
        List<Type> _targetTypes;
        public event Action<Type> OnSelected;

        public TypeAdvancedDropdown(AdvancedDropdownState state) : base(state) {
            var minSize = minimumSize;
            minSize.y = 400;
            minimumSize = minSize;

            var _targetAssemblyNames = CompilationPipeline
                .GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies)
                .Select(x => x.name)
                .ToHashSet();

            _targetTypes = TypeCache.GetTypesDerivedFrom<object>()
                .Where(type =>
                {
                    if (type.IsSubclassOf(typeof(Attribute))) return false;
                    if (type.IsNotPublic) return false;
                    if (type.IsGenericTypeDefinition) return false;
                    if (type.Name.Contains("=")) return false;
                    if (type.Name.Contains("<") || type.Name.Contains(">")) return false;
                    if (type.Name.Contains("MonoScriptData")) return false;
                    if (type.IsDefined(typeof(CompilerGeneratedAttribute), false)) return false;

                    string assemblyName = type.Assembly.GetName().Name;
                    if (_targetAssemblyNames.Contains(assemblyName) == false && assemblyName.Contains("UnityEngine") == false) return false;
                    return true;
                })
                .OrderBy(type => type.FullName)
                .ToList();
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Root");

            foreach (var type in _targetTypes)
            {
                var currentItem = root;
                if (type.Namespace == null)
                {
                    var parent = root.children.FirstOrDefault(child => child.name == "<No Namespace>");
                    if (parent == null)
                    {
                        parent = new AdvancedDropdownItem("<No Namespace>");
                        root.AddChild(parent);
                    }
                    currentItem = parent;
                }
                else
                {
                    foreach (var ns in type.Namespace.Split('.'))
                    {
                        var categoryItem = currentItem.children.FirstOrDefault(child => child.name == ns);
                        if (categoryItem == null)
                        {
                            categoryItem = new AdvancedDropdownItem(ns);
                            currentItem.AddChild(categoryItem);
                        }
                        currentItem = categoryItem;
                    }
                }

                string label = type.Namespace == null ? type.FullName : type.FullName[(type.Namespace.Length + 1)..];

                currentItem.AddChild(new TypeAdvancedDropdownItem(type, $"{label}        [ {type.Namespace} ]"));
            }

            // int priorityOld = int.MaxValue;
            // foreach (var (category, priority) in commandDatabase.GetCategories())
            // {
            //     if (priorityOld != int.MaxValue && priorityOld - priority >= 100)
            //     {
            //         root.AddSeparator();
            //     }
            //     root.AddChild(new AdvancedDropdownItem(category));
            //     priorityOld = priority;
            // }

            // foreach (Type commandType in commandTypes)
            // {
            //     var commandProfile = commandDatabase.GetProfile(commandType);
            //     if (commandProfile.IsCategorized == false)
            //     {
            //         uncategorized ??= new AdvancedDropdownItem("Uncategorized");
            //         uncategorized.AddChild(new TypeAdvancedDropdownItem(commandType, commandProfile.Name));
            //         continue;
            //     }

            //     var categoryPath = commandProfile.CategoryName.Split('/');

            //     var categoryItem = root.children.FirstOrDefault(child => child.name == commandProfile.CategoryName);
            //     categoryItem.AddChild(new TypeAdvancedDropdownItem(commandType, commandProfile.Name));
            // }

            // if (uncategorized != null)
            // {
            //     root.AddSeparator();
            //     root.AddChild(uncategorized);
            // }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is not TypeAdvancedDropdownItem commandItem)
            {
                throw new ArgumentException($"item is not {nameof(TypeAdvancedDropdownItem)}");
            }
            OnSelected?.Invoke(commandItem.Type);
        }
    }
}