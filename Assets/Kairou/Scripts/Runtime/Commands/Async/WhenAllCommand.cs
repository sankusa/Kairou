using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Async", "WhenAll")]
    public partial class WhenAllCommand : AsyncCommand
    {
        [SerializeField] List<VariableValueGetterKey<UniTask>> _uniTaskVariableKeys;

        [CommandExecute]
        async UniTask ExecuteAsync(IProcessInterface process, CancellationToken cancellationToken)
        {
            var uniTasks = _uniTaskVariableKeys.Select(x => x.Find(process).GetValue());
            await UniTask.WhenAll(uniTasks).AttachExternalCancellation(cancellationToken);
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