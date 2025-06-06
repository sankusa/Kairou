using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    public class ScriptBookEditor : EditorWindow
    {

        public static void Open(Object bookOwner, string bookPropertyPath)
        {
            var window = GetWindow<ScriptBookEditor>();
            window.titleContent = new GUIContent("ScriptBookEditor");

            window.SetTarget(bookOwner, bookPropertyPath);
        }

        [SerializeField] VisualTreeAsset _headerPanelUXML;
        [SerializeField] VisualTreeAsset _bookHeaderUXML;
        [SerializeField] VisualTreeAsset _pageListPanelUXML;
        [SerializeField] VisualTreeAsset _pageHeaderPanelUXML;
        [SerializeField] VisualTreeAsset _commandListPanelUXML;
        [SerializeField] VisualTreeAsset _variablePanelUXML;
        [SerializeField] VisualTreeAsset _pagePaneUXML;
        [SerializeField] VisualTreeAsset _commandPaneUXML;
        [SerializeField] VisualTreeAsset _commandPicker_CategoryListUXML;
        [SerializeField] VisualTreeAsset _commandPicker_CommandListUXML;

        [SerializeField] RestorableBookHolder _bookHolder = new();

        [SerializeField] ScriptBookEditorHeaderPanel _headerPanel = new();
        [SerializeField] BookHeaderPanel _bookHeaderPanel = new();
        [SerializeField] PageListPanel _pageListPanel = new();
        [SerializeField] PageHeaderPanel _pageHeaderPanel = new();
        [SerializeField] CommandListPanel _commandListPanel = new();
        [SerializeField] CommandPanel _commandPanel = new();
        [SerializeField] VariablePanel _variablePanel = new();
        [SerializeField] CommandPickerPanel _commandPickerPanel = new();

        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            EditorApplication.projectChanged += OnProjectOrHierarchyChanged;
            EditorApplication.hierarchyChanged += OnProjectOrHierarchyChanged;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.projectChanged -= OnProjectOrHierarchyChanged;
            EditorApplication.hierarchyChanged -= OnProjectOrHierarchyChanged;
        }

        void CreateGUI()
        {
            var root = rootVisualElement;

            _headerPanel.Initialize(rootVisualElement, _headerPanelUXML);

            (var bookPane, var variablePane) = UIToolkitUtil.CreateSplitView(
                out TwoPaneSplitView rootSplitView,
                rootVisualElement,
                fixedPaneIndex: 1,
                viewDataKey: "Split0");

            rootSplitView.schedule.Execute(() =>
            {
                if (_headerPanel.VariableDisplayToggle.value) return;
                rootSplitView.CollapseChild(1);
            });

            _headerPanel.VariableDisplayToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    rootSplitView.UnCollapse();
                }
                else
                {
                    rootSplitView.CollapseChild(1);
                }
            });
            _headerPanel.ReloadButton.clicked += () => Reload();

            _bookHeaderPanel.Initialize(bookPane, _bookHeaderUXML);

            var panes = UIToolkitUtil.CreateSplitView(bookPane, viewDataKey: "Split1");
            var leftPane = panes.leftPane;

            var pagePane = _pagePaneUXML.Instantiate();
            pagePane.style.flexGrow = 1;
            panes.rightPane.Add(pagePane);
            var pagePaneInner = pagePane.Q<VisualElement>("Pane");

            _pageHeaderPanel.Initialize(
                pagePaneInner,
                _pageHeaderPanelUXML,
                () => _pageListPanel.Reload()
            );

            var (centerPane, rightPane) = UIToolkitUtil.CreateSplitView(pagePaneInner, 1, 200f, viewDataKey: "Split2");

            _pageListPanel.Initialize(
                leftPane,
                _pageListPanelUXML,
                (bookId, pageIndex) =>
                {
                    _pageHeaderPanel.SetTarget(bookId, pageIndex);
                    _commandListPanel.SetTarget(bookId, pageIndex);
                    _commandPanel.SetTarget(bookId, pageIndex, 0);
                    _variablePanel.SetTarget(bookId, pageIndex);
                },
                () =>
                {
                    _commandListPanel.Reload();
                    _variablePanel.Reload();
                }
            );

            _commandListPanel.Initialize(
                centerPane,
                _commandListPanelUXML,
                (bookId, pageIndex, commandIndex) => _commandPanel.SetTarget(bookId, pageIndex, commandIndex),
                () => _commandPanel.Reload()
            );

            var (commandPaneParent, commandPickerPane) = UIToolkitUtil.CreateSplitView(rightPane, 1, 200f, TwoPaneSplitViewOrientation.Vertical, viewDataKey: "Split3");

            var commandPane = _commandPaneUXML.Instantiate();
            commandPane.style.flexGrow = 1;
            commandPaneParent.Add(commandPane);
            var commandPaneInner = commandPane.Q<VisualElement>("Pane");

            _commandPanel.Initialize(
                commandPaneInner,
                () => _commandListPanel.Reflesh()
            );

            _commandPickerPanel.Initialize(
                commandPickerPane,
                _commandPicker_CategoryListUXML,
                _commandPicker_CommandListUXML,
                (commandType) => _commandListPanel.InsertCommand(commandType)
            );

            _variablePanel.Initialize(
                variablePane,
                _variablePanelUXML
            );
        }

        void SetTarget(Object bookOwner, string bookPropertyPath)
        {
            _bookHolder.Reset(bookOwner, bookPropertyPath);

            _headerPanel.SetTarget(_bookHolder.BookId);
            _bookHeaderPanel.SetTarget(_bookHolder.BookId);
            _pageListPanel.SetTarget(_bookHolder.BookId);
            _pageHeaderPanel.SetTarget(_bookHolder.BookId, 0);
            _commandListPanel.SetTarget(_bookHolder.BookId, 0);
            _commandPanel.SetTarget(_bookHolder.BookId, 0, 0);
            _variablePanel.SetTarget(_bookHolder.BookId, 0);
        }

        void Reload()
        {
            _headerPanel.Reload();
            _bookHeaderPanel.Reload();
            _pageListPanel.Reload();
            _pageHeaderPanel.Reload();
            _commandListPanel.Reload();
            _commandPanel.Reload();
            _variablePanel.Reload();
            _commandPickerPanel.Reload();
        }

        // PlayMode遷移にも呼ばれる
        void OnProjectOrHierarchyChanged()
        {
            // ObjectがDestroyされた場合など
            if (_bookHolder.RestoreObjectIfNull())
            {

            }
            _headerPanel.OnProjectOrHierarchyChanged();
            _bookHeaderPanel.OnProjectOrHierarchyChanged();
            _pageListPanel.OnProjectOrHierarchyChanged();
            _pageHeaderPanel.OnProjectOrHierarchyChanged();
            _commandListPanel.OnProjectOrHierarchyChanged();
            _commandPanel.OnProjectOrHierarchyChanged();
            _variablePanel.OnProjectOrHierarchyChanged();
        }

        void OnUndoRedoPerformed()
        {
            _headerPanel.OnUndoRedoPerformed();
            _bookHeaderPanel.OnUndoRedoPerformed();
            _pageListPanel.OnUndoRedoPerformed();
            _pageHeaderPanel.OnUndoRedoPerformed();
            _commandListPanel.OnUndoRedoPerformed();
            _commandPanel.OnUndoRedoPerformed();
            _variablePanel.OnUndoRedoPerformed();
        }
    }
}