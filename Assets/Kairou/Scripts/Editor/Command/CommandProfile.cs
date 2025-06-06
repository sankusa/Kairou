using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public readonly struct CommandProfile
    {
        readonly Type _type;
        readonly CommandInfoAttribute _commandInfo;
        readonly CommandSetting _setting;
        readonly CommandCategorySetting _category;
        
        public CommandProfile(Type type, CommandSettingTableSet settingTableSet, CommandCategorySettingTableSet categoryTableSet)
        {
            _type = type;
            _setting = settingTableSet.Find(type);
            _commandInfo = CommandTypeCache.FindCommandInfo(type);
            _category = categoryTableSet.Find(GetCategoryName(_setting, _commandInfo));
        }

        public string Name
        {
            get
            {
                if (_setting != null) return _setting.Name;
                if (_commandInfo != null) return _commandInfo.CommandName;
                return _type.Name;
            }
        }

        public Color LabelColor
        {
            get
            {
                if (_category != null) return _category.LabelColor;
                return GUICommon.DefaultLabelColor;
            }
        }

        static string GetCategoryName(CommandSetting setting, CommandInfoAttribute commandInfo)
        {
            if (setting != null) return setting.Category;
            if (commandInfo != null) return commandInfo.CategoryName;
            return null;
        }

        public string CategoryName
        {
            get
            {
                return GetCategoryName(_setting, _commandInfo);
            }
        }

        public bool IsCategorized
        {
            get
            {
                return _setting != null || _commandInfo?.CategoryName != null;
            }
        }

        public Texture2D Icon
        {
            get
            {
                if (_setting != null && _setting.Icon != null) return _setting.Icon;
                if (_category != null && _category.DefaultCommandIcon != null) return _category.DefaultCommandIcon;
                return null;
            }
        }

        public Color IconColor
        {
            get
            {
                if (_setting != null && _setting.Icon != null && _setting.IconColor != Color.clear)
                {
                    return _setting.IconColor;
                }
                if (_category != null) return _category.DefaultCommandIconColor;
                return Color.white;
            }
        }

        public Color BackgoundColor
        {
            get
            {
                if (_category != null)
                {
                    return _category.BackgroundColor;
                }
                return Color.clear;
            }
        }

        public SummaryPositionType SummaryPositionType
        {
            get
            {
                if (_setting == null) return SummaryPositionType.Right;
                return _setting.SummaryPosition;
            }
        }

        public MonoScript Script => _setting?.Script;
    }
}