using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
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
        bool ExistsTargetPage => _bookHolder.HasValidBook && _bookHolder.Book.Pages.HasElementAt(_pageIndex);

        [SerializeField] int _selectedCommandIndex;

        [SerializeField] AdvancedDropdownState _commandDropdownState = new();

        ListView _listView;

        bool IsInitialized => _listView != null;

        ActionDebouncer _refleshDebouncer;

        CommandDatabase _commandDatabase => CommandDatabase.Load();

        static readonly Color _selectedRowOverlayColor = new(1, 1, 0.95f, 0.15f);
        static readonly Color _hoverRowOverlayColor = new(0, 0, 0, 0.15f);

        static readonly Color _summaryColor = new Func<Color>(() =>
        {
            ColorUtility.TryParseHtmlString("#C6B8A3", out Color color);
            return color;
        })();

        public void Initialize(VisualElement parent, VisualTreeAsset commandListPanelUXML, CommandSpecificAction onSelectionChanged, Action onCollectionChanged)
        {
            // AdvancedDropdown
            var commandAdvancedDropdown = new CommandAdvancedDropdown(_commandDropdownState);
            commandAdvancedDropdown.OnSelected += command =>
            {
                // BookUtilForEditor.AddCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, command);
                int insertIndex = _listView.selectedIndex == -1 ? _bookHolder.Book.Pages[_pageIndex].Commands.Count : (_listView.selectedIndex + 1);
                BookUtilForEditor.InsertCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, insertIndex, command);
                _listView.Rebuild();
                onCollectionChanged?.Invoke();
                _listView.SetSelection(insertIndex);
            };

            // ListView
            var commandListPanel = commandListPanelUXML.Instantiate();
            parent.Add(commandListPanel);

            _listView = commandListPanel.Q<ListView>("CommandList");
            _listView.makeItem = () =>
            {
                var item = _listView.itemTemplate.CloneTree();
                var iconBox = item.Q<VisualElement>("IconBox");
                iconBox.Add(new Image());
                var copyButton = item.Q<Button>("CopyButton");
                copyButton.clicked += () =>
                {
                    int index = (int)item.userData;
                    var sourceCommand = _bookHolder.Book.Pages[_pageIndex].Commands[index];
                    var copied = sourceCommand.Copy();
                    BookUtilForEditor.InsertCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, index, copied);
                    _listView.Rebuild();
                    onCollectionChanged?.Invoke();
                };
                var copyIcon = new Image() { image = GUISkin.Instance.copyIcon };
                copyIcon.style.width = 14;
                copyIcon.style.height = 14;
                copyIcon.style.opacity = 0.5f;
                copyButton.Add(copyIcon);

                var deleteButton = item.Q<Button>("DeleteButton");
                deleteButton.clicked += () =>
                {
                    int index = (int)item.userData;
                    var command = _bookHolder.Book.Pages[_pageIndex].Commands[index];
                    BookUtilForEditor.RemoveCommand(_bookHolder.Owner, _bookHolder.Book, _pageIndex, index);
                    _listView.Rebuild();
                    onCollectionChanged?.Invoke();
                };
                var deleteIcon = new Image() { image = GUISkin.Instance.deleteIcon };
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
                        overlay.style.backgroundColor = _hoverRowOverlayColor;
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
                Command command = _bookHolder.Book.Pages[_pageIndex].Commands[i];
                Type commandType = command.GetType();
                AsyncCommand asyncCommand = command as AsyncCommand;
                CommandInfoAttribute commandInfo = command.GetType().GetCustomAttribute<CommandInfoAttribute>();
                var commandProfile = _commandDatabase.GetProfile(commandType);
                var (icon, iconColor) = commandProfile.Icon;

                element.parent.style.paddingTop = 0;
                element.parent.style.paddingBottom = 0;
                element.parent.style.paddingLeft = 0;
                element.parent.style.paddingRight = 0;
                element.parent.parent.Q<VisualElement>("unity-list-view__reorderable-handle").style.display = DisplayStyle.None;
                var asyncCommandMark = element.Q<VisualElement>("AsyncCommandMark");
                asyncCommandMark.visible = asyncCommand != null;
                var notAwaitIcon = element.Q<VisualElement>("NotAwaitIcon");
                notAwaitIcon.style.display = (asyncCommand != null && asyncCommand.AsyncCommandParameter.Await == false) ? DisplayStyle.Flex : DisplayStyle.None;
                var summaryBox = element.Q<VisualElement>("SummaryBox");
                summaryBox.style.backgroundColor = commandProfile.SummaryBackgoundColor;
                var iconBox = element.Q<VisualElement>("IconBox");
                iconBox.style.display = icon == null ? DisplayStyle.None : DisplayStyle.Flex;
                var iconImage = iconBox.Q<Image>();
                iconImage.image = icon;
                iconImage.tintColor = iconColor;
                var nameLabel = element.Q<Label>("NameLabel");
                nameLabel.text = commandProfile.Name;
                nameLabel.style.color = commandProfile.NameColor;
                var summaryLabel = element.Q<Label>("SummaryLabel");
                summaryLabel.text = command.GetSummary();
                summaryLabel.style.color = _summaryColor;
                var indentBox = element.Q<VisualElement>("IndentBox");
                indentBox.style.width = 10 * command.CalculateBlockLevel();
                indentBox.style.flexShrink = 0;
                indentBox.style.flexGrow = 0;

                var summaryMainBox = element.Q<VisualElement>("SummaryMainBox");
                var summaryIndent = element.Q<VisualElement>("SummaryIndent");
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
                // onSelectionChanged?.Invoke(_bookHolder.BookId, _pageIndex, toIndex);
                _listView.selectedIndex = toIndex;
            };

            _listView.selectedIndicesChanged += commandIndices =>
            {
                _selectedCommandIndex = _listView.selectedIndex;
                UpdateOverlayColor();
                if (ExistsTargetPage == false) return;
                var selectedCommandIndex = commandIndices.FirstOrDefault();
                onSelectionChanged?.Invoke(_bookHolder.BookId, _pageIndex, selectedCommandIndex);
            };

            _refleshDebouncer = new ActionDebouncer(_listView, 0.05f, 5, () => _listView.RefreshItems());
            
            Reload();
        }

        void UpdateOverlayColor()
        {
            for (int i = 0; i < _listView.itemsSource.Count; i++)
            {
                UpdateOverlayColor(i);
            }
        }

        void UpdateOverlayColor(int index)
        {
            var row = _listView.GetRootElementForIndex(index);
            if (row == null) return;
            row.Q<VisualElement>("Overlay").style.backgroundColor = _listView.selectedIndices.Contains(index) ? _selectedRowOverlayColor : Color.clear;
        }

        public void SetTarget(BookId bookId, int pageIndex)
        {
            int selectedCommandIndex = (bookId == _bookHolder.BookId && pageIndex == _pageIndex) ? _selectedCommandIndex : 0;
            _bookHolder.Reset(bookId);
            _pageIndex = pageIndex;
            if (IsInitialized) Reload(selectedCommandIndex);
        }

        public void Reload()
        {
            Reload(_selectedCommandIndex);
        }

        public void Reload(int selectedCommandIndex)
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
            _listView.SetSelectionWithoutNotify(new int[] {selectedCommandIndex});
        }

        public void Reflesh() => _refleshDebouncer.Schedule();

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
            _listView.Rebuild();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(CommandListPanel)} is not initialized.");
        }
    }
}