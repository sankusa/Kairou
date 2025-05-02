using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

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
            var root = new AdvancedDropdownItem("Root");

            var commandTypes = TypeCache
                .GetTypesWithAttribute(typeof(CommandInfoAttribute))
                .Where(type => type.IsSubclassOf(typeof(Command)) && !type.IsAbstract);

            foreach (Type commandType in commandTypes)
            {
                var commandInfo = commandType.GetCustomAttribute<CommandInfoAttribute>();
                var categoryPath = commandInfo.CategoryPath.Split('/');

                var parentItem = root;
                foreach (string category in categoryPath)
                {
                    var currentCategoryItem = root.children.FirstOrDefault(child => child.name == category);
                    if (currentCategoryItem == null)
                    {
                        currentCategoryItem = new AdvancedDropdownItem(category);
                        root.AddChild(currentCategoryItem);
                    }
                    parentItem = currentCategoryItem;
                }
                parentItem.AddChild(new CommandAdvancedDropdownItem(commandType, commandInfo.CommandName));
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