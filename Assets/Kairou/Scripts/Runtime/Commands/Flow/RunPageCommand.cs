using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public partial class RunPageCommand : AsyncCommand
    {
        [GenerateValidation]
        [SerializeField] SiblingPageSelector _target;

        [CommandExecute]
        UniTask ExecuteAsync(IProcessInterface process, CancellationToken cancellationToken)
        {
            return process.RunPageAsync(_target.PageId, cancellationToken);
        }

        public override string GetSummary()
        {
            return _target.GetSummary();
        }
    }
}