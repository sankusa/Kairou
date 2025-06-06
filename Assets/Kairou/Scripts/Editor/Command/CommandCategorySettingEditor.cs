using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;
using System.Linq;

namespace Kairou.Editor
{
    public class CommandCategorySettingEditor : EditorWindow
    {
        [MenuItem("Window/" + nameof(Kairou) + "/" + nameof(CommandCategorySettingEditor))]
        static void Open()
        {
            GetWindow<CommandCategorySettingEditor>();
        }

        CommandCategorySettingTableSet _tableSet;

        ObjectDropdown _objectDropdown;
        CommandCategorySettingTableEditView _tableEditView;

        void OnEnable()
        {
            _tableSet = CommandCategorySettingTableSet.Load();
        }

        void CreateGUI()
        {
            // UIの初期化
            var header = new Toolbar();

            _objectDropdown = new ObjectDropdown();
            _objectDropdown.SetUp<CommandCategorySettingTable>("Edit Target", t => $"{t.Priority}: {t.name}", t => _tableEditView.Bind(t));

            var headerSpacer = new ToolbarSpacer();
            headerSpacer.style.flexGrow = 1;

            var reloadButton = new ToolbarButton();
            reloadButton.text = "Reload";
            reloadButton.clicked += () => Reload();

            header.Add(_objectDropdown);
            header.Add(headerSpacer);
            header.Add(reloadButton);

            _tableEditView = new CommandCategorySettingTableEditView();
            
            rootVisualElement.Add(header);
            rootVisualElement.Add(_tableEditView);

            Reload();
        }

        void Reload()
        {
            // データをセット
            _objectDropdown.SetObjects(_tableSet.Tables);
        }
    }
}