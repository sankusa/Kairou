using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou.DataStore
{
    internal static class DataTypeDictionary
    {
        public static Dictionary<Type, DataType> Dic { get; } = new();

        public static void Register(DataType type)
        {
            if (Dic.ContainsKey(type.TargetType))
            {
                throw new ArgumentException($"Type '{type.TargetType}' already registered.");
            }
            Dic[type.TargetType] = type;
        }
    }

    public class DataTypeCache
    {
        public static IEnumerable<DataType> GetTypes()
        {
            foreach (var value in DataTypeDictionary.Dic.Values)
            {
                yield return value;
            }
        }

        public static DataType Get(string typeId) => DataTypeDictionary.Dic.Values.First(x => x.TypeId == typeId);
        public static DataType Find(string typeId) => DataTypeDictionary.Dic.Values.FirstOrDefault(x => x.TypeId == typeId);
    }

    public static class DataTypeCache<T>
    {
        static readonly DataType<T> _type;
        public static DataType<T> Type => _type;

        static DataTypeCache()
        {
            _type = DataTypeDictionary.Dic[typeof(T)] as DataType<T>;
        }
    }
}