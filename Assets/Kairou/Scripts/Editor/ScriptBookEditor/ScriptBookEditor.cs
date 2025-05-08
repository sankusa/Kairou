using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    public class ScriptBookEditor : EditorWindow
    {

        public static void Open(IScriptBookOwner scriptBookOwner)
        {
            var window = GetWindow<ScriptBookEditor>();
            window.titleContent = new GUIContent("ScriptBookEditor");

            window.SetTarget(scriptBookOwner);
        }

        [SerializeField] VisualTreeAsset _headerUXML;
        [SerializeField] VisualTreeAsset _pageListPanelUXML;
        [SerializeField] VisualTreeAsset _commandListPanelUXML;
        [SerializeField] VisualTreeAsset _variablePanelUXML;

        [SerializeField] Object _scriptBookOwnerObject;
        IScriptBookOwner Scriptbookowner => _scriptBookOwnerObject as IScriptBookOwner;
        [SerializeField] GlobalObjectId _globalObjectId;

        VisualElement _header;
        [SerializeField] PageListPanel _pageListPanel = new();
        [SerializeField] CommandListPanel _commandListPanel = new();
        [SerializeField] CommandPanel _commandPanel = new();
        [SerializeField] VariablePanel _variablePanel = new();

        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            EditorApplication.projectChanged += OnProjectOrHierarchyChanged;
            EditorApplication.hierarchyChanged += OnProjectOrHierarchyChanged;
            // SetTarget(GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_globalObjectId)as IScriptBookOwner);
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
            ToolbarToggle variableToggle = new() {text = "Variables", viewDataKey = "VariableToggle"}; 
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

            _header = _headerUXML.Instantiate();
            bookPane.Add(_header);
            _header.Q<ObjectField>().value = _scriptBookOwnerObject;

            var panes = UIToolkitUtil.CreateSplitView(bookPane, viewDataKey: "Split1");
            var leftPane = panes.leftPane;
            var (centerPane, rightPane) = UIToolkitUtil.CreateSplitView(panes.rightPane, 1, 200f, viewDataKey: "Split2");

            _pageListPanel.Initialize(
                leftPane,
                _pageListPanelUXML,
                pageIndex =>
                {
                    _commandListPanel.SetTarget(Scriptbookowner, pageIndex);
                    _commandPanel.SetTarget(Scriptbookowner, pageIndex, 0);
                    _variablePanel.SetTarget(Scriptbookowner, pageIndex);
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
                (owner, pageIndex, commandIndex) => _commandPanel.SetTarget(owner, pageIndex, commandIndex),
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

        void SetTarget(IScriptBookOwner scriptBookOwner)
        {
            if (scriptBookOwner == null)
            {
                ClearTarget();
                return;
            }
            _scriptBookOwnerObject = scriptBookOwner.AsObject();
            _globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(_scriptBookOwnerObject);
            _header.Q<ObjectField>().value = _scriptBookOwnerObject;
            _pageListPanel.SetTarget(scriptBookOwner);
            _commandListPanel.SetTarget(scriptBookOwner, 0);
            _commandPanel.SetTarget(scriptBookOwner, 0, 0);
            _variablePanel.SetTarget(scriptBookOwner, 0);
        }

        void ClearTarget()
        {
            _scriptBookOwnerObject = null;
            if (_header != null) _header.Q<ObjectField>().value = null;
            _pageListPanel.SetTarget(null);
            _commandListPanel.SetTarget(null, 0);
            _commandPanel.SetTarget(null, 0, 0);
            _variablePanel.SetTarget(null, 0);
        }

        void OnProjectOrHierarchyChanged()
        {
            // ObjectがDestroyされた場合など
            if (_scriptBookOwnerObject == null)
            {
                // 復元を試みる
                _scriptBookOwnerObject = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_globalObjectId);
                SetTarget(Scriptbookowner);
            }
        }

        void OnUndoRedoPerformed()
        {
            _pageListPanel.OnUndoRedoPerformed();
            _commandListPanel.OnUndoRedoPerformed();
            _variablePanel.OnUndoRedoPerformed();
        }
    }
}