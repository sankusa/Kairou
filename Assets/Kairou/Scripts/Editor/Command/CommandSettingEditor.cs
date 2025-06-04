using UnityEditor;
using UnityEditor.UIElements;

namespace Kairou.Editor
{
    public class CommandSettingEditor : EditorWindow
    {
        [MenuItem("Window/" + nameof(Kairou) + "/" + nameof(CommandSettingEditor))]
        static void Open()
        {
            GetWindow<CommandSettingEditor>();
        }

        CommandSettingTableSet _tableSet = new();

        ObjectDropdown _objectDropdown;
        CommandSettingTableEditView _tableEditView;

        void CreateGUI()
        {
            // UIの初期化
            var header = new Toolbar();

            _objectDropdown = new ObjectDropdown();
            _objectDropdown.SetUp<CommandSettingTable>("Edit Target", t => $"{t.Priority}: {t.name}", t => _tableEditView.Bind(t));

            var headerSpacer = new ToolbarSpacer();
            headerSpacer.style.flexGrow = 1;

            var reloadButton = new ToolbarButton();
            reloadButton.text = "Reload";
            reloadButton.clicked += () => Reload();

            header.Add(_objectDropdown);
            header.Add(headerSpacer);
            header.Add(reloadButton);

            _tableEditView = new CommandSettingTableEditView();
            
            rootVisualElement.Add(header);
            rootVisualElement.Add(_tableEditView);

            Reload();
        }

        void Reload()
        {
            // データをセット
            _tableSet.Reload();
            _objectDropdown.SetObjects(_tableSet.Tables);
        }
    }
}