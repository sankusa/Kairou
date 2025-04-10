using System;
using UnityEditor;

namespace Kairou
{
    public readonly struct LabelWidthScope : IDisposable
    {
        readonly float _originalLabelWidth;

        public LabelWidthScope(float labelWidth)
        {
            _originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        public readonly void Dispose()
        {
            EditorGUIUtility.labelWidth = _originalLabelWidth;
        }
    }
}