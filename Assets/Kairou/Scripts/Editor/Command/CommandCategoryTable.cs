using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Kairou.Editor
{
    [CreateAssetMenu(fileName = nameof(CommandCategoryTable), menuName = nameof(Kairou) + "/" + nameof(CommandCategoryTable))]
    public class CommandCategoryTable : ScriptableObject
    {
        [SerializeField] int _priority = 0;
        public int Priority => _priority;
        [SerializeField] ReadonlySerializableDictionary<string, CommandCategory> _categories = new(static c => c.Name);
        public ReadonlySerializableDictionary<string, CommandCategory> Categories => _categories;
    }

    public class CommandCategoryTableSet
    {
        CommandCategoryTable[] _tables;
        ReadOnlyCollection<CommandCategoryTable> _readOnlyTables;
        public ReadOnlyCollection<CommandCategoryTable> Tables => _readOnlyTables;

        public IEnumerable<string> CategoryNames => _tables.SelectMany(x => x.Categories.Keys).Distinct();

        public void Reload()
        {
            _tables = AssetUtil.LoadAllAssets<CommandCategoryTable>();
            Array.Sort(_tables, (a, b) => b.Priority.CompareTo(a.Priority));
            _readOnlyTables = Array.AsReadOnly(_tables);
        }

        public CommandCategory Find(string name)
        {
            foreach (var table in _tables)
            {
                if (table.Categories.TryGetValue(name, out var category))
                {
                    return category;
                }
            }
            return null;
        }
    }
}