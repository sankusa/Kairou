using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    [CustomEditor(typeof(ScriptBookAsset))]
    public class ScriptBookAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var asset = (ScriptBookAsset)target;
            serializedObject.UpdateIfRequiredOrScript();
            var bookProp = serializedObject.FindProperty("_book");
            var idProp = bookProp.FindPropertyRelative("_id");
            EditorGUILayout.PropertyField(idProp);
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Open ScriptBookEditor"))
            {
                ScriptBookEditor.Open(asset, asset.BookPropertyPath);
            }
        }
    }
}