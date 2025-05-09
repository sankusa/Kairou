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
            if (GUILayout.Button("Open ScriptBookEditor"))
            {
                ScriptBookEditor.Open(asset, asset.ScriptBookPropertyPath);
            }
        }
    }
}