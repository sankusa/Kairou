using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Kairou.Editor
{
    public class CommandAdvancedDropdownItem : AdvancedDropdownItem
    {
        public Type CommandType { get; set; }
        public CommandAdvancedDropdownItem(Type commandType, string name) : base(name)
        {
            CommandType = commandType;
        }
    }

    public class CommandAdvancedDropdown : AdvancedDropdown
    {
        public event Action<Command> OnSelected;

        public CommandAdvancedDropdown(AdvancedDropdownState state) : base(state) {
            var minSize = minimumSize;
            minSize.y = 400;
            minimumSize = minSize;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var commandDatabase = CommandDatabase.Load();

            var root = new AdvancedDropdownItem("Root");
            AdvancedDropdownItem uncategorized = null;

            var commandTypes = CommandTypeCache.Types;

            int priorityOld = int.MaxValue;
            foreach (var (category, priority) in commandDatabase.GetCategories())
            {
                if (priorityOld != int.MaxValue && priorityOld - priority >= 100)
                {
                    root.AddSeparator();
                }
                root.AddChild(new AdvancedDropdownItem(category));
                priorityOld = priority;
            }

            foreach (Type commandType in commandTypes)
            {
                var commandProfile = commandDatabase.GetProfile(commandType);
                if (commandProfile.IsCategorized == false)
                {
                    uncategorized ??= new AdvancedDropdownItem("Uncategorized");
                    uncategorized.AddChild(new CommandAdvancedDropdownItem(commandType, commandProfile.Name));
                    continue;
                }

                var categoryPath = commandProfile.CategoryName.Split('/');

                var categoryItem = root.children.FirstOrDefault(child => child.name == commandProfile.CategoryName);
                categoryItem.AddChild(new CommandAdvancedDropdownItem(commandType, commandProfile.Name));
            }

            if (uncategorized != null)
            {
                root.AddSeparator();
                root.AddChild(uncategorized);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is not CommandAdvancedDropdownItem commandItem)
            {
                throw new ArgumentException($"item is not {nameof(CommandAdvancedDropdownItem)}");
            }
            var command = Command.CreateInstance(commandItem.CommandType);
            OnSelected?.Invoke(command);
        }
    }
}