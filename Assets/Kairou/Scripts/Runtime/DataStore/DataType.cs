using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kairou.DataStore
{
    public abstract class DataType
    {
        public abstract Type TargetType { get; }
        public abstract string TypeId { get; }
        public void Register() => DataTypeDictionary.Register(this);
        public abstract string Normalize(string value);
#if UNITY_EDITOR
        public virtual string DrawField(Rect rect, string label, string value) => EditorGUI.TextField(rect, value);
        public virtual float GetFieldHeight(string value) => EditorGUIUtility.singleLineHeight;
#endif
    }

    public abstract class DataType<T> : DataType
    {
        public override Type TargetType => typeof(T);
        public abstract string ToString(T value);
        public abstract T FromString(string value);
        public override string Normalize(string value)
        {
            return ToString(FromString(value));
        }
    }
}