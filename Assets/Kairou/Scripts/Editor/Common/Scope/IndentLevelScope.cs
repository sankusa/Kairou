using System;
using UnityEditor;

namespace Kairou
{
    public readonly struct IndentLevelScope : IDisposable
    {
        readonly int _originalIndentLevel;

        public IndentLevelScope(int indentLevel)
        {
            _originalIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel;
        }

        public readonly void Dispose()
        {
            EditorGUI.indentLevel = _originalIndentLevel;
        }
    }
}