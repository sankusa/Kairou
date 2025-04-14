using System;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public abstract class VariableDefinition
    {
        [SerializeField] string _name;
        public string Name => _name;

        [SerializeField] bool _store;
        public bool Store => _store;

        [SerializeField] string _storeKey;
        public string StoreKey => _storeKey;

        public abstract Type TargetType { get; }

        public abstract Variable CreateVariable();
    }

    [Serializable]
    public class VariableDefinition<T> : VariableDefinition
    {
        [SerializeField] T _defaultValue;
        public T DefaultValue => _defaultValue;

        public override Type TargetType => typeof(T);

        public override Variable CreateVariable()
        {
            return Variable<T>.Rent(this);
        }
    }
}