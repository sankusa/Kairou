using System;
using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public class CommandDatabase
    {
        static readonly WeakReference<CommandDatabase> _instance = new(null);

        static CommandDatabase()
        {
            EditorApplication.projectChanged += ReloadDatabase;
        }

        public static CommandDatabase Load()
        {
            if (!_instance.TryGetTarget(out var instance))
            {
                instance = new CommandDatabase();
                instance.Reload();
                _instance.SetTarget(instance);
            }
            return instance;
        }

        static void ReloadDatabase()
        {
            _instance.TryGetTarget(out var instance);
            instance?.Reload();
        }

        CommandSettingTableSet _settingTableSet = new();
        CommandCategoryTableSet _categoryTableSet = new();

        void Reload()
        {
            _settingTableSet.Reload();
            _categoryTableSet.Reload();
        }

        public (CommandSetting setting, CommandCategory category) FindSetting(Type type)
        {
            var setting = _settingTableSet.Find(type);
            var category = _categoryTableSet.Find(setting);
            return (setting, category);
        }
    }
}