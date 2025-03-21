using System;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public abstract class AsyncCommand : Command
    {
        public sealed override void Execute(IProcess process)
        {
            throw new InvalidOperationException($"Use {nameof(ExecuteAsync)} instead of Execute.");
        }

        public abstract UniTask ExecuteAsync(IProcess process);
    }
}