using System;
using UnityEngine;

namespace Kairou.Editor
{
    [Serializable]
    public class CommandCategorySetting
    {
        [SerializeField] string _name;
        public string Name => _name;

        [SerializeField] int _priority;
        public int Priority => _priority;

        [SerializeField] Texture2D _defaultCommandIcon;
        public Texture2D DefaultCommandIcon => _defaultCommandIcon;

        [SerializeField] Color _defaultCommandIconColor = Color.clear;
        public Color DefaultCommandIconColor => _defaultCommandIconColor;

        [SerializeField] Color _labelColor = GUICommon.DefaultLabelColor;
        public Color LabelColor => _labelColor;

        [SerializeField] Color _backgroundColor = new(1, 1, 1, 1);
        public Color BackgroundColor => _backgroundColor;
    }
}