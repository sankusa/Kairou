using UnityEngine;

namespace Kairou
{
    public static class ObjectExtensions
    {
        public static bool IsDestroyed(this Object obj) => obj == null && !ReferenceEquals(obj, null);
    }
}