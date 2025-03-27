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
        public sealed override void Execute(PageProcess process)
        {
            throw new InvalidOperationException($"Use {nameof(ExecuteAsync)} instead of Execute.");
        }

        public abstract UniTask ExecuteAsync(PageProcess process, CancellationToken cancellationToken);
    }
}