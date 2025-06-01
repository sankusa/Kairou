using System.Collections.Generic;
using System.Linq;
#if KAIROU_VCONTAINER_SUPPORT
using VContainer;
#endif
#if KAIROU_EXTENJECT_SUPPORT
using Zenject;
#endif

namespace Kairou
{
    public class CompositeObjectResolver
    {
        readonly List<IObjectResolver> _resolvers = new();

#if KAIROU_VCONTAINER_SUPPORT
        VContainer.IObjectResolver _vcontainerResolver;
#endif
#if KAIROU_EXTENJECT_SUPPORT
        DiContainer _extenjectDiContainer;
#endif

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

#if KAIROU_VCONTAINER_SUPPORT
        public void SetVContainerResolver(VContainer.IObjectResolver resolver)
        {
            if (resolver == null) return;
            _vcontainerResolver = resolver;
        }
#endif

#if KAIROU_EXTENJECT_SUPPORT
        public void SetExtenjectDiContainer(DiContainer diContainer)
        {
            if (diContainer == null) return;
            _extenjectDiContainer = diContainer;
        }
#endif

        public T Resolve<T>()
        {
            foreach (IObjectResolver resolver in _resolvers)
            {   
                if (resolver.Resolve<T>() is T t) return t;
            }
#if KAIROU_VCONTAINER_SUPPORT
            if (_vcontainerResolver != null)
            {
                if (_vcontainerResolver.TryResolve(out T resolved)) return resolved;
            }
#endif
#if KAIROU_EXTENJECT_SUPPORT
            if (_extenjectDiContainer != null)
            {
                // TryResolve()は値型に対応していないため不使用
                // 例外処理は重いので何とかしたい
                try
                {
                    T resolved = _extenjectDiContainer.Resolve<T>();
                    if (resolved != null) return resolved;
                }
                catch (ZenjectException) {}
            }
#endif
            return default;
        }

        public bool TryResolve<T>(out T value)
        {
            foreach (IObjectResolver resolver in _resolvers)
            {   
                if (resolver.Resolve<T>() is T t)
                {
                    value = t;
                    return true;
                }
            }
#if KAIROU_VCONTAINER_SUPPORT
            if (_vcontainerResolver != null)
            {
                if (_vcontainerResolver.TryResolve(out T resolved))
                {
                    value = resolved;
                    return true;
                }
            }
#endif
#if KAIROU_EXTENJECT_SUPPORT
            if (_extenjectDiContainer != null)
            {
                // TryResolve()は値型に対応していないため不使用
                // 例外処理は重いので何とかしたい
                try
                {
                    T resolved = _extenjectDiContainer.Resolve<T>();
                    if (resolved != null)
                    {
                        value = resolved;
                        return true;
                    }
                }
                catch (ZenjectException) {}
            }
#endif
            value = default;
            return false;
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            foreach (IObjectResolver resolver in _resolvers)
            {
                foreach (T t in resolver.ResolveAll<T>())
                {
                    yield return t;
                }
            }
#if KAIROU_VCONTAINER_SUPPORT
            if (_vcontainerResolver != null)
            {
                if (_vcontainerResolver.TryResolve(out T resolved)) yield return resolved;
            }
#endif
#if KAIROU_EXTENJECT_SUPPORT
            if (_extenjectDiContainer != null)
            {
                foreach (T t in _extenjectDiContainer.ResolveAll<T>())
                {
                    yield return t;
                }
            }
#endif
        }

        public void Clear()
        {
            _resolvers.Clear();
#if KAIROU_VCONTAINER_SUPPORT
            _vcontainerResolver = null;
#endif
#if KAIROU_EXTENJECT_SUPPORT
            _extenjectDiContainer = null;
#endif
        }
    }
}