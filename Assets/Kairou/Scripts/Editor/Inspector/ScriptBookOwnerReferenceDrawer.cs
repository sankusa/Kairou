using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace Kairou
{
    [CustomPropertyDrawer(typeof(ScriptBookOwnerReference))]
    public class ScriptBookOwnerReferenceDrawer : PropertyDrawer
    {
        static readonly SearchContext _searchContext = SearchService.CreateContext("t=" + nameof(ScriptBookComponent) + " or t=" + nameof(ScriptBookAsset));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty scriptBookOwnerProp = property.FindPropertyRelative("_scriptBookOwner");
            Object obj = ObjectField.DoObjectField(position, scriptBookOwnerProp.objectReferenceValue, typeof(Object), label, _searchContext, UnityEngine.Search.SearchViewFlags.CompactView);
            // UnitySearchではシーン上のコンポーネントを直接取得できず、代わりにコンポーネントを持つGameObjectを取得する。
            if (obj is GameObject)
            {
                obj = (obj as GameObject).GetComponent<ScriptBookComponent>();
            }
            if (obj is IScriptBookOwner && obj != scriptBookOwnerProp.objectReferenceValue)
            {
                scriptBookOwnerProp.objectReferenceValue = obj;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}