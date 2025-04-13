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

        internal abstract void Return();

        internal virtual void Clear()
        {
            _definition = null;
        }

        public abstract T ConvertTo<T>();
        public abstract void ConvertFrom<T>(T value);
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
            get
            {
                SyncWithStoreIfNeeded();
                return _value;
            }
            set
            {
                _value = value;
                SetToStoreIfNeeded();
            }
        }

        public override object ValueAsObject => Value;
        public override Type Type => typeof(T);

        private Variable() {}

        void SetUp(VariableDefinition<T> definition)
        {
            _definition = definition;
            _value = definition.DefaultValue;
            SyncWithStoreIfNeeded();
        }

        void SetToStoreIfNeeded()
        {
            if (_definition.Store)
            {
                IDataStore.Instance.SetValue(_definition.StoreKey, _value);
            }
        }

        void SyncWithStoreIfNeeded()
        {
            if (_definition.Store)
            {
                if (IDataStore.Instance.TryGetValue(_definition.Name, out T value))
                {
                    _value = value;
                }
                else
                {
                    IDataStore.Instance.SetValue(_definition.StoreKey, _value);
                }
            }
        }

        internal override void Return()
        {
            Return(this);
        }

        internal override void Clear()
        {
            base.Clear();
            _value = default;
        }

        public override U ConvertTo<U>()
        {
            return TypeConverterCache<T, U>.Convert(Value);
        }

        public override void ConvertFrom<U>(U value)
        {
            Value = TypeConverterCache<U, T>.Convert(value);
        }
    }
}