using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    public class ScriptBookEditor : EditorWindow
    {
        [SerializeField] VisualTreeAsset _headerUXML;
        [SerializeField] VisualTreeAsset _pageListPanelUXML;
        [SerializeField] VisualTreeAsset _commandListPanelUXML;

        [SerializeField] Object _scriptBookOwnerObject;

        [SerializeField] PageListPanel _pageListPanel = new();
        [SerializeField] CommandListPanel _commandListPanel = new();
        [SerializeField] CommandPanel _commandPanel = new();

        public static void Open(Object scriptBookOwnerObject)
        {
            // Using GetWindow() immediately calls CreateGUI(), preventing pre-setting of values.
            var window = CreateInstance<ScriptBookEditor>();
            window._scriptBookOwnerObject = scriptBookOwnerObject;
            window.titleContent = new GUIContent("ScriptBookEditor");

            window.Show();
        }

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

            var header = _headerUXML.Instantiate();
            rootVisualElement.Add(header);
            header.Q<ObjectField>().value = _scriptBookOwnerObject;

            var panes = UIToolkitUtil.CreateSplitView(rootVisualElement);
            var leftPane = panes.leftPane;
            var (centerPane, rightPane) = UIToolkitUtil.CreateSplitView(panes.rightPane, 1, 200f);

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
                (owner, pageIndex, commandIndex) => _commandPanel.SetTarget(owner, pageIndex, commandIndex)
            );

            _commandPanel.Initialize(
                rightPane,
                () => _commandListPanel.Rebuild()
            );

            _pageListPanel.SetScriptBookOwner(_scriptBookOwnerObject as IScriptBookOwner);
        }

        void OnProjectOrHierarchyChanged()
        {
            if (_scriptBookOwnerObject == null)
            {
                _pageListPanel.SetScriptBookOwner(null);
                _commandListPanel.SetTarget(null, 0);
                _commandPanel.SetTarget(null, 0, 0);
            }
        }

        void OnUndoRedoPerformed()
        {
            _pageListPanel.OnUndoRedoPerformed();
            _commandListPanel.OnUndoRedoPerformed();
        }
    }
}