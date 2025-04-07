using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    [CustomEditor(typeof(ScriptBookComponent))]
    public class ScriptBookComponentInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open ScriptBookEditor"))
            {
                ScriptBookEditor.Open((ScriptBookComponent)target);
            }
        }
    }
}