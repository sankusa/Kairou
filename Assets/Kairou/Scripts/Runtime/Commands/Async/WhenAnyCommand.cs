using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Async", "WhenAny")]
    public partial class WhenAnyCommand : AsyncCommand
    {
        [SerializeField] List<VariableValueGetterKey<UniTask>> _uniTaskVariableKeys;

        [CommandExecute]
        UniTask ExecuteAsync(IProcessInterface process, CancellationToken cancellationToken)
        {
            var uniTasks = _uniTaskVariableKeys.Select(x => x.Find(process).GetValue());
            return UniTask.WhenAny(uniTasks).AttachExternalCancellation(cancellationToken);
        }

        public override string GetSummary()
        {
            return string.Join(' ', _uniTaskVariableKeys.Select(x => x.GetSummary()));
        }

        protected override IEnumerable<string> Validate()
        {
            for (int i = 0; i < _uniTaskVariableKeys.Count; i++)
            {
                foreach (string errorMessage in _uniTaskVariableKeys[i].Validate(this, $"{nameof(_uniTaskVariableKeys)}[{i}]"))
                {
                    yield return errorMessage;
                }
            }
        }
    }
}