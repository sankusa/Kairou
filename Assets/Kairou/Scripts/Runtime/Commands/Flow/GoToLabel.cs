using UnityEngine;

namespace Kairou
{
    public partial class GoToLabel : Command
    {
        [SerializeField] string _label;

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            for (int i = 0; i < ParentPage.Commands.Count; i++)
            {
                if (ParentPage.Commands[i] is ILabel labelCommand && labelCommand.Label == _label)
                {
                    process.GoToIndex(i);
                    return;
                }
            }
        }

        public override string GetSummary() => _label;
    }
}