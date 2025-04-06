using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public abstract class AsyncCommand : Command
    {
        /// <summary>
        /// Implement ExecuteAsync instead.
        /// </summary>
        public sealed override void InvokeExecute(PageProcess process)
        {
            throw new InvalidOperationException($"Use {nameof(InvokeExecuteAsync)} instead of Execute.");
        }

        public virtual UniTask InvokeExecuteAsync(PageProcess process, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(InvokeExecuteAsync));
        }
    }
}