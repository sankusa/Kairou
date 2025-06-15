using System;
using System.Collections.Generic;

namespace Kairou
{
    public static class VariableTypeCache
    {
        public static IEnumerable<Type> GetVariableTargetType()
        {
            foreach (var key in VariableTypeDictionary.Dic.Keys)
            {
                yield return key;
            }
        }

        public static VariableType GetVariableType(Type type) => VariableTypeDictionary.Dic[type];
    }

    public static class VariableTypeCache<T>
    {
        static readonly VariableType<T> _variableType;
        public static VariableType<T> VariableType => _variableType;

        static VariableTypeCache()
        {
            _variableType = VariableTypeDictionary.Dic[typeof(T)] as VariableType<T>;
        }
    }
}