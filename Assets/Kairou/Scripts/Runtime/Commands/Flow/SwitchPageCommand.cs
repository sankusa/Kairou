using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public partial class SwitchPageCommand : Command
    {
        [GenerateValidation]
        [SerializeField] SiblingPageSelector _target;

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            process.SwitchPage(_target.PageId);
        }

        public override string GetSummary()
        {
            return _target.GetSummary();
        }
    }
}