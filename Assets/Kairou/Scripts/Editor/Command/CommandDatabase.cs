using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public class CommandDatabase
    {
        static readonly WeakReference<CommandDatabase> _instance = new(null);

        public static CommandDatabase Load()
        {
            if (!_instance.TryGetTarget(out var instance))
            {
                instance = new CommandDatabase();
                _instance.SetTarget(instance);
            }
            return instance;
        }

        readonly CommandSettingTableSet _commandSettingTableSet = CommandSettingTableSet.Load();
        readonly CommandCategorySettingTableSet _categorySettingTableSet = CommandCategorySettingTableSet.Load();

        CommandDatabase() {}

        public CommandProfile GetProfile(Type type)
        {
            return new CommandProfile(type, _commandSettingTableSet, _categorySettingTableSet);
        }

        public CommandCategorySetting FindCategorySetting(string categoryName)
        {
            return _categorySettingTableSet.Find(categoryName);
        }

        public IEnumerable<Type> FindCommandsByCategoryName(string categoryName)
        {
            foreach (var type in CommandTypeCache.Types)
            {
                if (GetProfile(type).CategoryName == categoryName) yield return type;
            }
        }

        public IEnumerable<(string categoryName, int priority)> GetCategories()
        {
            return CommandTypeCache.Types
                .Select(x =>
                {
                    var categoryName = GetProfile(x).CategoryName;
                    if (categoryName == null) return (null, 0);
                    var categorySetting = _categorySettingTableSet.Find(categoryName);
                    if (categorySetting == null) return (categoryName, 0);
                    return (categoryName, categorySetting.Priority);
                })
                .Distinct()
                .Where(x => x.categoryName != null)
                .OrderByDescending(x => x.Item2)
                .ThenBy(x => x.categoryName);
        }

        public IEnumerable<string> GetOutOfTableCategoryNames()
        {
            return GetCategories().Select(x => x.categoryName).Except(_categorySettingTableSet.GetCategories().Keys);
        }

        public IEnumerable<Type> UncategorizedCommands()
        {
            foreach (var type in CommandTypeCache.Types)
            {
                if (GetProfile(type).IsCategorized == false) yield return type;
            }
        }
    }
}