
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Flow", "Label")]
    public partial class LabelCommand : Command, ILabel
    {
        [SerializeField] string _label;
        public string Label => _label;

        [CommandExecute]
        void Execute() {}

        public override string GetSummary() => _label;
    }
}