using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kairou.DataStore
{
    static class DataTypeDefinitionRegistrar
    {
        static bool _isRegistered;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            if (_isRegistered) return;
            _isRegistered = true;
            new StringDataType().Register();
            new IntDataType().Register();
            new FloatDataType().Register();
            new BoolDataType().Register();
            new Vector2DataType().Register();
            new Vector3DataType().Register();
        }
    }

    public class StringDataType : DataType<string>
    {
        public override string TypeId => "String";
        public override string FromString(string value) => value;
        public override string ToString(string value) => value;
#if UNITY_EDITOR
        public override string DrawField(Rect rect, string label, string value)
        {
            if (label == "")
            {
                return EditorGUI.TextArea(rect, value);
            }
            else
            {
                var labelRect = new Rect(rect) { width = EditorGUIUtility.labelWidth, height = EditorGUIUtility.singleLineHeight };
                EditorGUI.LabelField(labelRect, label);
                rect.xMin = labelRect.xMax + EditorGUIUtility.standardVerticalSpacing;
                return EditorGUI.TextArea(rect, value);
            }
        }
        public override float GetFieldHeight(string value)
        {
            return EditorStyles.textArea.CalcHeight(new GUIContent(value), EditorGUIUtility.currentViewWidth);
        }
#endif
    }

    public class IntDataType : DataType<int>
    {
        public override string TypeId => "Int";
        public override int FromString(string value) => int.Parse(value);
        public override string ToString(int value) => value.ToString();
#if UNITY_EDITOR
        public override string DrawField(Rect rect, string label, string value)
        {
            int.TryParse(value, out int intValue);
            return EditorGUI.IntField(rect, label, intValue).ToString();
        }
#endif
    }

    public class FloatDataType : DataType<float>
    {
        public override string TypeId => "Float";
        public override float FromString(string value) => float.Parse(value);
        public override string ToString(float value) => value.ToString();
#if UNITY_EDITOR
        public override string DrawField(Rect rect, string label, string value)
        {
            float.TryParse(value, out float floatValue);
            return EditorGUI.FloatField(rect, label, floatValue).ToString();
        }
#endif
    }

    public class BoolDataType : DataType<bool>
    {
        public override string TypeId => "Bool";
        public override bool FromString(string value) => bool.Parse(value);
        public override string ToString(bool value) => value.ToString();
#if UNITY_EDITOR
        public override string DrawField(Rect rect, string label, string value)
        {
            bool.TryParse(value, out bool boolValue);
            return EditorGUI.Toggle(rect, label, boolValue).ToString();
        }
#endif
    }

    public class Vector2DataType : DataType<Vector2>
    {
        public override string TypeId => "Vector2";
        public override Vector2 FromString(string value)
        {
            var values = value.Trim('(', ')', ' ').Split(',');
            if (values.Length != 2) throw new InvalidOperationException("Invalid Vector2 format: " + value);
            return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
        }
        public override string ToString(Vector2 value) => value.ToString();
#if UNITY_EDITOR
        public override string DrawField(Rect rect, string label, string value)
        {
            var values = value.Trim('(', ')', ' ').Split(',');
            Vector2 vector = Vector2.zero;
            if (values.Length == 2 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y))
            {
                vector = new Vector2(x, y);
            }
            return EditorGUI.Vector2Field(rect, label, vector).ToString();
        }
#endif
    }

    public class Vector3DataType : DataType<Vector3>
    {
        public override string TypeId => "Vector3";
        public override Vector3 FromString(string value)
        {
            var values = value.Trim('(', ')', ' ').Split(',');
            if (values.Length != 3) throw new InvalidOperationException("Invalid Vector2 format: " + value);
            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }
        public override string ToString(Vector3 value) => value.ToString();
#if UNITY_EDITOR
        public override string DrawField(Rect rect, string label, string value)
        {
            var values = value.Trim('(', ')', ' ').Split(',');
            Vector3 vector = Vector3.zero;
            if (values.Length == 3 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
            {
                vector = new Vector3(x, y, z);
            }
            return EditorGUI.Vector3Field(rect, label, vector).ToString();
        }
#endif
    }
}