using System.Collections.Generic;

namespace Kairou
{
    public interface IObjectResolver
    {
        T Resolve<T>() where T : class;
        IEnumerable<T> ResolveAll<T>() where T : class;
    }
}