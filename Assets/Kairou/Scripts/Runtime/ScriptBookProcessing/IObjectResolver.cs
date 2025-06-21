using System;
using System.Collections.Generic;

namespace Kairou
{
    public interface IObjectResolver
    {
        T Resolve<T>();
        IEnumerable<T> ResolveAll<T>();
        object Resolve(Type type);
        IEnumerable<object> ResolveAll(Type type);
    }
}