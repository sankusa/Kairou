using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using System.Collections.Generic;

namespace Kairou.Editor
{
    [Serializable]
    public class CommandListPanel
    {
        [Serializable]
        public class CommandList
        {
            [SerializeReference] List<Command> _commands;
            public List<Command> Commands
            {
                get => _commands;
                set => _commands = value;
            }
        }

        [SerializeField] RestorableBookHolder _bookHolder = new();
        [SerializeField] int _pageIndex;
        bool ExistsTargetPage => _bookHolder.HasValidBook && _bookHolder.Book.Pages.HasElementAt(_pageIndex);

        [SerializeField] int _selectedCommandIndex;

        [SerializeField] AdvancedDropdownState _commandDropdownState = new();

        ListView _listView;

        bool IsInitialized => _listView != null;

        Action<int> _onSelectionChanged;
        Action _onCollectionChanged;

        ActionDebouncer _refreshDebouncer;

        CommandDatabase _commandDatabase;
        CommandDatabase CommandDatabase => _commandDatabase ??= CommandDatabase.Load();

        List<int> _selectedIndicesOld = new();

        public void Initialize(VisualElement parent, VisualTreeAsset commandListPanelUXML, Action<int> onSelectionChanged, Action onCollectionChanged)
        {
            _onSelectionChanged = onSelectionChanged;
            _onCollectionChanged = onCollectionChanged;

            // AdvancedDropdown
            var commandAdvancedDropdown = new CommandAdvancedDropdown(_commandDropdownState);
            commandAdvancedDropdown.OnSelected += commandType =>
            {
                InsertCommand(commandType);
            };

            // ListView
            var commandListPanel = commandListPanelUXML.Instantiate();
            parent.Add(commandListPanel);

            _listView = commandListPanel.Q<ListView>("CommandList");
            _listView.makeItem = () =>
            {
                var item = _listView.itemTemplate.CloneTree();
                var rowNumberLabel = item.Q<Label>("RowNumberLabel");
                item.style.color = Color.grey;
                var iconBox = item.Q<VisualElement>("IconBox");
                iconBox.Add(new Image());
                var copyButton = item.Q<Button>("CopyButton");
                copyButton.clicked += () =>
                {
                    int index = (int)item.userData;
                    var sourceCommand = _bookHolder.Book.Pages[_pageIndex].Commands[index];
                    var copied = sourceCommand.Copy();
                    BookUtilForEditor.InsertCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, index, copied);
                    _listView.RefreshItems();
                    _onCollectionChanged?.Invoke();
                };
                var copyIcon = new Image() { image = GUISkin.Instance.CopyIcon };
                copyIcon.style.width = 14;
                copyIcon.style.height = 14;
                copyIcon.style.opacity = 0.5f;
                copyButton.Add(copyIcon);

                var deleteButton = item.Q<Button>("DeleteButton");
                deleteButton.clicked += () =>
                {
                    int index = (int)item.userData;
                    RemoveCommand(index);
                };
                var deleteIcon = new Image() { image = GUISkin.Instance.DeleteIcon };
                deleteIcon.style.width = 14;
                deleteIcon.style.height = 14;
                deleteIcon.style.opacity = 0.5f;
                deleteButton.Add(deleteIcon);

                var overlay = item.Q<VisualElement>("Overlay");
                item.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    int idnex = (int)item.userData;
                    if (_listView.selectedIndices.Contains(idnex) == false)
                    {
                        overlay.style.backgroundColor = GUICommon.HoverOverlayColor;
                    }
                });
                item.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    int idnex = (int)item.userData;
                    if (_listView.selectedIndices.Contains(idnex) == false)
                    {
                        overlay.style.backgroundColor = Color.clear;
                    }
                });

                return item;
            };

            _listView.bindItem = (element, i) =>
            {
                element.userData = i;

                element.parent.style.paddingTop = 0;
                element.parent.style.paddingBottom = 0;
                element.parent.style.paddingLeft = 0;
                element.parent.style.paddingRight = 0;
                element.parent.parent.Q<VisualElement>("unity-list-view__reorderable-handle").style.display = DisplayStyle.None;
                var rowNumberLabel = element.Q<Label>("RowNumberLabel");
                rowNumberLabel.text = (i + 1).ToString();
                rowNumberLabel.style.width = _bookHolder.Book.Pages[_pageIndex].Commands.Count.ToString().Length * 8 + 2;
                var asyncCommandMark = element.Q<VisualElement>("AsyncCommandMark");
                var notAwaitIcon = element.Q<VisualElement>("NotAwaitIcon");
                var summaryBox = element.Q<VisualElement>("SummaryBox");
                var iconBox = element.Q<VisualElement>("IconBox");
                var iconImage = iconBox.Q<Image>();
                var nameLabel = element.Q<Label>("NameLabel");
                var summaryLabel = element.Q<Label>("SummaryLabel");
                summaryLabel.style.color = GUISkin.Instance.DefaultSummaryColor;
                var disableOverlay = element.Q<VisualElement>("DisableOverlay");
                var indentBox = element.Q<VisualElement>("IndentBox");
                indentBox.style.flexShrink = 0;
                indentBox.style.flexGrow = 0;
                var summaryMainBox = element.Q<VisualElement>("SummaryMainBox");
                var summaryIndent = element.Q<VisualElement>("SummaryIndent");
                var errorBox = element.Q<VisualElement>("ErrorBox");

                Command command = _bookHolder.Book.Pages[_pageIndex].Commands[i];
                if (command == null)
                {
                    asyncCommandMark.visible = false;
                    notAwaitIcon.style.display = DisplayStyle.None;
                    summaryBox.style.backgroundColor = Color.clear;
                    iconBox.style.display = DisplayStyle.None;
                    iconImage.image = null;
                    iconImage.tintColor = Color.clear;
                    nameLabel.text = "Null";
                    nameLabel.style.color = Color.red;
                    disableOverlay.style.display = DisplayStyle.None;
                    summaryLabel.text = string.Empty;
                    indentBox.style.display = DisplayStyle.None;
                    summaryIndent.style.display = DisplayStyle.None;
                    errorBox.style.display = DisplayStyle.None;
                    return;
                }

                Type commandType = command.GetType();
                AsyncCommand asyncCommand = command as AsyncCommand;
                CommandInfoAttribute commandInfo = command.GetType().GetCustomAttribute<CommandInfoAttribute>();
                var commandProfile = CommandDatabase.GetProfile(commandType);
                var icon = commandProfile.Icon;
                var iconColor = commandProfile.IconColor;
                int blockLevel = command.CalculateBlockLevel();

                asyncCommandMark.visible = asyncCommand != null;
                notAwaitIcon.style.display = (asyncCommand != null && asyncCommand.AsyncCommandParameter.Await == false) ? DisplayStyle.Flex : DisplayStyle.None;
                summaryBox.style.backgroundColor = commandProfile.BackgoundColor;
                iconBox.style.display = icon == null ? DisplayStyle.None : DisplayStyle.Flex;
                iconImage.image = icon;
                iconImage.tintColor = iconColor;
                nameLabel.text = commandProfile.Name;
                nameLabel.style.color = commandProfile.LabelColor;
                summaryLabel.text = command.GetSummary();
                disableOverlay.style.display = command.Enable ? DisplayStyle.None : DisplayStyle.Flex;
                indentBox.style.display = blockLevel == 0 ? DisplayStyle.None : DisplayStyle.Flex;
                indentBox.style.width = 10 * blockLevel;

                if (commandProfile.SummaryPositionType == SummaryPositionType.Right)
                {
                    summaryMainBox.style.flexDirection = FlexDirection.Row;
                    summaryIndent.style.display = DisplayStyle.None;
                    nameLabel.style.flexGrow = 0;
                }
                else if (commandProfile.SummaryPositionType == SummaryPositionType.Bottom)
                {
                    summaryMainBox.style.flexDirection = FlexDirection.Column;
                    summaryIndent.style.display = DisplayStyle.Flex;
                    nameLabel.style.flexGrow = 1;
                }

                UpdateOverlayColor(i);

                string errorMessage = string.Join('\n', command.InvokeValidate());
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorBox.style.display = DisplayStyle.None;
                }
                else
                {
                    errorBox.style.display = DisplayStyle.Flex;

                    var helpBox = errorBox.Q<HelpBox>();
                    if (helpBox == null)
                    {
                        helpBox = new HelpBox("", HelpBoxMessageType.Error);
                        helpBox.Q<VisualElement>(className: "unity-help-box__icon").style.minHeight = 16;
                        errorBox.Add(helpBox);
                    }
                    helpBox.text = errorMessage;
                }
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
                RemoveCommand(_listView.selectedIndex);
            };

            _listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                if (ExistsTargetPage == false) return;
                // Since there is no event to overwrite the swap process with a custom implementation,
                // the move is first reverted, recorded with Undo, and then moved again.
                _bookHolder.Book.Pages[_pageIndex].MoveCommand(toIndex, fromIndex);
                BookUtilForEditor.MoveCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, fromIndex, toIndex);
                //_listView.RefreshItems(); // 入れ替え時、自動的にリフレッシュされるので不要
                _onCollectionChanged?.Invoke();
                // When dragging and swapping ListView elements, selectedIndicesChanged is not triggered, so it is manually triggered here instead.
                _listView.selectedIndex = toIndex;
            };

            _listView.selectedIndicesChanged += indices =>
            {
                if (ExistsTargetPage == false) return;
                UpdateOverlayColor(_selectedIndicesOld);
                UpdateOverlayColor(indices);
                if (_listView.selectedIndex != -1 && _listView.selectedIndex != _selectedCommandIndex)
                {
                    _selectedCommandIndex = _listView.selectedIndex;
                    _onSelectionChanged?.Invoke(_selectedCommandIndex);
                    _selectedIndicesOld = indices.ToList();
                }
            };

            _listView.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.ctrlKey && evt.keyCode == KeyCode.C)
                {
                    var commands = new CommandList();
                    commands.Commands = _listView.selectedIndices.OrderBy(x => x).Select(x => _bookHolder.Book.Pages[_pageIndex].Commands[x]).ToList();
                    string json = EditorJsonUtility.ToJson(commands);
                    EditorGUIUtility.systemCopyBuffer = json;
                    evt.StopImmediatePropagation();
                }

                if (evt.ctrlKey && evt.keyCode == KeyCode.X)
                {
                    var commands = new CommandList();
                    commands.Commands = _listView.selectedIndices.OrderBy(x => x).Select(x => _bookHolder.Book.Pages[_pageIndex].Commands[x]).ToList();
                    string json = EditorJsonUtility.ToJson(commands);
                    EditorGUIUtility.systemCopyBuffer = json;
                    RemoveCommands(commands.Commands);
                    evt.StopImmediatePropagation();
                }

                if (evt.ctrlKey && evt.keyCode == KeyCode.V)
                {
                    if (string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer)) return;
                    var commands = new CommandList();
                    try
                    {
                        EditorJsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, commands);
                    }
                    catch (ArgumentException) {}
                    if (commands.Commands != null)
                    {
                        InsertCommands(commands.Commands);
                    }
                }
            });

            _refreshDebouncer = new ActionDebouncer(_listView, 0.05f, 5, () => _listView.RefreshItems());
            
            Reload();
        }

        void UpdateOverlayColor(IEnumerable<int> indices)
        {
            foreach (var index in indices)
            {
                UpdateOverlayColor(index);
            }
        }

        void UpdateOverlayColor(int index)
        {
            var row = _listView.GetRootElementForIndex(index);
            if (row == null) return;
            row.Q<VisualElement>("Overlay").style.backgroundColor = _listView.selectedIndices.Contains(index) ? GUICommon.SelectedOverlayColor : Color.clear;
        }

        public void SetTarget(BookId bookId, int pageIndex)
        {
            int selectedCommandIndex = (bookId == _bookHolder.BookId && pageIndex == _pageIndex) ? _listView.selectedIndex : 0;
            _bookHolder.Reset(bookId);
            _pageIndex = pageIndex;
            if (IsInitialized) Reload(selectedCommandIndex);
        }

        public void Reload()
        {
            Reload(_listView.selectedIndex);
        }

        public void Reload(int selectedCommandIndex)
        {
            if (IsInitialized == false) return;

            if (ExistsTargetPage)
            {
                // _listView.bindingPath = $"{_bookHolder.BookPropertyPath}._pages.Array.data[{_pageIndex}]._commands";
                // _listView.Bind(new SerializedObject(_bookHolder.Owner));
                _listView.itemsSource = _bookHolder.Book.Pages[_pageIndex].Commands as IList;
                _listView.enabledSelf = true;
            }
            else
            {
                _listView.itemsSource = null;
                _listView.enabledSelf = false;
            }
            _listView.SetSelectionWithoutNotify(new int[] {selectedCommandIndex});
        }

        public void Refresh() => _refreshDebouncer.Schedule();

        public void OnProjectOrHierarchyChanged()
        {
            if (_bookHolder.RestoreObjectIfNull())
            {
                Reload();
            }
        }

        public void OnUndoRedoPerformed()
        {
            if (IsInitialized == false) return;
            _listView.RefreshItems();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(CommandListPanel)} is not initialized.");
        }

        public void InsertCommand(Type commandType)
        {
            var command = Command.CreateInstance(commandType);
            int insertIndex = Mathf.Min(_listView.selectedIndex + 1, _bookHolder.Book.Pages[_pageIndex].Commands.Count);
            BookUtilForEditor.InsertCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, insertIndex, command);
            _listView.RefreshItems();
            _onCollectionChanged?.Invoke();
            _listView.SetSelection(insertIndex);
            _listView.ScrollToItem(insertIndex);
        }

        void InsertCommands(IEnumerable<Command> commands)
        {
            int insertIndex = Mathf.Min(_listView.selectedIndex + 1, _bookHolder.Book.Pages[_pageIndex].Commands.Count);
            BookUtilForEditor.InsertCommands(_bookHolder.Owner, _bookHolder.Book, _pageIndex, insertIndex, commands);
            _listView.RefreshItems();
            _onCollectionChanged?.Invoke();
            _listView.SetSelection(Enumerable.Range(insertIndex, commands.Count()));
            _listView.ScrollToItem(insertIndex + commands.Count() - 1);
        }

        void RemoveCommand(int index)
        {
            if (_bookHolder.Book.Pages[_pageIndex].Commands.HasElementAt(index) == false) return;

            var command = _bookHolder.Book.Pages[_pageIndex].Commands[index];
            BookUtilForEditor.RemoveCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, index);
            _listView.RefreshItems();
            _onCollectionChanged?.Invoke();

            if (index == _listView.selectedIndex)
            {
                _onSelectionChanged?.Invoke(index);
            }
        }

        void RemoveCommands(IEnumerable<Command> commands)
        {
            var selectedCommand = _bookHolder.Book.Pages[_pageIndex].Commands.HasElementAt(_listView.selectedIndex) ? _bookHolder.Book.Pages[_pageIndex].Commands[_listView.selectedIndex] : null;

            BookUtilForEditor.RemoveCommands(_bookHolder.Owner, _bookHolder.Book, _pageIndex, commands);
            _listView.RefreshItems();
            _onCollectionChanged?.Invoke();

            if (commands.Contains(selectedCommand))
            {
                _onSelectionChanged?.Invoke(_listView.selectedIndex);
            }
        }
    }
}