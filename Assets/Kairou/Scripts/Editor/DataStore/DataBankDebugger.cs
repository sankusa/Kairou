using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Kairou.DataStore.Editor
{
    public class DataBankDebugger : EditorWindow
    {
        [MenuItem("Window/" + nameof(Kairou) + "/" + nameof(DataBankDebugger))]
        static void Open()
        {
            GetWindow<DataBankDebugger>();
        }

        [Serializable]
        class StorageStatus
        {
            [SerializeField] string _storageKey;
            public string StorageKey
            {
                get => _storageKey;
                set => _storageKey = value;
            }
            [SerializeField] bool _isExpanded;
            public bool IsExpanded
            {
                get => _isExpanded;
                set => _isExpanded = value;
            }

            public StorageStatus(string storageKey)
            {
                _storageKey = storageKey;
                _isExpanded = false;
            }
        }

        [SerializeField] List<StorageStatus> _statuses = new();
        [SerializeField] List<StorageStatus> _statusesCache = new();
        Vector2 _scrollPos;

        void OnGUI()
        {
            if (EditorApplication.isPlaying == false)
            {
                EditorGUILayout.HelpBox("This window is available only in play mode.", MessageType.Info);
                return;
            }
            var newStatuses = DataBank.Storages.Select(kvp =>
            {
                var status = _statuses.Find(k => k.StorageKey == kvp.Key);
                if (status != null) return status;
                return _statusesCache.Find(k => k.StorageKey == kvp.Key) ?? new StorageStatus(kvp.Key);
            }).ToList();
            foreach (var status in _statuses)
            {
                if (newStatuses.Find(k => k.StorageKey == status.StorageKey) == null) _statusesCache.Add(status);
            }
            _statuses = newStatuses;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var status in _statuses)
            {
                status.IsExpanded = EditorGUILayout.Foldout(status.IsExpanded, status.StorageKey);
                if (status.IsExpanded)
                {
                    var storage = DataBank.GetStorage(status.StorageKey);
                    if (storage == DataBank.GetStorage())
                    {
                        EditorGUILayout.LabelField("Default Storage", GUI.skin.box);
                    }
                    if (storage is IDataRecordsProvider provider)
                    {
                        foreach (var record in provider.GetRecords())
                        {
                            var type = DataTypeCache.Get(record.TypeId);
                            float height = type == null ? EditorGUIUtility.singleLineHeight : type.GetFieldHeight(record.Value);
                            var rect = EditorGUILayout.GetControlRect(false, height);
                            var rects = RectUtil.SplitRect(rect, 2, true, new[] { (80, true), (2f, false)});
                            
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUI.Popup(rects[0], 0, new [] { record.TypeId.ToString() });
                            EditorGUI.EndDisabledGroup();
                            if (type != null)
                            {
                                record.Value = type.DrawField(rects[1], record.Key, record.Value);
                            }
                            else
                            {
                                record.Value = EditorGUI.TextField(rects[1], record.Key, record.Value);
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}