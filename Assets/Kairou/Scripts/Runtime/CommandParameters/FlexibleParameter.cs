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
        [SerializeField] VariableValueGetterKey<T> _variable;

        public T ResolveValue(IProcessInterface process)
        {
            return _resolveType switch
            {
                ResolveType.Value => _value,
                ResolveType.Variable => _variable.Find(process).GetValue(),
                _ => throw new InvalidOperationException()
            };
        }

        public string GetSummary()
        {
            return _resolveType switch
            {
                ResolveType.Value => _value.ToString(),
                ResolveType.Variable => _variable.GetSummary(),
                _ => throw new InvalidOperationException()
            };
        }

        public override IEnumerable<string> Validate(Command command, string fieldName)
        {
            switch (_resolveType)
            {
                case ResolveType.Value:
                    break;
                case ResolveType.Variable:
                    foreach (string errorMessage in _variable.Validate(command, $"{fieldName}.{nameof(_variable)}"))
                    {
                        yield return errorMessage;
                    }
                    break;
                default:
                    yield return $"{fieldName} : Unknown ResolveType: {_resolveType}";
                    break;
            }
        }
    }
}