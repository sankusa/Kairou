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
                .Where(x => x.fromType == type || type.IsSubclassOf(x.fromType))
                .Select(x => x.toType)
                .Prepend(type);
        }

        public static IEnumerable<Type> GetConvertibleFromTypes(Type type)
        {
            return TypeConverterDictionary.Dic.Keys
                .Where(x => x.toType == type)
                .Select(x => x.toType)
                .Prepend(type);
        }

        public static bool CanConvert(Type fromType, Type toType)
        {
            return TypeConverterDictionary.Dic.ContainsKey((fromType, toType)) || (fromType == toType);
        }
    }

    public static class TypeConverterCache<TFrom, TTo>
    {
        private static readonly ITypeConverter<TFrom, TTo> _converter;

        public static bool HasConverter => _converter != null;

        public static bool IsSameType => typeof(TFrom) == typeof(TTo);

        static TypeConverterCache()
        {
            if (IsSameType)
            {
                _converter = new SameTypeConverter<TFrom>() as ITypeConverter<TFrom, TTo>;
                return;
            }
            TypeConverter converter;
            Type inputType = typeof(TFrom);
            while (TypeConverterDictionary.Dic.TryGetValue((inputType, typeof(TTo)), out converter) == false)
            {
                inputType = inputType.BaseType;
            }
            _converter = converter as ITypeConverter<TFrom, TTo>;
        }

        public static TTo Convert(TFrom input)
        {
            if (_converter == null)
            {
                throw new InvalidOperationException($"Terget TypeConverter not found. {typeof(TFrom).Name} -> {typeof(TTo).Name}");
            }
            return _converter.Convert(input);
        }
    }
}