using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public abstract class VariableValueAccessorKey : IValidatableAsCommandField
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

        public string GetSummary() => VariableKeySharedLogic.GetSummary(_variableName);

        public IEnumerable<string> Validate(Command command, string fieldName)
        {
            return VariableKeySharedLogic.Validate(
                command,
                fieldName,
                _variableName,
                _targetScope,
                (variable, fieldName) => ValidateVariableDefine(variable, fieldName)
            );
        }

        IEnumerable<string> ValidateVariableDefine(VariableDefinition variable, string fieldName)
        {
            if (TypeConverterCache.CanConvert(variable.TargetType, TargetType) == false || TypeConverterCache.CanConvert(TargetType, variable.TargetType) == false)
            {
                yield return $"{fieldName} : The found variable cannot convert mutually. TergetType: {TargetType.Name}, VariableType: {variable.TargetType}, VariableName: {_variableName}";
            }
        }
    }

    [Serializable]
    public class VariableValueAccessorKey<T> : VariableValueAccessorKey
    {
        public override Type TargetType => typeof(T);
        
        public new VariableValueAccessor<T> Find(IProcessInterface process)
        {
            return process.FindVariableValueAccessor<T>(VariableName, TargetScope);
        }
    }
}