using System;
using System.Collections.Generic;

namespace Kairou
{
    public static class PageProcessExtensions
    {
        public static Variable FindVariable(this PageProcess process, string name)
        {
            Variable variable;
            process.Variables.TryGetValue(name, out variable);
            if (variable != null) return variable;
            process.BookProcess.Variables.TryGetValue(name, out variable);
            if (variable != null) return variable;
            return null;
        }

        public static Variable<T> FindVariable<T>(this PageProcess process, VariableKey<T> key)
        {
            Variable variable;
            Variable<T> typed;
            process.Variables.TryGetValue(key.Name, out variable);
            typed = variable as Variable<T>;
            if (typed != null) return typed;
            process.BookProcess.Variables.TryGetValue(key.Name, out variable);
            typed = variable as Variable<T>;
            if (variable != null) return typed;
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