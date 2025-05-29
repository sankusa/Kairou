using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Async", "Delay")]
    public partial class DelayCommand : AsyncCommand
    {
        [SerializeField] DelayType _delaytype = DelayType.DeltaTime;

        [GenerateValidation]
        [SerializeField] FlexibleParameter<float> _seconds;

        [CommandExecute]
        UniTask ExecuteAsync(IProcessInterface process, CancellationToken cancellationToken)
        {
            return UniTask.Delay((int)(_seconds.ResolveValue(process) * 1000), _delaytype, cancellationToken: cancellationToken);
        }

        public override string GetSummary() => $"{_seconds.GetSummary()} seconds{(_delaytype == DelayType.DeltaTime ? "" : $"  ({_delaytype.ToString()})")}";
    }
}