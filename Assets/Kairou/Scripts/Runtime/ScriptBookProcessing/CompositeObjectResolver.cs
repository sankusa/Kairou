using System.Collections.Generic;
using System.Linq;

namespace Kairou
{
    public class CompositeObjectResolver
    {
        readonly List<IObjectResolver> _resolvers = new();

        public void Add(IObjectResolver resolver)
        {
            if (resolver == null) return;
            _resolvers.Add(resolver);
        }

        public T Resolve<T>() where T : class
        {
            foreach (IObjectResolver resolver in _resolvers)
            {   
                if (resolver.Resolve<T>() is T t) return t;
            }
            return null;
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _resolvers.SelectMany(resolver => resolver.ResolveAll<T>());
        }

        public void Clear()
        {
            _resolvers.Clear();
        }
    }
}