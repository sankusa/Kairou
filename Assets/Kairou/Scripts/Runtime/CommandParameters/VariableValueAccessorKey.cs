using System;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public abstract class VariableValueAccessorKey
    {
        [SerializeField] TargetVariableScope _targetScope = TargetVariableScope.None;
        public TargetVariableScope TargetScope => _targetScope;

        [SerializeField] string _variableName;
        public string VariableName => _variableName;

        public abstract Type TargetType { get; }

        public Variable Find(PageProcess process)
        {
            return process.FindVariable(VariableName, TargetScope);
        }
    }

    [Serializable]
    public class VariableValueAccessorKey<T> : VariableValueAccessorKey
    {
        public override Type TargetType => typeof(T);
        
        public new VariableValueAccessor<T> Find(PageProcess process)
        {
            return process.FindVariableValueAccessor<T>(VariableName, TargetScope);
        }
    }
}