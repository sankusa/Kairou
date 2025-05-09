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
    [Serializable]
    public class CommandListPanel
    {
        public delegate void CommandSpecificAction(ScriptBookId scriptBookId, int pageIndex, int commandIndex);

        [SerializeField] RestorableScriptBookHolder _bookHolder = new();
        [SerializeField] int _pageIndex;
        bool ExistsTargetPage => _bookHolder.ScriptBook != null && _bookHolder.ScriptBook.Pages.HasElementAt(_pageIndex);

        [SerializeField] AdvancedDropdownState _commandDropdownState = new();

        ListView _listView;

        bool IsInitialized => _listView != null;

        public void Initialize(VisualElement parent, VisualTreeAsset commandListPanelUXML, CommandSpecificAction onSelectionChanged, Action onCollectionChanged)
        {
            // AdvancedDropdown
            var commandAdvancedDropdown = new CommandAdvancedDropdown(_commandDropdownState);
            commandAdvancedDropdown.OnSelected += command =>
            {
                ScriptBookUtilForEditor.AddCommand(_bookHolder.Owner, _bookHolder.ScriptBook, _pageIndex, command);
                _listView.Rebuild();
            };

            // ListView
            var commandListPanel = commandListPanelUXML.Instantiate();
            parent.Add(commandListPanel);

            _listView = commandListPanel.Q<ListView>("CommandList");
            _listView.bindItem = (element, i) =>
            {
                Command command = _bookHolder.ScriptBook.Pages[_pageIndex].Commands[i];
                CommandInfoAttribute commandInfo = command.GetType().GetCustomAttribute<CommandInfoAttribute>();
                element.Q<Label>("NameLabel").text = commandInfo.CommandName;
                element.Q<Label>("SummaryLabel").text = command.GetSummary();

                string errorMessage = string.Join('\n', command.InvokeValidate());
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
                ScriptBookUtilForEditor.RemoveCommand(_bookHolder.Owner,_bookHolder.ScriptBook, _pageIndex, _listView.selectedIndex);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
            };

            _listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                if (ExistsTargetPage == false) return;
                // Since there is no event to overwrite the swap process with a custom implementation,
                // the move is first reverted, recorded with Undo, and then moved again.
                _bookHolder.ScriptBook.Pages[_pageIndex].MoveCommand(toIndex, fromIndex);
                ScriptBookUtilForEditor.MoveCommand(_bookHolder.Owner, _bookHolder.ScriptBook, _pageIndex, fromIndex, toIndex);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
                // When dragging and swapping ListView elements, selectedIndicesChanged is not triggered, so it is manually triggered here instead.
                onSelectionChanged?.Invoke(_bookHolder.ScriptBookId, _pageIndex, toIndex);
            };

            _listView.selectedIndicesChanged += commandIndices =>
            {
                if (ExistsTargetPage == false) return;
                var selectedCommandIndex = commandIndices.FirstOrDefault();
                onSelectionChanged?.Invoke(_bookHolder.ScriptBookId, _pageIndex, selectedCommandIndex);
            };
            
            Reload();
        }

        public void SetTarget(ScriptBookId scriptBookId, int pageIndex)
        {
            _bookHolder.Reset(scriptBookId);
            _pageIndex = pageIndex;
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            ThrowIfNotInitialized();

            if (ExistsTargetPage)
            {
                _listView.itemsSource = _bookHolder.ScriptBook.Pages[_pageIndex].Commands;
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

        public void OnProjectOrHierarchyChanged()
        {
            if (_bookHolder.RestoreObjectIfNull())
            {
                Reload();
            }
        }

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