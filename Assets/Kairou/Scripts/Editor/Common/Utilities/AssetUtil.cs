using System;
using UnityEditor;
using UnityEngine;

namespace Kairou
{
    public class AssetUtil
    {
        public static void SaveAsset<T>(T asset) where T : ScriptableObject
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            
            // デフォルトの保存先とファイル名を指定
            string defaultName = typeof(T).Name;
            string path = EditorUtility.SaveFilePanelInProject(
                title: "Save Asset",
                defaultName: defaultName,
                extension: "asset",
                message: "Select a location and file name to save the asset."
            );

            if (string.IsNullOrEmpty(path))
                return;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}