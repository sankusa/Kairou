using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace Kairou
{
    public class ScriptBookOwnerConverter
    {
        public static void ConvertAndSaveAsAsset(ScriptBook scriptBook)
        {
            AssetUtil.SaveAsset(ToAsset(scriptBook));
        }

        public static ScriptBookAsset ToAsset(ScriptBook source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            var asset = ScriptableObject.CreateInstance<ScriptBookAsset>();
            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(source), asset.ScriptBook);
            return asset;
        }
    }
}