using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public abstract class FlexibleParameter : IValidatableAsCommandField
    {
        public enum ResolveType
        {
            Value = 0,
            Variable = 1,
        }

        [SerializeField] protected ResolveType _resolveType;

        public abstract IEnumerable<string> Validate(Command command, string fieldName);
    }

    [Serializable]
    public class FlexibleParameter<T> : FlexibleParameter
    {
        [SerializeField] T _value;
        [SerializeField] VariableValueGetterKey<T> _variableValueGetterKey;

        public T ResolveValue(IProcessInterface process)
        {
            return _resolveType switch
            {
                ResolveType.Value => _value,
                ResolveType.Variable => _variableValueGetterKey.Find(process).GetValue(),
                _ => throw new InvalidOperationException()
            };
        }

        public override IEnumerable<string> Validate(Command command, string fieldName)
        {
            if (_resolveType == ResolveType.Value) {}
            else if (_resolveType == ResolveType.Variable)
            {
                foreach (string errorMessage in _variableValueGetterKey.Validate(command, fieldName))
                {
                    yield return errorMessage;
                }
            }
            else
            {
                yield return $"{fieldName} : Unknown ResolveType: {_resolveType}";
            }
        }
    }
}