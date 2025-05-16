using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public abstract class VariableValueGetterKey : IValidatableAsCommandField
    {
        [SerializeField] TargetVariableScope _targetScope = TargetVariableScope.None;
        public TargetVariableScope TargetScope => _targetScope;

        [SerializeField] string _variableName;
        public string VariableName => _variableName;

        public bool IsEmpty() => string.IsNullOrEmpty(_variableName);

        public abstract Type TargetType { get; }

        public Variable Find(IProcessInterface process)
        {
            return process.FindVariable(VariableName, TargetScope);
        }

        public IEnumerable<string> Validate(Command command, string fieldName)
        {
            return VariableKeyValidator.Validate(
                command,
                fieldName,
                _variableName,
                _targetScope,
                (variable, fieldName) => ValidateVariableDefine(variable, fieldName)
            );
        }

        IEnumerable<string> ValidateVariableDefine(VariableDefinition variable, string fieldName)
        {
            if (TypeConverterCache.CanConvert(variable.TargetType, TargetType) == false)
            {
                yield return $"{fieldName} : The found variable cannot convert. TargetType: {TargetType.Name}, VariableType: {variable.TargetType}, VariableName: {_variableName}";
            }
        }
    }

    [Serializable]
    public class VariableValueGetterKey<T> : VariableValueGetterKey
    {
        public override Type TargetType => typeof(T);
        
        public new VariableValueGetter<T> Find(IProcessInterface process)
        {
            return process.FindVariableValueGetter<T>(VariableName, TargetScope);
        }
    }
}