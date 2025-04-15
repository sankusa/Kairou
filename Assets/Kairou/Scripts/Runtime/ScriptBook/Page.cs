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
        [SerializeField] string _id;
        public string Id
        {
            get => _id;
            set => _id = value;
        }

        [NonSerialized] ScriptBook _parentBook;
        public ScriptBook ParentBook => _parentBook;
        public int Index => _parentBook.Pages.IndexOf(this);

        [SerializeReference] List<Command> _commands = new();
        public List<Command> Commands => _commands;

        [SerializeReference] List<VariableDefinition> _variables = new();
        public List<VariableDefinition> Variables => _variables;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            foreach (Command command in _commands)
            {
                if (command == null) continue;
                ((ICommandInternalForPage)command).SetParentPage(this);
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
            ((ICommandInternalForPage)command).SetParentPage(this);
        }

        internal void RemoveCommand(Command command)
        {
            if (command == null) return;
            if (_commands.Contains(command) == false) return;
            _commands.Remove(command);
            ((ICommandInternalForPage)command).SetParentPage(null);
        }

        internal void RemoveCommandAt(int commandIndex)
        {
            if (_commands.HasElementAt(commandIndex) == false) return;
            var command = _commands[commandIndex];
            _commands.RemoveAt(commandIndex);
            ((ICommandInternalForPage)command).SetParentPage(null);
        }

        internal void MoveCommand(int fromIndex, int toIndex)
        {
            _commands.Move(fromIndex, toIndex);
        }
    }
}