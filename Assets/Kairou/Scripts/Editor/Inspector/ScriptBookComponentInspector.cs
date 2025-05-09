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
            if (GUILayout.Button("Convert To Asset"))
            {
                ScriptBookOwnerConverter.ConvertAndSaveAsAsset(component.ScriptBook);
            }
            if (GUILayout.Button("Open ScriptBookEditor"))
            {
                ScriptBookEditor.Open(component, component.ScriptBookPropertyPath);
            }
        }
    }
}