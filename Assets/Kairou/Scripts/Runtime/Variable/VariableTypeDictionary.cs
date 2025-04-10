using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    public static class VariableTypeDictionary
    {
        static Dictionary<Type, VariableType> _dic = new();
        public static Dictionary<Type, VariableType> Dic => _dic;

        // TODO: 膨大なリフレクション情報がキャッシュされてしまうので、辞書生成処理をSourceGeneratorで生成するようにしたい
        static VariableTypeDictionary()
        {
            // Register all VariableType<T> types in the assembly
            var variableGenericBaseType = typeof(VariableType<>);
            var variableTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                {
                    return t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == variableGenericBaseType && t.IsAbstract == false && t.IsGenericType == false;
                });
            foreach (var type in variableTypes)
            {
                Type baseType = type.BaseType;
                var genericArgument = baseType.GenericTypeArguments[0];
                _dic[genericArgument] = (VariableType)Activator.CreateInstance(type);
            }
        }
    }
}