using System;
using System.Collections.Generic;
using System.Linq;

namespace Kairou
{
    internal static class TypeConverterDictionary
    {
        public static Dictionary<(Type inputType, Type outputType), TypeConverter> Dic { get; private set; } = new();

        public static void RegisterConverter(TypeConverter converter)
        {
            if (Dic.ContainsKey((converter.InputType, converter.OutputType)))
            {
                throw new ArgumentException($"Converter for {converter.InputType} to {converter.OutputType} already registered.");
            }
            Dic[(converter.InputType, converter.OutputType)] = converter;
        }
    }
}