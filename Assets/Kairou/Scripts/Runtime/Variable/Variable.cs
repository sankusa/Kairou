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

        internal abstract void ReturnToPool();

        internal virtual void Clear()
        {
            _definition = null;
        }

        public abstract bool CanConvertTo<T>();
        public abstract bool CanConvertFrom<T>();
        public bool CanMutuallyConvert<T>()
        {
            return CanConvertTo<T>() && CanConvertFrom<T>();
        }

        public abstract T GetValueAs<T>();
        public abstract void SetValueAs<T>(T value);
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
            },
            initialCapacity: 0,
            maxCapacity: -1,
            initialElements: 0
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

        internal override void ReturnToPool()
        {
            Return(this);
        }

        internal override void Clear()
        {
            base.Clear();
            _value = default;
        }

        public override bool CanConvertTo<U>()
        {
            return TypeConverterCache<T, U>.HasConverter;
        }

        public override bool CanConvertFrom<U>()
        {
            return TypeConverterCache<U, T>.HasConverter;
        }

        public override U GetValueAs<U>()
        {
            return TypeConverterCache<T, U>.Convert(Value);
        }

        public override void SetValueAs<U>(U value)
        {
            Value = TypeConverterCache<U, T>.Convert(value);
        }
    }

    public readonly struct VariableValueGetter<T>
    {
        readonly Variable _variable;
        public VariableValueGetter(Variable variable)
        {
            _variable = variable;
            if (variable != null && variable.CanConvertTo<T>() == false)
            {
                throw new InvalidOperationException($"Variable<{variable.Type.Name}> cannot convert to {typeof(T).Name}. VariableName: {variable.Name}.");
            }
        }
        public readonly bool HasVariable => _variable != null;
        public readonly T GetValue()
        {
            return _variable.GetValueAs<T>();
        }
    }
    public readonly struct VariableValueSetter<T>
    {
        readonly Variable _variable;
        public VariableValueSetter(Variable variable)
        {
            _variable = variable;
            if (variable != null && variable.CanConvertFrom<T>() == false)
            {
                throw new InvalidOperationException($"Variable<{variable.Type.Name}> cannot convert from {typeof(T).Name}. VariableName: {variable.Name}.");
            }
        }
        public readonly bool HasVariable => _variable != null;
        public readonly void SetValue(T value)
        {
            _variable.SetValueAs(value);
        }
    }
    public readonly struct VariableValueAccessor<T>
    {
        readonly Variable _variable;
        public VariableValueAccessor(Variable variable)
        {
            _variable = variable;
            if (variable != null && variable.CanMutuallyConvert<T>() == false)
            {
                throw new InvalidOperationException($"Variable<{variable.Type.Name}> cannot convert mutually {typeof(T).Name}. VariableName: {variable.Name}.");
            }
        }
        public readonly bool HasVariable => _variable != null;
        public readonly T GetValue()
        {
            return _variable.GetValueAs<T>();
        }
        public readonly void SetValue(T value)
        {
            _variable.SetValueAs(value);
        }
    }
}