using System;
using System.Collections.Generic;

namespace Kairou
{
    public static class PageProcessExtensions
    {
        public static Variable FindVariable(this PageProcess process, string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            Variable variable;
            if (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Page)
            {
                if (process.Variables.TryGetValue(name, out variable)) return variable;
            }
            if (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Book)
            {
                if (process.BookProcess.Variables.TryGetValue(name, out variable)) return variable;
            }
            return null;
        }

        public static Variable<T> FindVariable<T>(this PageProcess process, string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            Variable variable;
            Variable<T> typed;
            if (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Page)
            {
                process.Variables.TryGetValue(name, out variable);
                typed = variable as Variable<T>;
                if (typed != null) return typed;
            }
            if (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Book)
            {
                process.BookProcess.Variables.TryGetValue(name, out variable);
                typed = variable as Variable<T>;
                if (variable != null) return typed;
            }
            return null;
        }

        public static T Resolve<T>(this PageProcess process) where T : class
        {
            return process.BookProcess.RootProcess.ObjectResolver.Resolve<T>();
        }

        public static IEnumerable<T> ResolveAll<T>(this PageProcess process) where T : class
        {
            return process.BookProcess.RootProcess.ObjectResolver.ResolveAll<T>();
        }
    }
}