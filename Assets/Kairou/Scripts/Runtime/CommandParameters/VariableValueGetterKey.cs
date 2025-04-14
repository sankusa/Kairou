using System;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public abstract class VariableValueGetterKey
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
    public class VariableValueGetterKey<T> : VariableValueGetterKey
    {
        public override Type TargetType => typeof(T);
        
        public new VariableValueGetter<T> Find(PageProcess process)
        {
            return process.FindVariableValueGetter<T>(VariableName, TargetScope);
        }
    }
}