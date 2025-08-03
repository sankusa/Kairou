using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou.Tests.Runtime
{
    public partial class PeakBlockCommand : Command
    {
        [SerializeField] VariableKey<bool> _hasBlock = new();
        
        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            _hasBlock.Find(process).Value = process.PeekBlock() != null;
        }

        public override string GetSummary()
        {
            return _hasBlock.GetSummary();
        }
    }

    public partial class PauseResumeCommand : Command
    {
        [SerializeField] float _delaySeconds;
        
        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            process.Pause();
            UniTask.Delay((int)(_delaySeconds * 1000)).ContinueWith(() => process.Resume()).Forget();
        }

        public override string GetSummary()
        {
            return _delaySeconds.ToString();
        }
    }

    public partial class InnerCancelCommand : AsyncCommand
    {   
        [CommandExecute]
        async UniTask ExecuteAsync(IProcessInterface process)
        {
            new CancellationTokenSource().Cancel();
            await UniTask.CompletedTask;
        }
    }
}