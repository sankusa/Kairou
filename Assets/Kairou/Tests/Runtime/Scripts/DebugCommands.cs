using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou.Tests.Runtime
{
    public partial class AssertCommand : Command
    {
        [GenerateValidation]
        [SerializeReference] Condition _condition = new Condition<int>();

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            if (_condition.Evaluate(process) == false)
            {
                throw new AssertionException($"{nameof(AssertCommand)} BookId = \"{ParentPage.ParentBook.Id}\", PageId = \"{ParentPage.Id}\", CommandIndex = {Index}");
            }
        }

        public override string GetSummary()
        {
            return _condition.GetSummary();
        }
    }

    public partial class ReportCommand : Command
    {
        [CommandExecute]
        void Execute()
        {
            Debug.Log($"\"{ParentPage.ParentBook.Id}\" {ParentPage.Index}");
        }
    }

    public partial class SuccessCommand : Command
    {
        [CommandExecute]
        void Execute()
        {
            Debug.Log($"Success \"{ParentPage.ParentBook.Id}\" \"{ParentPage.Id}\"");
        }
    }
}