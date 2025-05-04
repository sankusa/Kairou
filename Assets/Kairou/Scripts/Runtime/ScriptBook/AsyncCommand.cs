using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public class AsyncCommandParameter
    {
        [SerializeField] bool _await = true;
        public bool Await => _await;
        [SerializeField] VariableValueSetterKey<UniTask> _uniTaskStoreVariable = new();
        public VariableValueSetterKey<UniTask> UniTaskStoreVariable => _uniTaskStoreVariable;

        public IEnumerable<string> Validate(Command command, string fieldName)
        {
            if (_await == false)
            {
                foreach (string errorMessage in _uniTaskStoreVariable.Validate(command, $"{fieldName}.{nameof(_uniTaskStoreVariable)}"))
                {
                    yield return errorMessage;
                }
            }
            yield break;
        }
    }

    public abstract class AsyncCommand : Command
    {
        [SerializeField] AsyncCommandParameter _asyncCommandParameter = new();
        public AsyncCommandParameter AsyncCommandParameter => _asyncCommandParameter;
        
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

        public override IEnumerable<string> InvokeValidate()
        {
            foreach (string errorMessage in AsyncCommandParameter.Validate(this, nameof(_asyncCommandParameter)))
            {
                yield return errorMessage;
            }
            foreach (string errorMessage in base.InvokeValidate())
            {
                yield return errorMessage;
            }
        }
    }
}