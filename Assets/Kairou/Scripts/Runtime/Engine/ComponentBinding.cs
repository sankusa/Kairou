using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    public class ComponentBinding : MonoBehaviour, IObjectResolver
    {
        [SerializeField, ComponentSelector] List<Component> _components;
        public List<Component> Components => _components;

        public T Resolve<T>()
        {
            foreach (Component component in _components)
            {
                if (component is T t) return t;
            }
            return default;
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return _components.OfType<T>();
        }
    }
}