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

        [SerializeField] Object _scriptBookOwnerObject;

        VisualElement _header;
        [SerializeField] PageListPanel _pageListPanel = new();
        [SerializeField] CommandListPanel _commandListPanel = new();
        [SerializeField] CommandPanel _commandPanel = new();

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

            _header = _headerUXML.Instantiate();
            rootVisualElement.Add(_header);
            _header.Q<ObjectField>().value = _scriptBookOwnerObject;

            var panes = UIToolkitUtil.CreateSplitView(rootVisualElement, viewDataKey: "Split1");
            var leftPane = panes.leftPane;
            var (centerPane, rightPane) = UIToolkitUtil.CreateSplitView(panes.rightPane, 1, 200f, viewDataKey: "Split2");

            _pageListPanel.Initialize(
                leftPane,
                _pageListPanelUXML,
                pageIndex =>
                {
                    _commandPanel.SetTarget(null, 0, 0);
                    _commandListPanel.SetTarget(_scriptBookOwnerObject as IScriptBookOwner, pageIndex);
                },
                () => _commandListPanel.Reload()
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
        }

        void SetTarget(IScriptBookOwner scriptBookOwner)
        {
            if (scriptBookOwner == null)
            {
                ClearTarget();
                return;
            }
            _scriptBookOwnerObject = scriptBookOwner.AsObject();
            _header.Q<ObjectField>().value = _scriptBookOwnerObject;
            _pageListPanel.SetTarget(scriptBookOwner);
            _commandListPanel.SetTarget(scriptBookOwner, 0);
            _commandPanel.SetTarget(scriptBookOwner, 0, 0);
        }

        void ClearTarget()
        {
            _scriptBookOwnerObject = null;
            _header.Q<ObjectField>().value = null;
            _pageListPanel.SetTarget(null);
            _commandListPanel.SetTarget(null, 0);
            _commandPanel.SetTarget(null, 0, 0);
        }

        void OnProjectOrHierarchyChanged()
        {
            // ObjectがDestroyされた場合など
            if (_scriptBookOwnerObject == null)
            {
                ClearTarget();
            }
        }

        void OnUndoRedoPerformed()
        {
            _pageListPanel.OnUndoRedoPerformed();
            _commandListPanel.OnUndoRedoPerformed();
        }
    }
}