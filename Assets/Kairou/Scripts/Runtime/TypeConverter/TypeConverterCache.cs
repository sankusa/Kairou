using System;
using System.Collections.Generic;
using System.Linq;

namespace Kairou
{
    public static class TypeConverterCache
    {
        public static IEnumerable<Type> GetConvertibleToTypes(Type type)
        {
            return TypeConverterDictionary.Dic.Keys
                .Where(x => x.inputType == type || type.IsSubclassOf(x.inputType))
                .Select(x => x.outputType);
        }

        public static IEnumerable<Type> GetConvertibleFromTypes(Type type)
        {
            return TypeConverterDictionary.Dic.Keys
                .Where(x => x.outputType == type)
                .Select(x => x.inputType);
        }
    }

    public static class TypeConverterCache<TInput, TOutput>
    {
        private static readonly ITypeConverter<TInput, TOutput> _converter;

        public static bool HasConverter => _converter != null;

        public static bool IsSameType => typeof(TInput) == typeof(TOutput);

        static TypeConverterCache()
        {
            if (IsSameType)
            {
                _converter = new SameTypeConverter<TInput>() as ITypeConverter<TInput, TOutput>;
                return;
            }
            TypeConverter converter;
            Type inputType = typeof(TInput);
            while (TypeConverterDictionary.Dic.TryGetValue((inputType, typeof(TOutput)), out converter) == false)
            {
                inputType = inputType.BaseType;
            }
            _converter = converter as ITypeConverter<TInput, TOutput>;
        }

        public static TOutput Convert(TInput input)
        {
            if (_converter == null)
            {
                throw new InvalidOperationException($"Terget TypeConverter not found. {typeof(TInput).Name} -> {typeof(TOutput).Name}");
            }
            return _converter.Convert(input);
        }
    }
}