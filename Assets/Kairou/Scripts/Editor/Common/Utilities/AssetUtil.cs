using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

        /// <summary>
        /// プロジェクト内の対象型のアセットを全てロード
        /// </summary>
        public static List<T> LoadAllAssets<T>(Func<T, bool> predicate = null) where T : Object
        {   
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            var list = new List<T>();
            foreach(string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset == null) continue;
                if (predicate != null && !predicate(asset)) continue;
                list.Add(asset);
            }
            return list;
        }
    }
}