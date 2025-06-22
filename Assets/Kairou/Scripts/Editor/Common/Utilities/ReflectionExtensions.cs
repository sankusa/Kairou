using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kairou.Editor
{
    public static class ReflectionExtensions
    {
        public static string GetDisplayLabel(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return "";
            }
            var sb = new StringBuilder();
            if (methodInfo.IsStatic) sb.Append("static ");
            sb.Append(TypeNameUtil.ConvertToPrimitiveTypeName(methodInfo.ReturnType.Name));
            sb.Append(" ");
            sb.Append(methodInfo.Name);
            sb.Append("(");
            var parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                sb.Append(TypeNameUtil.ConvertToPrimitiveTypeName(parameter.ParameterType.Name));
                sb.Append(" ");
                sb.Append(parameter.Name);
                if (parameter.HasDefaultValue)
                {
                    sb.Append(" = ");
                    sb.Append(parameter.DefaultValue);
                }
                if (i < parameters.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}