using System;

namespace Kairou
{
    public abstract class TypeConverter
    {
        public abstract Type InputType { get;}
        public abstract Type OutputType { get;}
        public void Register()
        {
            TypeConverterDictionary.RegisterConverter(this);
        }
    }
    public interface ITypeConverter<in TInput, TOutput>
    {
        TOutput Convert(TInput input);
    }
    public abstract class TypeConverter<TInput, TOutput> : TypeConverter, ITypeConverter<TInput, TOutput>
    {
        public override Type InputType => typeof(TInput);
        public override Type OutputType => typeof(TOutput);

        public abstract TOutput Convert(TInput input);
    }
    public class SameTypeConverter<T> : TypeConverter<T, T>
    {
        public override T Convert(T input)
        {
            return input;
        }
    }
}