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

        [SerializeField] Color _commandNameColor = new(0.8f, 0.8f, 0.8f, 1);
        public Color CommandNameColor => _commandNameColor;

        [SerializeField] Color _summaryBackgroundColor = new(1, 1, 1, 1);
        public Color SummaryBackgroundColor => _summaryBackgroundColor;
    }
}