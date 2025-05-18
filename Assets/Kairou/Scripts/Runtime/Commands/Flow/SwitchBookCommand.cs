using UnityEngine;

namespace Kairou
{
    [CommandInfo("Flow", "SwitchBook")]
    public partial class SwitchBookCommand : Command
    {
        [SerializeField] bool _switchSeries;

        [GenerateValidation]
        [SerializeField] BookAndPageSelector _target;

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            if (_switchSeries)
            {
                process.SwitchBookAsNewSeries(_target.Book, _target.PageId);
            }
            else
            {
                process.SwitchBook(_target.Book, _target.PageId);
            }
        }

        public override string GetSummary()
        {
            return (_switchSeries ? "(Switch Series) " : "") + _target.GetSummary();
        }
    }
}