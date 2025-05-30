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

        public void Add(List<IObjectResolver> resolvers)
        {
            if (resolvers == null) return;
            _resolvers.AddRange(resolvers);
        }

        public T Resolve<T>()
        {
            foreach (IObjectResolver resolver in _resolvers)
            {   
                if (resolver.Resolve<T>() is T t) return t;
            }
            return default;
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return _resolvers.SelectMany(resolver => resolver.ResolveAll<T>());
        }

        public void Clear()
        {
            _resolvers.Clear();
        }
    }
}