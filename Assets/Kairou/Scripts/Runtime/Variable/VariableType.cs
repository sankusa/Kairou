using System;
using Kairou.DataStore;

namespace Kairou
{
    public abstract class VariableType
    {
        public abstract Type Type { get; }

        public virtual bool CanNegate => false;
        public virtual bool CanAdd => false;
        public virtual bool CanSubtract => false;
        public virtual bool CanMultiply => false;
        public virtual bool CanDivide => false;
        public virtual bool CanModulo => false;

        public void Register()
        {
            VariableTypeDictionary.RegisterVariableType(this);
        }
    }
    public abstract class VariableType<T> : VariableType
    {
        public sealed override Type Type => typeof(T);

        public virtual T Negate(T value) => throw new NotImplementedException();
        public virtual T Add(T value1, T value2) => throw new NotImplementedException();
        public virtual T Subtract(T value1, T value2) => throw new NotImplementedException();
        public virtual T Multiply(T value1, T value2) => throw new NotImplementedException();
        public virtual T Divide(T value1, T value2) => throw new NotImplementedException();
        public virtual T Modulo(T value1, T value2) => throw new NotImplementedException();
    }

    public interface IDataStorageAccessor<T>
    {
        bool TryGet(IDataStorage storage, string key, out T value);
        void Set(IDataStorage storage, string key, T value);
    }
}