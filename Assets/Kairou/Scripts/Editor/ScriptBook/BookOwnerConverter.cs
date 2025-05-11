using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace Kairou
{
    public class BookOwnerConverter
    {
        public static void ConvertAndSaveAsAsset(ScriptBook book)
        {
            AssetUtil.SaveAsset(ToAsset(book));
        }

        public static ScriptBookAsset ToAsset(ScriptBook source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            var asset = ScriptableObject.CreateInstance<ScriptBookAsset>();
            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(source), asset.Book);
            return asset;
        }
    }
}