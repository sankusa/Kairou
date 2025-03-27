using System;

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
        [NonSerialized] Page _parentPage;
        public Page ParentPage => _parentPage;

        public int Index => _parentPage.Commands.IndexOf(this);

        void ICommandInternalForPage.SetParentPage(Page parentPage) => _parentPage = parentPage;

        public abstract void Execute(PageProcess pageProcess);
        public virtual string GetSummary() => null;
    }
}