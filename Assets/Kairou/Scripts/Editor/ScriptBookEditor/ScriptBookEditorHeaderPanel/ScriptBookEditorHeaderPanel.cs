using System;
using System.Linq;
using Kairou.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou
{
    [Serializable]
    public class ScriptBookEditorHeaderPanel
    {
        [SerializeField] RestorableBookHolder _bookHolder = new();

        ObjectField _objectField;
        Label _bookPathLabel;
        ToolbarToggle _variableDisplayToggle;
        public ToolbarToggle VariableDisplayToggle => _variableDisplayToggle;
        ToolbarButton _reloadButton;
        public ToolbarButton ReloadButton => _reloadButton;
        
        bool IsInitialized => _objectField != null;

        public void Initialize(VisualElement parent, VisualTreeAsset headerPanelUXML)
        {
            var headerPanel = headerPanelUXML.Instantiate();
            parent.Add(headerPanel);

            _objectField = headerPanel.Q<ObjectField>();
            _bookPathLabel = headerPanel.Q<Label>("BookPath");
            _variableDisplayToggle = headerPanel.Q<ToolbarToggle>();
            _reloadButton = headerPanel.Q<ToolbarButton>();

            Reload();
        }

        public void SetTarget(BookId bookId)
        {
            _bookHolder.Reset(bookId);
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            if (IsInitialized == false) return;

            _objectField.value = _bookHolder.Owner;
            _bookPathLabel.text = $".{_bookHolder.BookPropertyPath}";
        }

        public void OnProjectOrHierarchyChanged()
        {
            if (_bookHolder.RestoreObjectIfNull())
            {
                Reload();
            }
        }

        public void OnUndoRedoPerformed()
        {
            
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(BookHeaderPanel)} is not initialized.");
        }
    }
}