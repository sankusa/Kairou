using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    /// <summary>Restricted to use by ScriptBook only</summary>
    internal interface IPageInternalForScriptBook
    {
        /// <summary>Must always be synchronized with the state of ScriptBook.Pages</summary>
        void SetParentBook(ScriptBook scriptBook);
    }

    // When this becomes a child element of ScriptBook, the parent must be set to _scriptBook at the same time to maintain consistency.
    [Serializable]
    public class Page : IPageInternalForScriptBook, ISerializationCallbackReceiver
    {
        [SerializeField] string _pageId;
        public Guid PageId { get; private set; } = Guid.NewGuid();

        [SerializeField] string _name;
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        [NonSerialized] ScriptBook _parentBook;
        public ScriptBook ParentBook => _parentBook;

        [SerializeReference] List<Command> _commands = new();
        public IReadOnlyList<Command> Commands => _commands;

        public int Index => _parentBook.Pages.IndexOf(this);

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _pageId = PageId.ToString("N");
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(_pageId) == false)
            {
                if (Guid.TryParse(_pageId, out Guid guid))
                {
                    PageId = guid;
                }
            }

            foreach (Command command in _commands)
            {
                if (command == null) continue;
                (command as ICommandInternalForPage).SetParentPage(this);
            }
        }

        void IPageInternalForScriptBook.SetParentBook(ScriptBook parentBook)
        {
            _parentBook = parentBook;
        }

        internal void AddCommand(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (_commands.Contains(command)) throw new ArgumentException(nameof(command) + " is already added.");
            _commands.Add(command);
            (command as ICommandInternalForPage).SetParentPage(this);
        }

        internal void RemoveCommand(Command command)
        {
            if (command == null) return;
            if (_commands.Contains(command) == false) return;
            _commands.Remove(command);
            (command as ICommandInternalForPage).SetParentPage(null);
        }

        internal void RemoveCommandAt(int commandIndex)
        {
            if (_commands.HasElementAt(commandIndex) == false) return;
            var command = _commands[commandIndex];
            _commands.RemoveAt(commandIndex);
            (command as ICommandInternalForPage).SetParentPage(null);
        }

        internal void MoveCommand(int fromIndex, int toIndex)
        {
            _commands.Move(fromIndex, toIndex);
        }
    }
}