using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    /// <summary>Restricted to use by book only</summary>
    internal interface IPageInternalForBook
    {
        /// <summary>Must always be synchronized with the state of book.Pages</summary>
        void SetParentBook(ScriptBook book);
    }

    // When this becomes a child element of book, the parent must be set to _parentBook at the same time to maintain consistency.
    [Serializable]
    public class Page : IPageInternalForBook, ISerializationCallbackReceiver
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
        public IReadOnlyList<Command> Commands => _commands;

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

        void IPageInternalForBook.SetParentBook(ScriptBook parentBook)
        {
            _parentBook = parentBook;
        }

        public int IndexOf(Command command)
        {
            for (int i = 0; i < _commands.Count; i++) {
                if (_commands[i] == command) {
                    return i;
                }
            }
            return -1;
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

        public IEnumerable<Page> GetSiblingPages()
        {
            foreach (Page page in _parentBook.Pages)
            {
                if (page == this) continue;
                yield return page;
            }
        }

        public int FindBlockEndIndex(IBlockStart blockStart)
        {
            int index = _commands.IndexOf(blockStart as Command);
            if(index == -1) throw new ArgumentException($"BlockStart not found in page.");

            FindBlockEndIndexInternal(ref index, blockStart.BlockCategory);
            if (index == _commands.Count) return -1;
            return index;

            void FindBlockEndIndexInternal(ref int index, string blockCategory)
            {
                // Startの次のインデックス以降を探す
                index++;
                for (; index < _commands.Count; index++)
                {
                    if (_commands[index] is IBlockEnd blockEnd)
                    {
                        if (blockEnd.BlockCategory == blockCategory) return;
                    }

                    // BlockEndがBlockStartだった場合、再びBlockEndを探す
                    while (index < _commands.Count && _commands[index] is IBlockStart start)
                    {
                        FindBlockEndIndexInternal(ref index, start.BlockCategory);
                    }
                }
            }
        }

        public int FindBlockStartIndex(IBlockEnd blockEnd)
        {
            int index = _commands.IndexOf(blockEnd as Command);
            if(index == -1) throw new ArgumentException($"BlockEnd not found in page.");

            if (FindBlockStartIndexInternal(ref index, blockEnd.BlockCategory))
            {
                return index;
            }
            else
            {
                return -1;
            }

            bool FindBlockStartIndexInternal(ref int index, string blockCategory)
            {
                // Endの前のインデックス以前を探す
                index--;
                for (; index >= 0; index--)
                {
                    if (_commands[index] is IBlockStart blockStart)
                    {
                        if (blockStart.BlockCategory == blockCategory)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    // BlockStartがBlockEndだった場合、再びBlockEndを探す
                    while (index >= 0 && _commands[index] is IBlockEnd end)
                    {
                        bool matched = FindBlockStartIndexInternal(ref index, end.BlockCategory);
                        // マッチしなかったBlockStartが元々探してたBlockStartかを調べる
                        if (matched == false)
                        {
                            index++;
                            break;
                        }
                    }
                }
                return false;
            }
        }

        public int CalculateBlockLevel(int targetIndex)
        {
            int level = 0;
            int index = 0;
            targetIndex = Math.Min(targetIndex, _commands.Count);

            CalculateBlockLevelInternal(ref index, ref level, targetIndex, null);
            return level;

            void CalculateBlockLevelInternal(ref int index, ref int level, int targetIndex, string blockCategory)
            {
                for (; index <= targetIndex; index++)
                {
                    if (_commands[index] is IBlockEnd blockEnd)
                    {
                        if (blockEnd.BlockCategory == blockCategory)
                        {
                            level--;
                            return;
                        }
                    }

                    while (index < targetIndex && _commands[index] is IBlockStart start)
                    {
                        level++;
                        index++;
                        CalculateBlockLevelInternal(ref index, ref level, targetIndex, start.BlockCategory);
                    }
                }
            }
        }
    }
}