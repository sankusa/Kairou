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

        bool IsInitialized => _listView != null;

        public void Initialize(VisualElement parent, VisualTreeAsset commandListPanelUXML, CommandSpecificAction onSelectionChanged, Action onCollectionChanged)
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

                string errorMessage = string.Join('\n', command.Validate());
                if (string.IsNullOrEmpty(errorMessage))
                {
                    var errorBox = element.Q<VisualElement>("ErrorBox");
                    errorBox.style.display = DisplayStyle.None;
                }
                else
                {
                    var errorBox = element.Q<VisualElement>("ErrorBox");
                    errorBox.style.display = DisplayStyle.Flex;

                    var helpBox = errorBox.Q<HelpBox>();
                    if (helpBox == null)
                    {
                        helpBox = new HelpBox(errorMessage, HelpBoxMessageType.Error);
                        helpBox.Q<VisualElement>(className: "unity-help-box__icon").style.minHeight = 16;
                        errorBox.Add(helpBox);
                    }
                }
            };

            _listView.onAdd = _ =>
            {
                if (ExistsTargetPage == false) return;
                var rect = _listView.worldBound;
                commandAdvancedDropdown.Show(rect);
                onCollectionChanged?.Invoke();
            };

            _listView.onRemove = _ =>
            {
                if (ExistsTargetPage == false) return;
                ScriptBookOwnerUtilForEditor.RemoveCommand(ScriptBookOwner, _pageIndex, _listView.selectedIndex);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
            };

            _listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                if (ExistsTargetPage == false) return;
                // Since there is no event to overwrite the swap process with a custom implementation,
                // the move is first reverted, recorded with Undo, and then moved again.
                ScriptBookOwner.ScriptBook.Pages[_pageIndex].MoveCommand(toIndex, fromIndex);
                ScriptBookOwnerUtilForEditor.MoveCommand(ScriptBookOwner, _pageIndex, fromIndex, toIndex);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
                // When dragging and swapping ListView elements, selectedIndicesChanged is not triggered, so it is manually triggered here instead.
                onSelectionChanged?.Invoke(ScriptBookOwner, _pageIndex, toIndex);
            };

            _listView.selectedIndicesChanged += commandIndices =>
            {
                if (ExistsTargetPage == false) return;
                var selectedCommandIndex = commandIndices.FirstOrDefault();
                onSelectionChanged?.Invoke(ScriptBookOwner, _pageIndex, selectedCommandIndex);
            };
            
            Reload();
        }

        public void SetTarget(IScriptBookOwner scriptBookOwner, int pageIndex)
        {
            _scriptBookOwnerObject = scriptBookOwner?.AsObject();
            _pageIndex = pageIndex;
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            ThrowIfNotInitialized();

            if (ExistsTargetPage)
            {
                _listView.itemsSource = (IList)ScriptBookOwner.ScriptBook.Pages[_pageIndex].Commands;
                _listView.enabledSelf = true;
            }
            else
            {
                _listView.itemsSource = null;
                _listView.enabledSelf = false;
            }
            _listView.selectedIndex = 0;
        }

        public void Rebuild() => _listView.Rebuild();

        public void OnUndoRedoPerformed()
        {
            ThrowIfNotInitialized();
            _listView.Rebuild();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(CommandListPanel)} is not initialized.");
        }
    }
}