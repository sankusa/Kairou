using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public abstract class VariableKey
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
            return command.ValidateVariableKey(
                fieldName,
                _variableName,
                _targetScope,
                (variable, fieldName) => ValidateVariableDefine(variable, fieldName)
            );
        }

        IEnumerable<string> ValidateVariableDefine(VariableDefinition variable, string fieldName)
        {
            if (variable.TargetType != TargetType)
            {
                yield return $"{fieldName} : Variable type mismatch. Expected: {TargetType}, Actual: {variable.TargetType}, VariableName: {_variableName}";
            }
        }
    }

    [Serializable]
    public class VariableKey<T> : VariableKey
    {
        public override Type TargetType => typeof(T);

        public new Variable<T> Find(IProcessInterface process)
        {
            return process.FindVariable<T>(VariableName, TargetScope);
        }
    }
}