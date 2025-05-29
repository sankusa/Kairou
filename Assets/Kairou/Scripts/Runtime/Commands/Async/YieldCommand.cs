using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Async", "Yield")]
    public partial class YieldCommand : AsyncCommand
    {
        [CommandExecute]
        UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            return UniTask.Yield(cancellationToken);
        }
    }
}