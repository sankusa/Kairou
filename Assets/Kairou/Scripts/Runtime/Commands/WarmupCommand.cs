using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public partial class WarmupCommand : AsyncCommand, IBlockStart, IBlockEnd
    {
        public string BlockCategory => "If";

        public bool IsLoopBlock => false;

        [CommandExecute]
        async UniTask ExecuteAsync(IProcessInterface process, CancellationToken cancellationToken)
        {
            cancellationToken.RegisterWithoutCaptureExecutionContext(static _ => {}, this).Dispose();
            process.TryPopBlock<IfBlock>(out var block);
            var ifBlock = IfBlock.Rent();
            ifBlock.SetStartAndEnd(this);
            ParentPage.FindBlockStartIndex(this);
            int s = ifBlock.StartIndex;
            int e = ifBlock.EndIndex;
            await UniTask.Yield(cancellationToken);
        }
    }
}