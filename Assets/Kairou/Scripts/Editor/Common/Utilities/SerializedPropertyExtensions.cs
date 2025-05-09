using System;
using UnityEditor;

namespace Kairou.Editor
{
    public static class SerializedPropertyExtensions
    {
        public static object GetObject(this SerializedProperty prop) {
            string path = SerializedPropertyUtil.ConvertToReflectionPath(prop.propertyPath);
            object obj = prop.serializedObject.targetObject;
            return ReflectionUtil.GetObject(obj, path);
        }

        public static int GetArrayElementIndex(this SerializedProperty prop) {
            string path = prop.propertyPath;
            int indexStartIndex = path.LastIndexOf('[') + 1;
            int closeBracketIndex = path.LastIndexOf(']');
            var indexString = path.AsSpan(indexStartIndex, closeBracketIndex - indexStartIndex);
            return int.Parse(indexString);
        }

        public static SerializedProperty GetParent(this SerializedProperty prop)
        {
            int lastDotIndex = prop.propertyPath.LastIndexOf('.');
            if (lastDotIndex == -1) return null;
            string parentPath = prop.propertyPath[..lastDotIndex];
            return prop.serializedObject.FindProperty(parentPath);
        }

        // リスト(要素数が違う場合)には非対応
        public static void CopyFrom(this SerializedProperty prop, SerializedProperty source) {
            SerializedProperty propCopy = prop.Copy();
            SerializedProperty sourceCopy = source.Copy();

            int depth = propCopy.depth;
            while (propCopy.NextVisible(true) && sourceCopy.NextVisible(true)) {
                if (propCopy.depth <= depth) break;
                switch (sourceCopy.propertyType) {
                    case SerializedPropertyType.Integer:
                        propCopy.intValue = sourceCopy.intValue;
                        break;
                    case SerializedPropertyType.Boolean:
                        propCopy.boolValue = sourceCopy.boolValue;
                        break;
                    case SerializedPropertyType.Float:
                        propCopy.floatValue = sourceCopy.floatValue;
                        break;
                    case SerializedPropertyType.String:
                        propCopy.stringValue = sourceCopy.stringValue;
                        break;
                    case SerializedPropertyType.Color:
                        propCopy.colorValue = sourceCopy.colorValue;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        propCopy.objectReferenceValue = sourceCopy.objectReferenceValue;
                        break;
                    case SerializedPropertyType.LayerMask:
                        propCopy.intValue = sourceCopy.intValue;
                        break;
                    case SerializedPropertyType.Enum:
                        propCopy.enumValueIndex = sourceCopy.enumValueIndex;
                        break;
                    case SerializedPropertyType.Vector2:
                        propCopy.vector2Value = sourceCopy.vector2Value;
                        break;
                    case SerializedPropertyType.Vector3:
                        propCopy.vector3Value = sourceCopy.vector3Value;
                        break;
                    case SerializedPropertyType.Vector4:
                        propCopy.vector4Value = sourceCopy.vector4Value;
                        break;
                    case SerializedPropertyType.Rect:
                        propCopy.rectValue = sourceCopy.rectValue;
                        break;
                    case SerializedPropertyType.AnimationCurve:
                        propCopy.animationCurveValue = sourceCopy.animationCurveValue;
                        break;
                    case SerializedPropertyType.Bounds:
                        propCopy.boundsValue = sourceCopy.boundsValue;
                        break;
                    case SerializedPropertyType.Gradient:
                        propCopy.gradientValue = sourceCopy.gradientValue;
                        break;
                    case SerializedPropertyType.Quaternion:
                        propCopy.quaternionValue = sourceCopy.quaternionValue;
                        break;
                    case SerializedPropertyType.ExposedReference:
                        propCopy.exposedReferenceValue = sourceCopy.exposedReferenceValue;
                        break;
                    // case SerializedPropertyType.FixedBufferSize:
                    //     propCopy.fixedBufferSize = sourceCopy.fixedBufferSize;
                    //     break;
                    case SerializedPropertyType.Vector2Int:
                        propCopy.vector2IntValue = sourceCopy.vector2IntValue;
                        break;
                    case SerializedPropertyType.Vector3Int:
                        propCopy.vector3IntValue = sourceCopy.vector3IntValue;
                        break;
                    case SerializedPropertyType.RectInt:
                        propCopy.rectIntValue = sourceCopy.rectIntValue;
                        break;
                    case SerializedPropertyType.BoundsInt:
                        propCopy.boundsIntValue = sourceCopy.boundsIntValue;
                        break;
                    // case SerializedPropertyType.ManagedReference:
                    //     propCopy.managedReferenceValue = sourceCopy.managedReferenceValue;
                    //     break;
                    case SerializedPropertyType.Hash128:
                        propCopy.hash128Value = sourceCopy.hash128Value;
                        break;
                }
            }
        }
    }
}