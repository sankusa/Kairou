using System;
using System.Collections.Generic;
using UnityEngine;

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

        public virtual void InvokeExecute(PageProcess pageProcess) {}
        public virtual string GetSummary() => null;
        public IEnumerable<string> InvokeValidate()
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
    }
}