using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    public class ScriptBookEditor : EditorWindow
    {

        public static void Open(Object scriptBookOwner, string scriptBookPath)
        {
            var window = GetWindow<ScriptBookEditor>();
            window.titleContent = new GUIContent("ScriptBookEditor");

            window.SetTarget(scriptBookOwner, scriptBookPath);
        }

        [SerializeField] VisualTreeAsset _bookHeaderUXML;
        [SerializeField] VisualTreeAsset _pageListPanelUXML;
        [SerializeField] VisualTreeAsset _commandListPanelUXML;
        [SerializeField] VisualTreeAsset _variablePanelUXML;

        [SerializeField] RestorableScriptBookHolder _bookHolder = new();

        [SerializeField] BookHeaderPanel _bookHeaderPanel = new();
        [SerializeField] PageListPanel _pageListPanel = new();
        [SerializeField] CommandListPanel _commandListPanel = new();
        [SerializeField] CommandPanel _commandPanel = new();
        [SerializeField] VariablePanel _variablePanel = new();

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

        public void CreateGUI()
        {
            var root = rootVisualElement;

            var toolbar = new Toolbar();
            rootVisualElement.Add(toolbar);

            (var bookPane, var variablePane) = UIToolkitUtil.CreateSplitView(
                out TwoPaneSplitView rootSplitView,
                rootVisualElement,
                viewDataKey: "Split0");

            toolbar.Add(new ToolbarSpacer() {flex = true});
            var variableToggle = new ToolbarToggle() {text = "Variables", viewDataKey = "VariableToggle"}; 
            toolbar.Add(variableToggle);

            rootSplitView.schedule.Execute(() =>
            {
                if (variableToggle.value) return;
                rootSplitView.CollapseChild(1);
            });

            variableToggle.RegisterValueChangedCallback(evt =>
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

            _bookHeaderPanel.Initialize(bookPane, _bookHeaderUXML);

            var panes = UIToolkitUtil.CreateSplitView(bookPane, viewDataKey: "Split1");
            var leftPane = panes.leftPane;
            var (centerPane, rightPane) = UIToolkitUtil.CreateSplitView(panes.rightPane, 1, 200f, viewDataKey: "Split2");

            _pageListPanel.Initialize(
                leftPane,
                _pageListPanelUXML,
                (scriptBookId, pageIndex) =>
                {
                    _commandListPanel.SetTarget(scriptBookId, pageIndex);
                    _commandPanel.SetTarget(scriptBookId, pageIndex, 0);
                    _variablePanel.SetTarget(scriptBookId, pageIndex);
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
                (scriptBookId, pageIndex, commandIndex) => _commandPanel.SetTarget(scriptBookId, pageIndex, commandIndex),
                () => _commandPanel.Reload()
            );

            _commandPanel.Initialize(
                rightPane,
                () => _commandListPanel.Rebuild()
            );

            _variablePanel.Initialize(
                variablePane,
                _variablePanelUXML
            );
        }

        void Reload()
        {
            _bookHeaderPanel.SetTarget(_bookHolder.ScriptBookId);
            _pageListPanel.SetTarget(_bookHolder.ScriptBookId);
            _commandListPanel.SetTarget(_bookHolder.ScriptBookId, 0);
            _commandPanel.SetTarget(_bookHolder.ScriptBookId, 0, 0);
            _variablePanel.SetTarget(_bookHolder.ScriptBookId, 0);
        }

        void SetTarget(Object scriptBookOwner, string scriptBookPath)
        {
            _bookHolder.Reset(scriptBookOwner, scriptBookPath);
            Reload();
        }

        // PlayMode遷移にも呼ばれる
        void OnProjectOrHierarchyChanged()
        {
            // ObjectがDestroyされた場合など
            if (_bookHolder.RestoreObjectIfNull())
            {
                Reload();
            }
            _bookHeaderPanel.OnProjectOrHierarchyChanged();
            _pageListPanel.OnProjectOrHierarchyChanged();
            _commandListPanel.OnProjectOrHierarchyChanged();
            _commandPanel.OnProjectOrHierarchyChanged();
            _variablePanel.OnProjectOrHierarchyChanged();
        }

        void OnUndoRedoPerformed()
        {
            _bookHeaderPanel.OnUndoRedoPerformed();
            _pageListPanel.OnUndoRedoPerformed();
            _commandListPanel.OnUndoRedoPerformed();
            _commandListPanel.OnUndoRedoPerformed();
            _variablePanel.OnUndoRedoPerformed();
        }
    }
}