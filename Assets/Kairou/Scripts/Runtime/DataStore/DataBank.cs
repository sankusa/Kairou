using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kairou.DataStore
{
    public static class DataBank
    {
        static IDataStorage _defaultStorage;
        static readonly Dictionary<string, IDataStorage> _storages = new();
        public static Dictionary<string, IDataStorage> Storages => _storages;

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        static void SetUp()
        {
            UnityEditor.EditorApplication.playModeStateChanged += CleanUp;
        }

        static void CleanUp(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                _defaultStorage = null;
                _storages.Clear();
                UnityEditor.EditorApplication.playModeStateChanged -= CleanUp;
            }
        }
#endif

        public static void Attach(string key, IDataStorage storage, bool setAsDefault = false)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            if (_storages.ContainsKey(key)) throw new ArgumentException($"Key '{key}' already exists.");
            
            _storages[key] = storage;
            if (_defaultStorage == null) _defaultStorage = storage;
            if (setAsDefault) _defaultStorage = storage;
        }

        public static void Detach(IDataStorage storage)
        {
            if (_defaultStorage == storage) _defaultStorage = null;
            string key = null;
            foreach (var kvp in _storages)
            {
                if (kvp.Value == storage)
                {
                    key = kvp.Key;
                    break;
                }
            }
            if (key == null) return;
            _storages.Remove(key);
        }

        public static IDataStorage GetStorage() => _defaultStorage;
        public static IDataStorage GetStorage(string storageKey) => _storages[storageKey];
    }
}