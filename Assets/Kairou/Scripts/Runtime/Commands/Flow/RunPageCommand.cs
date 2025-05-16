using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Flow", "RunPage")]
    public partial class RunPageCommand : AsyncCommand
    {
        [GenerateValidation]
        [SerializeField] SiblingPageSelector _target;

        [CommandExecute]
        async UniTask ExecuteAsync(IProcessInterface process, CancellationToken cancellationToken)
        {
            await process.RunPageAsync(_target.PageId, cancellationToken);
        }
    }
}