using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

namespace Kairou.Editor
{
    [Serializable]
    public class CommandPickerPanel
    {
        CommandDatabase _commandDatabase;
        CommandDatabase CommandDatabase => _commandDatabase ??= CommandDatabase.Load();

        List<string> _categoryNames;
        ListView _categoryListView;

        List<Type> _commandTypes;
        ListView _commandListView;

        public void Initialize(VisualElement parent, VisualTreeAsset categoryListUXML, VisualTreeAsset commandListUXML, Action<Type> onSelect)
        {
            var root = new VisualElement();
            root.style.flexGrow = 1;

            // CategoryList
            var categoryList = categoryListUXML.Instantiate();
            _categoryListView = categoryList.Q<ListView>();
            _categoryListView.makeItem = () =>
            {
                var item = _categoryListView.itemTemplate.Instantiate();
                var overlay = item.Q<VisualElement>("Overlay");
                item.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    overlay.style.backgroundColor = GUICommon.HoverOverlayColor;
                });
                item.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    overlay.style.backgroundColor = Color.clear;
                });
                return item;
            };
            _categoryListView.bindItem = (item, index) =>
            {
                string categoryName = _categoryNames[index];
                var categorySetting = CommandDatabase.FindCategorySetting(categoryName);
                item.style.backgroundColor = categorySetting != null ? categorySetting.BackgroundColor : Color.clear;
                var label = item.Q<Label>();
                label.text = categoryName ?? "<Uncategorized>";
                label.style.color = categorySetting != null ? categorySetting.LabelColor : GUICommon.DefaultLabelColor;
            };
            _categoryListView.selectedIndicesChanged += _ =>
            {
                string categoryName = _categoryNames[_categoryListView.selectedIndex];
                if (categoryName != null)
                {
                    _commandTypes = CommandDatabase.FindCommandsByCategoryName(categoryName).ToList();
                }
                else
                {
                    _commandTypes = CommandDatabase.UncategorizedCommands().ToList();
                }
                
                _commandListView.itemsSource = _commandTypes;
            };

            // CommandList
            var commandList = commandListUXML.Instantiate();
            _commandListView = commandList.Q<ListView>();
            _commandListView.makeItem = () =>
            {
                var item = _commandListView.itemTemplate.Instantiate();

                item.Q<VisualElement>("IconBox").Add(new Image());

                var overlay = item.Q<VisualElement>("Overlay");
                item.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    overlay.style.backgroundColor = GUICommon.HoverOverlayColor;
                });
                item.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    overlay.style.backgroundColor = Color.clear;
                });

                item.RegisterCallback<ClickEvent>(evt =>
                {
                    int index = (int)item.userData;
                    onSelect?.Invoke(_commandTypes[index]);
                });
                return item;
            };
            _commandListView.bindItem = (item, index) =>
            {
                var commandProfile = CommandDatabase.GetProfile(_commandTypes[index]);

                item.style.backgroundColor = commandProfile.BackgoundColor;

                var label = item.Q<Label>();
                label.text = CommandDatabase.GetProfile(_commandTypes[index]).Name;
                label.style.color = commandProfile.LabelColor;

                var image = item.Q<Image>();
                image.image = commandProfile.Icon;
                image.tintColor = commandProfile.IconColor;

                var iconBox = item.Q<VisualElement>("IconBox");
                iconBox.style.display = commandProfile.Icon == null ? DisplayStyle.None : DisplayStyle.Flex;

                item.userData = index;
            };

            var (leftPane, rightPane) = UIToolkitUtil.CreateSplitView(root, 1, 100, viewDataKey: nameof(CommandPickerPanel));
            leftPane.Add(categoryList);
            rightPane.Add(commandList);

            parent.Add(root);

            Reload();
        }

        public void Reload()
        {
            _categoryNames = CommandDatabase.GetCategories().Select(x => x.categoryName).Append(null).ToList();
            _categoryListView.itemsSource = _categoryNames;
            _commandListView.RefreshItems();
            _commandTypes = null;
            _commandListView.itemsSource = null;
        }
    }
}