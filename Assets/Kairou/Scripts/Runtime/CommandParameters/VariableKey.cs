using System;
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

        public Variable FindVariable(PageProcess process)
        {
            return process.FindVariable(VariableName, TargetScope);
        }
    }

    [Serializable]
    public class VariableKey<T> : VariableKey
    {
        public new Variable<T> FindVariable(PageProcess process)
        {
            return process.FindVariable<T>(VariableName, TargetScope);
        }
    }
}