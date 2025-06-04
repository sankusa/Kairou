using System;
using UnityEngine;

namespace Kairou.Editor
{
    [Serializable]
    public class CommandCategory
    {
        [SerializeField] string _name;
        public string Name => _name;

        [SerializeField] Color _summaryBackgroundColor = new(1, 1, 1, 1);
        public Color SummaryBackgroundColor => _summaryBackgroundColor;
    }
}