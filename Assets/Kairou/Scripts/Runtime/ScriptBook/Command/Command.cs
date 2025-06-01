using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kairou
{
    /// <summary>Restricted to use by Page only</summary>
    internal interface ICommandInternalForPage
    {
        /// <summary>Must always be synchronized with the state of Page.Commands</summary>
        void SetParentPage(Page page);
    }

    /// <summary>Base class for commands</summary>
    [Serializable]
    public abstract class Command : ICommandInternalForPage
    {
        public static Command CreateInstance(Type commandType)
        {
            return JsonUtility.FromJson(JsonUtility.ToJson((Command)Activator.CreateInstance(commandType)), commandType) as Command;
        }

        [NonSerialized] Page _parentPage;
        public Page ParentPage => _parentPage;

        public int Index => _parentPage.Commands.IndexOf(this);

        void ICommandInternalForPage.SetParentPage(Page parentPage) => _parentPage = parentPage;

        public virtual void InvokeExecute(IProcessInterface process) {}
        public virtual string GetSummary() => null;
        public virtual IEnumerable<string> InvokeValidate()
        {
            foreach (string errorMessage in Validate_Generated())
            {
                yield return errorMessage;
            }
            foreach (string errorMessage in Validate())
            {
                yield return errorMessage;
            }
        }
        protected virtual IEnumerable<string> Validate_Generated()
        {
            yield break;
        }
        protected virtual IEnumerable<string> Validate()
        {
            yield break;
        }

        public int CalculateBlockLevel() => _parentPage.CalculateBlockLevel(Index);

        public virtual IEnumerable<ScriptBook> GetReferencingBooks()
        {
            yield break;
        }

        public virtual void GetPreloadTargetBooks(ICollection<ScriptBook> books) {}

#if UNITY_EDITOR
        public Command Copy()
        {
            var copied = (Command)Activator.CreateInstance(GetType());
            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(this), copied);
            return copied;
        }
#endif
    }
}