using System;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public abstract class VariableDefinition
    {
        [SerializeField] string _name;
        public string Name => _name;

        public abstract Variable CreateVariable();
    }

    [Serializable]
    public class VariableDefinition<T> : VariableDefinition
    {
        [SerializeField] T _defaultValue;
        public T DefaultValue => _defaultValue;

        public override Variable CreateVariable()
        {
            return Variable<T>.Rent(this);
        }
    }
}