using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Profiling;
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
        [SerializeField] int _pageIndex = 0;
        [SerializeField] int _commandIndex = 0;

        [SerializeField] ScriptBookEditorHeaderPanel _headerPanel = new();
        [SerializeField] BookHeaderPanel _bookHeaderPanel = new();
        [SerializeField] PageListPanel _pageListPanel = new();
        [SerializeField] PageHeaderPanel _pageHeaderPanel = new();
        [SerializeField] CommandListPanel _commandListPanel = new();
        [SerializeField] CommandPanel _commandPanel = new();
        [SerializeField] VariablePanel _variablePanel = new();
        [SerializeField] CommandPickerPanel _commandPickerPanel = new();

        SerializedObject _serializedObject;

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
                    ChangePageIndex(bookId, pageIndex);
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
                (bookId, pageIndex, commandIndex) => ChangeCommandIndex(bookId, pageIndex, commandIndex),
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

            Rebind();
        }

        void Rebind()
        {
            if (_bookHolder.HasValidBook)
            {
                if (_serializedObject == null || _serializedObject.targetObject != _bookHolder.Owner)
                {
                    _serializedObject = new SerializedObject(_bookHolder.Owner);
                }
                var bookProp = _serializedObject.FindProperty(_bookHolder.BookPropertyPath);
                var pagesProp = bookProp.FindPropertyRelative("_pages");

                _bookHeaderPanel.SetTarget(bookProp.propertyPath);
                if (0 <= _pageIndex && _pageIndex < pagesProp.arraySize)
                {
                    var pageProp = pagesProp.GetArrayElementAtIndex(_pageIndex);
                    var commandsProp = pageProp.FindPropertyRelative("_commands");
                    if (0 <= _commandIndex && _commandIndex < commandsProp.arraySize)
                    {
                        var commandProp = commandsProp.GetArrayElementAtIndex(_commandIndex);
                        _commandPanel.SetTarget(commandProp);
                    }
                    else
                    {
                        // bindingPath削除
                        _commandPanel.SetTarget(null);
                    }
                    _pageHeaderPanel.SetTarget(pageProp.propertyPath);
                }
                else
                {
                    // bindingPath削除
                    // _pageHeaderPanel.SetTarget(pagesProp.propertyPath);
                }
                Profiler.BeginSample("ScriptBookEditor.Bind");
                rootVisualElement.Bind(_serializedObject);
                Profiler.EndSample();
            }
        }

        void SetTarget(Object bookOwner, string bookPropertyPath)
        {
            _bookHolder.Reset(bookOwner, bookPropertyPath);
            Rebind();

            _headerPanel.SetTarget(_bookHolder.BookId);
            // _bookHeaderPanel.SetTarget(_bookHolder.BookId);
            _pageListPanel.SetTarget(_bookHolder.BookId);
            // _pageHeaderPanel.SetTarget(_bookHolder.BookId, 0);
            _commandListPanel.SetTarget(_bookHolder.BookId, 0);
            // _commandPanel.SetTarget(_bookHolder.BookId, 0, 0);
            _variablePanel.SetTarget(_bookHolder.BookId, 0);
        }

        void ChangePageIndex(BookId bookId, int pageIndex)
        {
            _pageIndex = pageIndex;
            _commandIndex = 0;
            Rebind();
            _commandListPanel.SetTarget(bookId, pageIndex);
            // _commandPanel.SetTarget(bookId, pageIndex, 0);
            _variablePanel.SetTarget(bookId, pageIndex);
        }

        void ChangeCommandIndex(BookId bookId, int pageIndex, int commandIndex)
        {
            _commandIndex = commandIndex;
            Rebind();
            // _commandPanel.SetTarget(bookId, pageIndex, commandIndex);
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
                Rebind();
            }
            _headerPanel.OnProjectOrHierarchyChanged();
            // _bookHeaderPanel.OnProjectOrHierarchyChanged();
            _pageListPanel.OnProjectOrHierarchyChanged();
            // _pageHeaderPanel.OnProjectOrHierarchyChanged();
            _commandListPanel.OnProjectOrHierarchyChanged();
            // _commandPanel.OnProjectOrHierarchyChanged();
            _variablePanel.OnProjectOrHierarchyChanged();
        }

        void OnUndoRedoPerformed()
        {
            // ObjectがDestroyされた場合など
            if (_bookHolder.RestoreObjectIfNull())
            {
                Rebind();
            }
            _headerPanel.OnUndoRedoPerformed();
            // _bookHeaderPanel.OnUndoRedoPerformed();
            _pageListPanel.OnUndoRedoPerformed();
            // _pageHeaderPanel.OnUndoRedoPerformed();
            _commandListPanel.OnUndoRedoPerformed();
            // _commandPanel.OnUndoRedoPerformed();
            _variablePanel.OnUndoRedoPerformed();
        }
    }
}