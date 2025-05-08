using System;
using System.Collections.Generic;

namespace Kairou
{
    public static class PageProcessExtensions
    {
        public static Variable FindVariable(this PageProcess process, string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return FindVariableInternal(process, name, targetScope, static variable => true);
        }

        public static Variable<T> FindVariable<T>(this PageProcess process, string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return FindVariableInternal(process, name, targetScope, static variable => variable is Variable<T>) as Variable<T>;
        }

        public static VariableValueGetter<T> FindVariableValueGetter<T>(this PageProcess process, string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return new VariableValueGetter<T>(FindVariableInternal(process, name, targetScope, static variable => variable.CanConvertTo<T>()));
        }

        public static VariableValueSetter<T> FindVariableValueSetter<T>(this PageProcess process, string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return new VariableValueSetter<T>(FindVariableInternal(process, name, targetScope, static variable => variable.CanConvertFrom<T>()));
        }

        public static VariableValueAccessor<T> FindVariableValueAccessor<T>(this PageProcess process, string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return new VariableValueAccessor<T>(FindVariableInternal(process, name, targetScope, static variable => variable.CanMutuallyConvert<T>()));
        }

        static Variable FindVariableInternal(PageProcess process, string name, TargetVariableScope targetScope, Func<Variable, bool> filter)
        {
            Variable variable;
            if (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Page)
            {
                process.Variables.TryGetValue(name, out variable);
                if (variable != null && filter.Invoke(variable)) return variable;
            }
            if (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Book)
            {
                process.BookProcess.Variables.TryGetValue(name, out variable);
                if (variable != null && filter.Invoke(variable)) return variable;
            }
            return null;
        }

        public static T Resolve<T>(this PageProcess process)
        {
            return process.BookProcess.SeriesProcess.RootProcess.ObjectResolver.Resolve<T>();
        }

        public static IEnumerable<T> ResolveAll<T>(this PageProcess process)
        {
            return process.BookProcess.SeriesProcess.RootProcess.ObjectResolver.ResolveAll<T>();
        }
    }
}