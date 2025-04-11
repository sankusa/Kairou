using System;
using UnityEngine;

namespace Kairou
{
    public abstract class Variable
    {
        protected VariableDefinition _definition;
        public string Name => _definition.Name;

        public abstract object ValueAsObject { get; }
        public abstract Type Type { get; }

        public abstract void Return();

        public virtual void Clear()
        {
            _definition = null;
        }
    }

    public class Variable<T> : Variable
    {
        static readonly ObjectPool<Variable<T>> _pool = new(
            createFunc: static () => new Variable<T>(),
            onRent: static variable =>
            {

            },
            onReturn: static variable =>
            {
                variable.Clear();
            }
        );

        public static Variable<T> Rent(VariableDefinition<T> definition)
        {
            Variable<T> variable = _pool.Rent();
            variable.SetUp(definition);
            return variable;
        }

        public static void Return(Variable<T> variable)
        {
            _pool.Return(variable);
        }

        T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (_definition.Store)
                {
                    IDataStore.Instance.SetValue(_definition.StoreKey, value);
                }
                _value = value;
            }
        }
        public override object ValueAsObject => Value;
        public override Type Type => typeof(T);

        private Variable() {}

        void SetUp(VariableDefinition<T> definition)
        {
            _definition = definition;
            if (definition.Store)
            {
                if (IDataStore.Instance.TryGetValue(definition.Name, out T value))
                {
                    _value = value;
                }
                else
                {
                    _value = definition.DefaultValue;
                    IDataStore.Instance.SetValue(definition.StoreKey, _value);
                }
            }
            else
            {
                _value = definition.DefaultValue;
            }
        }

        public override void Return()
        {
            Return(this);
        }

        public override void Clear()
        {
            base.Clear();
            _value = default;
        }
    }
}