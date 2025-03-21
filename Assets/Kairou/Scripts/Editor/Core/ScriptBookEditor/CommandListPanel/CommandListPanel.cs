using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    public delegate void CommandSpecificAction(IScriptBookOwner owner, int pageIndex, int commandIndex);

    [Serializable]
    public class CommandListPanel
    {
        [SerializeField] Object _scriptBookOwnerObject;
        [SerializeField] int _pageIndex;
        IScriptBookOwner ScriptBookOwner => _scriptBookOwnerObject as IScriptBookOwner;
        bool ExistsTargetPage => ScriptBookOwner != null && ScriptBookOwner.ScriptBook.Pages.HasElementAt(_pageIndex);

        [SerializeField] AdvancedDropdownState _commandDropdownState = new();

        ListView _listView;

        public void Initialize(VisualElement parent, VisualTreeAsset commandListPanelUXML, CommandSpecificAction onSelectionChanged)
        {
            Initialize_Core(parent, commandListPanelUXML, onSelectionChanged);
            SetTarget(_scriptBookOwnerObject as IScriptBookOwner, _pageIndex);
        }

        void Initialize_Core(VisualElement parent, VisualTreeAsset commandListPanelUXML, CommandSpecificAction onSelectionChanged)
        {
            // AdvancedDropdown
            var commandAdvancedDropdown = new CommandAdvancedDropdown(_commandDropdownState);
            commandAdvancedDropdown.OnSelected += command =>
            {
                ScriptBookOwnerUtilForEditor.AddCommand(ScriptBookOwner, _pageIndex, command);
                _listView.Rebuild();
            };

            // ListView
            var commandListPanel = commandListPanelUXML.Instantiate();
            parent.Add(commandListPanel);

            _listView = commandListPanel.Q<ListView>("CommandList");
            _listView.bindItem = (element, i) =>
            {
                Command command = ScriptBookOwner.ScriptBook.Pages[_pageIndex].Commands[i];
                CommandInfoAttribute commandInfo = command.GetType().GetCustomAttribute<CommandInfoAttribute>();
                element.Q<Label>("NameLabel").text = commandInfo.CommandName;
                element.Q<Label>("SummaryLabel").text = command.GetSummary();
            };

            _listView.onAdd = _ =>
            {
                if (ExistsTargetPage == false) return;
                var rect = _listView.worldBound;
                commandAdvancedDropdown.Show(rect);
            };

            _listView.onRemove = _ =>
            {
                if (ExistsTargetPage == false) return;
                ScriptBookOwnerUtilForEditor.RemoveCommand(ScriptBookOwner, _pageIndex, _listView.selectedIndex);
                _listView.Rebuild();
            };

            _listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                if (ExistsTargetPage == false) return;
                // Since there is no event to overwrite the swap process with a custom implementation,
                // the move is first reverted, recorded with Undo, and then moved again.
                ScriptBookOwner.ScriptBook.Pages[_pageIndex].MoveCommand(toIndex, fromIndex);
                ScriptBookOwnerUtilForEditor.MoveCommand(ScriptBookOwner, _pageIndex, fromIndex, toIndex);
                _listView.Rebuild();
            };

            _listView.selectedIndicesChanged += commandIndices =>
            {
                if (ExistsTargetPage == false) return;
                var selectedCommandIndex = commandIndices.FirstOrDefault();
                onSelectionChanged?.Invoke(ScriptBookOwner, _pageIndex, selectedCommandIndex);
            };
        }

        public void SetTarget(IScriptBookOwner scriptBookOwner, int pageIndex)
        {
            ThrowIfNotInitialized();

            _scriptBookOwnerObject = scriptBookOwner?.AsObject();
            _pageIndex = pageIndex;
            Reload();
        }

        public void Reload()
        {
            ThrowIfNotInitialized();

            if (ExistsTargetPage)
            {
                _listView.itemsSource = ScriptBookOwner.ScriptBook.Pages[_pageIndex].Commands as IList;
                _listView.enabledSelf = true;
            }
            else
            {
                _listView.itemsSource = null;
                _listView.enabledSelf = false;
            }
            _listView.selectedIndex = -1;
        }

        public void Rebuild() => _listView.Rebuild();

        public void OnUndoRedoPerformed()
        {
            ThrowIfNotInitialized();
            _listView.Rebuild();
        }

        void ThrowIfNotInitialized()
        {
            if (_listView == null) throw new InvalidOperationException($"{nameof(CommandListPanel)} is not initialized.");
        }
    }
}