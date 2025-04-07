using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Cysharp.Threading.Tasks
{
    public struct UniTask { }
}
namespace Kairou
{
    public class PageProcess { }

    public abstract class Command
    {
        public virtual void InvokeExecute(PageProcess pageProcess) { }
    }
    public abstract class AsyncCommand : Command
    {
        public virtual UniTask InvokeExecuteAsync(PageProcess pageProcess, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(InvokeExecuteAsync));
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandExecuteAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class InjectAttribute : Attribute { }
}