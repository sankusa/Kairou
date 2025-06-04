using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public static class TickCounter
    {
        static long _count;
        public static long Count => _count++;

        static TickCounter()
        {
            EditorApplication.update += () => _count++;
        }
    }
}