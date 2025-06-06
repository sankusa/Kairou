using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    [CreateAssetMenu(fileName = nameof(CommandCategorySettingTable), menuName = nameof(Kairou) + "/" + nameof(CommandCategorySettingTable))]
    public class CommandCategorySettingTable : ScriptableObject
    {
        [SerializeField] int _priority = 0;
        public int Priority => _priority;
        [SerializeField] ReadonlySerializableDictionary<string, CommandCategorySetting> _settings = new(static c => c.Name);
        public ReadonlySerializableDictionary<string, CommandCategorySetting> Settings => _settings;
    }

    public class CommandCategorySettingTableSet
    {
        class CommandCategorySettingTableSetPostProcessor : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (string path in importedAssets)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(CommandCategorySettingTable))
                    {
                        CommandCategorySettingTableSet.OnTableCreated(path);
                    }
                }

                if (deletedAssets.Length > 0) CommandCategorySettingTableSet.OnAnyAssetDeleted();
            }
        }

        static readonly WeakReference<CommandCategorySettingTableSet> _instance = new(null);

        public static CommandCategorySettingTableSet Load()
        {
            if (!_instance.TryGetTarget(out var instance))
            {
                instance = new CommandCategorySettingTableSet();
                instance.Reload();
                _instance.SetTarget(instance);
            }
            return instance;
        }

        CommandCategorySettingTableSet() {}

        static void OnTableCreated(string path)
        {
            if (_instance.TryGetTarget(out var instance))
            {
                if (instance._tables.Any(table => AssetDatabase.GetAssetPath(table) == path)) return;
                var table = AssetDatabase.LoadAssetAtPath<CommandCategorySettingTable>(path);
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

        List<CommandCategorySettingTable> _tables;
        public IReadOnlyList<CommandCategorySettingTable> Tables => _tables;

        public IEnumerable<string> CategoryNames => _tables.SelectMany(x => x.Settings.Keys).Distinct();

        void Reload()
        {
            _tables = AssetUtil.LoadAllAssets<CommandCategorySettingTable>();
            SortTables();
        }

        void SortTables()
        {
            _tables.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }   

        public CommandCategorySetting Find(string name)
        {
            if (name == null) return null;

            foreach (var table in _tables)
            {
                if (table.Settings.TryGetValue(name, out var category))
                {
                    return category;
                }
            }
            return null;
        }

        public Dictionary<string, CommandCategorySetting> GetCategories()
        {
            Dictionary<string, CommandCategorySetting> categories = new();
            foreach (var table in _tables.AsEnumerable().Reverse())
            {
                foreach (var category in table.Settings.Values)
                {
                    categories[category.Name] = category;
                }
            }
            return categories;
        }
    }
}