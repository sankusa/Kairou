using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Flow", "SwitchPage")]
    public partial class SwitchPageCommand : Command
    {
        [GenerateValidation]
        [SerializeField] SiblingPageSelector _target;

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            process.SwitchPage(_target.PageId);
        }
    }
}