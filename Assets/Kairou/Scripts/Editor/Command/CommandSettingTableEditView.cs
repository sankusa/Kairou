using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [UxmlElement]
    public partial class CommandSettingTableEditView : VisualElement
    {
        readonly PropertyField _priorityField;
        readonly MultiColumnListView _multiColumnListView;

        SerializedObject _serializedObject;
        SerializedProperty _listProp;

        CommandCategorySettingTableSet _categoryTableSet = new();

        public CommandSettingTableEditView()
        {
            _categoryTableSet.Reload();

            _priorityField = new PropertyField();
            Add(_priorityField);

            _multiColumnListView = new MultiColumnListView()
            {
                bindingPath = "_settings._values",
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAddRemoveFooter = true,
                enabledSelf = false,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
            };
            _multiColumnListView.columns.Add(new Column {
                bindingPath = "_typeFullName",
                title = "Type",
                stretchable = true,
                makeCell = () =>
                {
                    var parent = new VisualElement();
                    parent.style.flexDirection = FlexDirection.Row;
                    var icon = new Image();
                    icon.style.width = 16;
                    icon.style.height = 16;
                    var label = new Label();
                    label.RegisterValueChangedCallback(_ =>
                    {
                        icon.image = CommandTypeCache.Types.Any(x => x.FullName == label.text) ? GUICommon.ValidIcon : GUICommon.InvalidIcon;
                    });
                    label.style.flexGrow = 1;
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    var selectButton = new Button(() =>
                    {
                        var menu = new GenericMenu();
                        foreach (var type in CommandTypeCache.Types)
                        {
                            menu.AddItem(new GUIContent(type.FullName.Replace('.', '/')), false, () => label.text = type.FullName);
                        }
                        menu.ShowAsContext();
                    });
                    selectButton.iconImage = GUICommon.DropdownIcon;
                    parent.Add(icon);
                    parent.Add(label);
                    parent.Add(selectButton);
                    return parent;
                },
            });
            _multiColumnListView.columns.Add(new Column { title = "Name", bindingPath = "_name", stretchable = true });
            _multiColumnListView.columns.Add(new Column {
                bindingPath = "_category",
                title = "Category",
                stretchable = true,
                makeCell = () =>
                {
                    var parent = new VisualElement();
                    parent.style.flexDirection = FlexDirection.Row;
                    var icon = new Image();
                    icon.style.width = 16;
                    icon.style.height = 16;
                    var textField = new TextField();
                    textField.RegisterValueChangedCallback(_ =>
                    {
                        icon.image = _categoryTableSet.CategoryNames.Contains(textField.value) ? GUICommon.ValidIcon : GUICommon.InvalidIcon;
                    });
                    textField.style.flexGrow = 1;
                    var selectButton = new Button(() =>
                    {
                        var menu = new GenericMenu();
                        foreach (var categoryName in _categoryTableSet.CategoryNames)
                        {
                            menu.AddItem(new GUIContent(categoryName), false, () => textField.value = categoryName);
                        }
                        menu.ShowAsContext();
                    });
                    selectButton.iconImage = GUICommon.DropdownIcon;
                    parent.Add(icon);
                    parent.Add(textField);
                    parent.Add(selectButton);
                    return parent;
                },
            });
            _multiColumnListView.columns.Add(new Column { title = "Icon", bindingPath = "_icon", stretchable = true });
            _multiColumnListView.columns.Add(new Column { title = "IconColor", bindingPath = "_iconColor", stretchable = true });
            _multiColumnListView.columns.Add(new Column { title = "SummaryPosition", bindingPath = "_summaryPosition", stretchable = true });
            _multiColumnListView.columns.Add(new Column { title = "Script", bindingPath = "_script", stretchable = true });

            Add(_multiColumnListView);
        }

        public void Bind(CommandSettingTable table)
        {
            _serializedObject = new SerializedObject(table);
            _listProp = _serializedObject.FindProperty("_settings._values");

            _priorityField.BindProperty(_serializedObject.FindProperty("_priority"));
            _multiColumnListView.Bind(_serializedObject);

            _multiColumnListView.enabledSelf = true;

            _multiColumnListView.Rebuild();
            _multiColumnListView.RefreshItems();
        }
    }
}