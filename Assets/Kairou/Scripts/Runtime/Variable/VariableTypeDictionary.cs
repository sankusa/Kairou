using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    internal static class VariableTypeDictionary
    {
        public static Dictionary<Type, VariableType> Dic { get; } = new();

        public static void RegisterVariableType(VariableType variableType)
        {
            if (Dic.ContainsKey(variableType.Type))
            {
                throw new ArgumentException($"VariableType for {variableType.Type} already registered.");
            }
            Dic[variableType.Type] = variableType;
        }
    }
}