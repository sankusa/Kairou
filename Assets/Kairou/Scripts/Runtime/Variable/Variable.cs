using System;

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
            variable._definition = definition;
            variable.Value = definition.DefaultValue;
            return variable;
        }

        public static void Return(Variable<T> variable)
        {
            _pool.Return(variable);
        }

        public T Value { get; set;}
        public override object ValueAsObject => Value;
        public override Type Type => typeof(T);

        private Variable() {}

        public override void Return()
        {
            Return(this);
        }

        public override void Clear()
        {
            base.Clear();
            Value = default;
        }
    }
}