using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [UxmlElement]
    public partial class CommandCategorySettingTableEditView : VisualElement
    {
        readonly PropertyField _priorityField;
        readonly MultiColumnListView _multiColumnListView;

        public CommandCategorySettingTableEditView()
        {
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
            _multiColumnListView.columns.Add(new Column { bindingPath = "_name", title = "Name", stretchable = true });
            _multiColumnListView.columns.Add(new Column { bindingPath = "_priority", title = "Priority", stretchable = true });
            _multiColumnListView.columns.Add(new Column { bindingPath = "_defaultCommandIcon", title = "DefaultCommandIcon", stretchable = true });
            _multiColumnListView.columns.Add(new Column { bindingPath = "_defaultCommandIconColor", title = "DefaultCommandIconColor", stretchable = true });
            _multiColumnListView.columns.Add(new Column { bindingPath = "_labelColor", title = "LabelColor", stretchable = true });
            _multiColumnListView.columns.Add(new Column { bindingPath = "_backgroundColor", title = "BackgroundColor", stretchable = true });

            Add(_multiColumnListView);
        }

        public void Bind(CommandCategorySettingTable table)
        {
            var so = new SerializedObject(table);

            _priorityField.BindProperty(so.FindProperty("_priority"));
            _multiColumnListView.Bind(so);

            _multiColumnListView.enabledSelf = true;
        }
    }
}