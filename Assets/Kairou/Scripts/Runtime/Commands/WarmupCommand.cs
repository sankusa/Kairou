using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public partial class WarmupCommand : AsyncCommand
    {
        [CommandExecute]
        async UniTask ExecuteAsync(IProcessInterface process, CancellationToken cancellationToken)
        {
            process.TryPopBlock<Block>(out var block);
            await UniTask.Yield(cancellationToken);
        }
    }
}