using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Async", "Yield")]
    public partial class YieldCommand : AsyncCommand
    {
        [CommandExecute]
        async UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            await UniTask.Yield(cancellationToken);
        }
    }
}