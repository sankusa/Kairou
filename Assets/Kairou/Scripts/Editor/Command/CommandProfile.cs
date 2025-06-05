using System;
using System.Reflection;
using UnityEngine;

namespace Kairou.Editor
{
    public class CommandProfile
    {
        readonly Type _type;
        readonly CommandInfoAttribute _commandInfo;
        readonly CommandSetting _setting;
        readonly CommandCategory _category;
        
        public CommandProfile(Type type, CommandSetting setting, CommandCategory category)
        {
            _type = type;
            _commandInfo = _type.GetCustomAttribute<CommandInfoAttribute>();
            _setting = setting;
            _category = category;
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

        public Color NameColor
        {
            get
            {
                if (_category != null) return _category.CommandNameColor;
                return Color.white;
            }
        }

        public (Texture2D icon, Color color) Icon
        {
            get
            {
                Texture2D icon = null;
                Color color = Color.white;
                if (_setting != null && _setting.Icon != null)
                {
                    icon = _setting.Icon;
                    if (_setting.IconColor != Color.clear)
                    {
                        color = _setting.IconColor;
                    }
                    else
                    {
                        color = _category.DefaultCommandIconColor;
                    }
                }
                else if (_category != null && _category.DefaultCommandIcon != null)
                {
                    icon = _category.DefaultCommandIcon;
                    color = _category.DefaultCommandIconColor;
                }
                return (icon, color);
            }
        }

        public Color SummaryBackgoundColor
        {
            get
            {
                if (_category != null)
                {
                    return _category.SummaryBackgroundColor;
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
    }
}