using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    public static class ObjectExtensions
    {
        public static void ChangeWithUndo(this Object obj, string undoName, Action<Object> changeAction)
        {
            Undo.RecordObject(obj, undoName);
            changeAction?.Invoke(obj);
            EditorUtility.SetDirty(obj);
        }
    }
}