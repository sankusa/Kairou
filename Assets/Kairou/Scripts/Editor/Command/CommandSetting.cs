using System;
using UnityEngine;

namespace Kairou.Editor
{
    public enum SummaryPositionType
    {
        Right,
        Bottom,
    }

    [Serializable]
    public class CommandSetting
    {
        [SerializeField] string _typeFullName;
        Type _type;
        public Type Type
        {
            get
            {
                if (_type == null || _type.FullName != _typeFullName)
                {
                    _type = CommandTypeCache.Find(_typeFullName);
                }
                return _type;
            }
        }

        [SerializeField] string _category;
        public string Category => _category;

        [SerializeField] Texture2D _icon;
        public Texture2D Icon => _icon;

        [SerializeField] Color _iconColor;
        public Color IconColor => _iconColor;

        [SerializeField] SummaryPositionType _summaryPosition = SummaryPositionType.Right;
        public SummaryPositionType SummaryPosition => _summaryPosition;
    }
}