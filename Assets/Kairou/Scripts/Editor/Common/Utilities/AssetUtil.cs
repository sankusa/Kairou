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
        public static List<T> LoadAllAssets<T>() where T : Object {
            List<T> list = new List<T>(); 
            
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            foreach(string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
                list.Add(asset);
            }
            return list;
        }
    }
}