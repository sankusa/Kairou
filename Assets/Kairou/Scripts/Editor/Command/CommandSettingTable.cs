using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kairou.Editor
{
    [CreateAssetMenu(fileName = nameof(CommandSettingTable), menuName = nameof(Kairou) + "/" + nameof(CommandSettingTable))]
    public class CommandSettingTable : ScriptableObject
    {
        [SerializeField] int _priority = 0;
        public int Priority => _priority;
        [SerializeField] ReadonlySerializableDictionary<Type, CommandSetting> _settings = new(static setting => setting.Type);
        public IReadOnlyDictionary<Type, CommandSetting> Settings => _settings;
    }

    public class CommandSettingTableSet
    {
        class CommandSettingTableSetPostProcessor : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (string path in importedAssets)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(CommandSettingTable))
                    {
                        CommandSettingTableSet.OnTableCreated(path);
                    }
                }

                if (deletedAssets.Length > 0) CommandSettingTableSet.OnAnyAssetDeleted();
            }
        }

        static readonly WeakReference<CommandSettingTableSet> _instance = new(null);

        public static CommandSettingTableSet Load()
        {
            if (!_instance.TryGetTarget(out var instance))
            {
                instance = new CommandSettingTableSet();
                instance.Reload();
                _instance.SetTarget(instance);
            }
            return instance;
        }

        CommandSettingTableSet() {}

        static void OnTableCreated(string path)
        {
            if (_instance.TryGetTarget(out var instance))
            {
                if (instance._tables.Any(table => AssetDatabase.GetAssetPath(table) == path)) return;
                var table = AssetDatabase.LoadAssetAtPath<CommandSettingTable>(path);
                if (table == null) return;
                instance._tables.Add(table);
                instance.SortTables();
            }
        }

        static void OnAnyAssetDeleted()
        {
            if (_instance.TryGetTarget(out var instance))
            {
                for (int i = instance._tables.Count - 1; i >= 0; i--)
                {
                    if (instance._tables[i] == null)
                    {
                        instance._tables.RemoveAt(i);
                    }
                }
            }
        }

        List<CommandSettingTable> _tables;
        public IReadOnlyList<CommandSettingTable> Tables => _tables;

        void Reload()
        {
            _tables = AssetUtil.LoadAllAssets<CommandSettingTable>();
            SortTables();
        }

        void SortTables()
        {
            _tables.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public CommandSetting Find(Type type)
        {
            foreach (var table in _tables)
            {
                if (table.Settings.TryGetValue(type, out var setting))
                {
                    return setting;
                }
            }
            return null;
        }
    }
}