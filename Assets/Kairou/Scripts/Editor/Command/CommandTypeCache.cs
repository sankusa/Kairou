using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public static class CommandTypeCache
    {
        static Dictionary<string, Type> _commandDic;
        static Dictionary<string, Type> CommandDic
        {
            get
            {
                if (_commandDic == null)
                {
                    Rebuild();
                }
                return _commandDic;
            }
        }

        public static void Rebuild()
        {
            _commandDic = new Dictionary<string, Type>();
            _commandDic = TypeCache
                .GetTypesDerivedFrom<Command>()
                .ToDictionary(static type => type.FullName, static type => type);
        }

        public static void Clear()
        {
            _commandDic = null;
        }

        public static Dictionary<string, Type>.ValueCollection Types => CommandDic.Values;

        public static Type Find(string typeFullName)
        {
            if (CommandDic.TryGetValue(typeFullName, out var type))
            {
                return type;
            }
            return null;
        }
    }
}