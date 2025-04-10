using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    public static class TypeNameUtil
    {
        public static string ConvertToPrimitiveTypeName(string typeName)
        {
            return typeName switch
            {
                "Boolean" => "bool",
                "Byte" => "byte",
                "SByte" => "sbyte",
                "Char" => "char",
                "Decimal" => "decimal",
                "Double" => "double",
                "Single" => "float",
                "Int32" => "int",
                "UInt32" => "uint",
                "Int64" => "long",
                "UInt64" => "ulong",
                "Object" => "object",
                "Int16" => "short",
                "UInt16" => "ushort",
                "String" => "string",
                "Void" => "void",
                _ => typeName,
            };
        }
    }
}