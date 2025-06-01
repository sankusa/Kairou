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
        
        bool IsInitialized => _objectField != null;

        public void Initialize(VisualElement parent, VisualTreeAsset headerPanelUXML)
        {
            var headerPanel = headerPanelUXML.Instantiate();
            parent.Add(headerPanel);

            _objectField = headerPanel.Q<ObjectField>();
            _bookPathLabel = headerPanel.Q<Label>("BookPath");
            _variableDisplayToggle = headerPanel.Q<ToolbarToggle>();

            Reload();
        }

        public void SetTarget(BookId bookId)
        {
            _bookHolder.Reset(bookId);
            if (IsInitialized) Reload();
        }

        public void Reload()
        {
            ThrowIfNotInitialized();

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
            ThrowIfNotInitialized();
        }

        void ThrowIfNotInitialized()
        {
            if (IsInitialized == false) throw new InvalidOperationException($"{nameof(BookHeaderPanel)} is not initialized.");
        }
    }
}