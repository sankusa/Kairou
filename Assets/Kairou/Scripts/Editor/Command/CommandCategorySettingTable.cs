using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        CommandCategorySettingTable[] _tables;
        ReadOnlyCollection<CommandCategorySettingTable> _readOnlyTables;
        public ReadOnlyCollection<CommandCategorySettingTable> Tables => _readOnlyTables;

        public IEnumerable<string> CategoryNames => _tables.SelectMany(x => x.Settings.Keys).Distinct();

        public void Reload()
        {
            _tables = AssetUtil.LoadAllAssets<CommandCategorySettingTable>();
            Array.Sort(_tables, (a, b) => b.Priority.CompareTo(a.Priority));
            _readOnlyTables = Array.AsReadOnly(_tables);
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
            foreach (var table in _tables.Reverse())
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