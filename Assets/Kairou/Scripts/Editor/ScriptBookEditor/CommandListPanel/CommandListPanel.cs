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
        public delegate void CommandSpecificAction(BookId bookId, int pageIndex, int commandIndex);

        [SerializeField] RestorableBookHolder _bookHolder = new();
        [SerializeField] int _pageIndex;
        bool ExistsTargetPage => _bookHolder.Book != null && _bookHolder.Book.Pages.HasElementAt(_pageIndex);

        [SerializeField] AdvancedDropdownState _commandDropdownState = new();

        ListView _listView;

        bool IsInitialized => _listView != null;

        public void Initialize(VisualElement parent, VisualTreeAsset commandListPanelUXML, CommandSpecificAction onSelectionChanged, Action onCollectionChanged)
        {
            // AdvancedDropdown
            var commandAdvancedDropdown = new CommandAdvancedDropdown(_commandDropdownState);
            commandAdvancedDropdown.OnSelected += command =>
            {
                BookUtilForEditor.AddCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, command);
                _listView.Rebuild();
            };

            // ListView
            var commandListPanel = commandListPanelUXML.Instantiate();
            parent.Add(commandListPanel);

            _listView = commandListPanel.Q<ListView>("CommandList");
            _listView.bindItem = (element, i) =>
            {
                Command command = _bookHolder.Book.Pages[_pageIndex].Commands[i];
                CommandInfoAttribute commandInfo = command.GetType().GetCustomAttribute<CommandInfoAttribute>();
                element.Q<Label>("NameLabel").text = commandInfo.CommandName;
                element.Q<Label>("SummaryLabel").text = command.GetSummary();
                var indentBox = element.Q<VisualElement>("IndentBox");
                indentBox.style.width = 12 * command.CalculateBlockLevel();
                indentBox.style.flexShrink = 0;
                indentBox.style.flexGrow = 0;

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
                BookUtilForEditor.RemoveCommand(_bookHolder.Owner,_bookHolder.Book, _pageIndex, _listView.selectedIndex);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
            };

            _listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                if (ExistsTargetPage == false) return;
                // Since there is no event to overwrite the swap process with a custom implementation,
                // the move is first reverted, recorded with Undo, and then moved again.
                _bookHolder.Book.Pages[_pageIndex].MoveCommand(toIndex, fromIndex);
                BookUtilForEditor.MoveCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, fromIndex, toIndex);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
                // When dragging and swapping ListView elements, selectedIndicesChanged is not triggered, so it is manually triggered here instead.
                onSelectionChanged?.Invoke(_bookHolder.BookId, _pageIndex, toIndex);
            };

            _listView.selectedIndicesChanged += commandIndices =>
            {
                if (ExistsTargetPage == false) return;
                var selectedCommandIndex = commandIndices.FirstOrDefault();
                onSelectionChanged?.Invoke(_bookHolder.BookId, _pageIndex, selectedCommandIndex);
            };
            
            Reload();
        }

        public void SetTarget(BookId bookId, int pageIndex)
        {
            _bookHolder.Reset(bookId);
            _pageIndex = pageIndex;
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            ThrowIfNotInitialized();

            if (ExistsTargetPage)
            {
                _listView.itemsSource = _bookHolder.Book.Pages[_pageIndex].Commands as IList;
                _listView.enabledSelf = true;
            }
            else
            {
                _listView.itemsSource = null;
                _listView.enabledSelf = false;
            }
            _listView.selectedIndex = 0;
        }

        public void Reflesh() => _listView.RefreshItems();//  _listView.Rebuild();

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