using System;

namespace Kairou
{
    public abstract class VariableType
    {
        public abstract Type Type { get; }
        public void Register()
        {
            VariableTypeDictionary.RegisterVariableType(this);
        }
    }
    public abstract class VariableType<T> : VariableType
    {
        public override Type Type => typeof(T);
    }
}