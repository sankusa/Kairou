using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

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
        CommandSettingTable[] _tables;
        ReadOnlyCollection<CommandSettingTable> _readOnlyTables;
        public ReadOnlyCollection<CommandSettingTable> Tables => _readOnlyTables;

        public void Reload()
        {
            _tables = AssetUtil.LoadAllAssets<CommandSettingTable>();
            Array.Sort(_tables, (a, b) => b.Priority.CompareTo(a.Priority));
            _readOnlyTables = Array.AsReadOnly(_tables);
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