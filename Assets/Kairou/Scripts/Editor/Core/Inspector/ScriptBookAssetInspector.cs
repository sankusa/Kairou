using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    [CustomEditor(typeof(ScriptBookAsset))]
    public class ScriptBookAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open ScriptBookEditor"))
            {
                ScriptBookEditor.Open(target as ScriptBookAsset);
            }
        }
    }
}