using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public static class EditorUtil
    {
        public static bool TryGetActiveFolderPath( out string path )
        {
            var tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );

            var args = new object[] { null };
            bool found = (bool)tryGetActiveFolderPath.Invoke( null, args );
            path = (string)args[0];

            return found;
        }
    }
}