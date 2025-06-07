using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    [CustomEditor(typeof(ScriptBookComponent))]
    public class ScriptBookComponentInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var component = (ScriptBookComponent)target;
            serializedObject.UpdateIfRequiredOrScript();
            var bookProp = serializedObject.FindProperty("_book");
            var idProp = bookProp.FindPropertyRelative("_id");
            EditorGUILayout.PropertyField(idProp);
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Convert To Asset"))
            {
                BookOwnerConverter.ConvertAndSaveAsAsset(component.Book);
            }
            if (GUILayout.Button("Open ScriptBookEditor"))
            {
                ScriptBookEditor.Open(component, component.BookPropertyPath);
            }
        }
    }
}