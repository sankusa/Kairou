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
        static Object _reservedBookOwner = null;
        static string _reservedBookPropertyPath = null;
        static int _reservedPageIndex = -1;
        static int _reservedCommandIndex = -1;

        public static void Open(Object bookOwner, string bookPropertyPath)
        {
            _reservedBookOwner = bookOwner;
            _reservedBookPropertyPath = bookPropertyPath;
            _reservedPageIndex = 0;
            _reservedCommandIndex = 0;
            var window = GetWindow<ScriptBookEditor>("ScriptBookEditor");
            // 既にウィンドウが開いている状態で呼ばれたら、CreateGUIは呼ばれないのでこちらで処理
            if (window.ResolveReservedParameters())
            {
                window.Rebind();
            }
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
        SerializedProperty _bookProp;
        SerializedProperty _pageProp;
        SerializedProperty _commandProp;

        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            EditorApplication.projectChanged += OnProjectChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        bool ResolveReservedParameters()
        {
            bool reservedAny = false;
            if (_reservedBookOwner != null || _reservedBookPropertyPath != null)
            {
                _bookHolder.Reset(_reservedBookOwner, _reservedBookPropertyPath);
                _reservedBookOwner = null;
                _reservedBookPropertyPath = null;
                reservedAny = true;
            }
            if (_reservedPageIndex != -1)
            {
                _pageIndex = _reservedPageIndex;
                _reservedPageIndex = -1;
                reservedAny = true;
            }
            if (_reservedCommandIndex != -1)
            {
                _commandIndex = _reservedCommandIndex;
                _reservedCommandIndex = -1;
                reservedAny = true;
            }
            return reservedAny;
        }

        void CreateGUI()
        {
            bool pageIndexReserved = _reservedPageIndex != -1;
            bool commandIndexReserved = _reservedCommandIndex != -1;
            ResolveReservedParameters();

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
                pageIndex =>
                {
                    OnPageIndexChanged(pageIndex);
                },
                () =>
                {
                    _serializedObject.UpdateIfRequiredOrScript();
                    _commandListPanel.Reload();
                },
                pageIndexReserved
            );

            _commandListPanel.Initialize(
                centerPane,
                _commandListPanelUXML,
                (commandIndex) =>
                {
                    OnCommandIndexChanged(commandIndex);
                },
                () =>
                {
                    _serializedObject.UpdateIfRequiredOrScript();
                },
                commandIndexReserved
            );

            var (commandPaneParent, commandPickerPane) = UIToolkitUtil.CreateSplitView(rightPane, 1, 200f, TwoPaneSplitViewOrientation.Vertical, viewDataKey: "Split3");

            var commandPane = _commandPaneUXML.Instantiate();
            commandPane.style.flexGrow = 1;
            commandPaneParent.Add(commandPane);
            var commandPaneInner = commandPane.Q<VisualElement>("Pane");

            _commandPanel.Initialize(
                commandPaneInner,
                () => {_commandListPanel.Refresh();}
            );

            _commandPickerPanel.Initialize(
                commandPickerPane,
                _commandPicker_CategoryListUXML,
                _commandPicker_CommandListUXML,
                (commandType) => _commandListPanel.InsertCommand(commandType)
            );

            _variablePanel.Initialize(
                variablePane,
                _variablePanelUXML,
                () =>
                {
                    _commandPanel.Refresh();
                    _commandListPanel.Refresh();
                }
            );

            Rebind();
        }

        void Rebind()
        {
            if (_bookHolder.HasValidBook)
            {
                if (_serializedObject == null || _serializedObject.targetObject != _bookHolder.Owner)
                {
                    if (SerializationUtility.HasManagedReferencesWithMissingTypes(_bookHolder.Owner))
                    {
                        EditorUtility.DisplayDialog("Warning", "Book has managed references with missing types.\nThis may destroy their data when saving.\nIf you edit it in its current state, missing-type data may be lost.\nPlease consider using [UnityEngine.Scripting.APIUpdating.MovedFrom] attributes or other methods to aid in migration.", "OK");
                    }
                    _serializedObject = new SerializedObject(_bookHolder.Owner);
                }
                _bookProp = _serializedObject.FindProperty(_bookHolder.BookPropertyPath);
            }
            else
            {
                _serializedObject = null;
                _bookProp = null;
            }
            _headerPanel.SetTarget(_bookHolder.BookId);
            _pageListPanel.SetTarget(_bookHolder.BookId);
            _bookHeaderPanel.Bind(_serializedObject, _bookProp?.propertyPath);
            _variablePanel.BindBookVariable(_serializedObject, _bookProp?.propertyPath);
            SetPageIndex(_pageIndex, _commandIndex);
        }

        void SetTarget(Object bookOwner, string bookPropertyPath)
        {
            _bookHolder.Reset(bookOwner, bookPropertyPath);
            _pageIndex = 0;
            _commandIndex = 0;
            Rebind();
        }

        void SetPageIndex(int pageIndex, int commandIndex = 0)
        {
            _pageIndex = pageIndex;
            if (_bookProp == null)
            {
                _pageProp = null;
            }
            else
            {
                var pagesProp = _bookProp.FindPropertyRelative("_pages");
                if (0 <= _pageIndex && _pageIndex < pagesProp.arraySize)
                {
                    _pageProp = pagesProp.GetArrayElementAtIndex(_pageIndex);
                }
                else
                {
                    _pageProp = null;
                }
            }
            _pageHeaderPanel.Bind(_serializedObject, _pageProp?.propertyPath);
            _commandListPanel.SetTarget(_bookHolder.BookId, pageIndex);
            _variablePanel.BindPageVariable(_pageProp?.propertyPath);
            SetCommandIndex(commandIndex);
        }

        void SetCommandIndex(int commandIndex)
        {
            _commandIndex = commandIndex;
            if (_pageProp == null)
            {
                _commandProp = null;
            }
            else
            {
                var commandsProp = _pageProp.FindPropertyRelative("_commands");
                if (0 <= _commandIndex && _commandIndex < commandsProp.arraySize)
                {
                    _commandProp = commandsProp.GetArrayElementAtIndex(_commandIndex);
                }
                else
                {
                    _commandProp = null;
                }
            }
            _commandPanel.Bind(_serializedObject, _commandProp?.propertyPath);
        }
        
        void OnPageIndexChanged(int pageIndex)
        {
            SetPageIndex(pageIndex);
            // VisualElementへBindをするとポーリング処理が発生する。
            // Bindを同じタイミングで行えば、ポーリング処理は一本化されるが、一部要素のみ再度Bindを行うとポーリングが新たに発生し
            // 元のポーリングと二重で走ることになる。
            // オブジェクトのサイズが大きくなると、ポーリングの度に発生するSerializedObject.UpdateIfRequiredOrScript()
            // がそれなりのコストになるため、ポーリングの一本化のため、一部の要素にBindする場合は全ての要素にBindし直す。
            // オブジェクトのサイズが大きくなることを想定するならBindはなるべく避けたい。
            // Bindもそれなりのコストがかかることに注意
            _variablePanel.RebindBookVariable();
        }

        void OnCommandIndexChanged(int commandIndex)
        {
            SetCommandIndex(commandIndex);
            // OnPageIndexChangedのコメントを参照
            _variablePanel.RebindBookVariable();
            _variablePanel.RebindPageVariable();
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
        void OnHierarchyChanged()
        {
            if (_bookHolder.RestoreObjectIfNull())
            {
                _serializedObject = new SerializedObject(_bookHolder.Owner);
                Rebind();
            }
            else if (_serializedObject != null && _serializedObject.targetObject == null)
            {
                _serializedObject = null;
                Rebind();
            }
            _headerPanel.OnProjectOrHierarchyChanged();
            _pageListPanel.OnProjectOrHierarchyChanged();
            _commandListPanel.OnProjectOrHierarchyChanged();
        }

        void OnProjectChanged()
        {
            if (_bookHolder.RestoreObjectIfNull())
            {
                _serializedObject = new SerializedObject(_bookHolder.Owner);
                Rebind();
            }
            else if (_serializedObject != null && _serializedObject.targetObject == null)
            {
                _serializedObject = null;
                Rebind();
            }
            _headerPanel.OnProjectOrHierarchyChanged();
            _pageListPanel.OnProjectOrHierarchyChanged();
            _commandListPanel.OnProjectOrHierarchyChanged();
        }

        void OnUndoRedoPerformed()
        {
            if (_bookHolder.RestoreObjectIfNull())
            {
                _serializedObject = new SerializedObject(_bookHolder.Owner);
                Rebind();
            }
            else if (_serializedObject != null)
            {
                if (_serializedObject.targetObject == null)
                {
                    _serializedObject = null;
                    Rebind();
                }
                else if (_serializedObject.UpdateIfRequiredOrScript())
                {
                    Rebind();
                }
            }

            _headerPanel.OnUndoRedoPerformed();
            _pageListPanel.OnUndoRedoPerformed();
            _commandListPanel.OnUndoRedoPerformed();
        }
    }
}