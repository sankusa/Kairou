using System;
using System.Collections.Generic;
using System.Linq;

namespace Kairou
{
    internal static class TypeConverterDictionary
    {
        public static Dictionary<(Type fromType, Type toType), TypeConverter> Dic { get; private set; } = new();

        public static void RegisterConverter(TypeConverter converter)
        {
            if (Dic.ContainsKey((converter.FromType, converter.ToType)))
            {
                throw new ArgumentException($"Converter for {converter.FromType} to {converter.ToType} already registered.");
            }
            Dic[(converter.FromType, converter.ToType)] = converter;
        }
    }
}