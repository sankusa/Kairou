using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public static class CommandTypeCache
    {
        static Dictionary<string, Type> _commandDic;
        static Dictionary<Type, CommandInfoAttribute> _commandInfoDic;

        static CommandTypeCache()
        {
            _commandDic = new Dictionary<string, Type>();
            _commandInfoDic = new Dictionary<Type, CommandInfoAttribute>();

            foreach (var type in TypeCache.GetTypesDerivedFrom<Command>())
            {
                if (type.IsAbstract) continue;
                _commandDic[type.FullName] = type;

                var commandInfo = type.GetCustomAttribute<CommandInfoAttribute>();
                if (commandInfo == null) continue;
                _commandInfoDic[type] = commandInfo;
            }
        }

        public static Dictionary<string, Type>.ValueCollection Types => _commandDic.Values;

        public static Dictionary<Type, CommandInfoAttribute>.ValueCollection CommandInfos => _commandInfoDic.Values;

        public static Type Find(string typeFullName)
        {
            if (_commandDic.TryGetValue(typeFullName, out var type))
            {
                return type;
            }
            return null;
        }

        public static CommandInfoAttribute FindCommandInfo(Type type)
        {
            if (_commandInfoDic.TryGetValue(type, out var info))
            {
                return info;
            }
            return null;
        }

        public static IEnumerable<string> GetCategoryPathsFromCommandInfo()
        {
            return _commandInfoDic.Values.Select(x => x.CategoryName).Distinct();
        }
    }
}