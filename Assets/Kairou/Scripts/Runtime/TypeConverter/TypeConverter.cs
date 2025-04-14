using System;

namespace Kairou
{
    public abstract class TypeConverter
    {
        public abstract Type FromType { get;}
        public abstract Type ToType { get;}
        public void Register()
        {
            TypeConverterDictionary.RegisterConverter(this);
        }
    }
    public interface ITypeConverter<in TFrom, TTo>
    {
        TTo Convert(TFrom fromValue);
    }
    public abstract class TypeConverter<TFrom, TTo> : TypeConverter, ITypeConverter<TFrom, TTo>
    {
        public override Type FromType => typeof(TFrom);
        public override Type ToType => typeof(TTo);

        public abstract TTo Convert(TFrom fromValue);
    }
    public class SameTypeConverter<T> : TypeConverter<T, T>
    {
        public override T Convert(T fromValue)
        {
            return fromValue;
        }
    }
}