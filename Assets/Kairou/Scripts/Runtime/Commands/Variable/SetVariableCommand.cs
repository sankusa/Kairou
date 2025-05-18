using UnityEngine;

namespace Kairou
{
    [CommandInfo("Variable", "SetVariable")]
    public partial class SetVariableCommand : Command
    {
        [GenerateValidation]
        [SerializeReference] VariableSetter _variableSetter = new VariableSetter<int>();

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            _variableSetter.Set(process);
        }

        override public string GetSummary() => _variableSetter.GetSummary();
    }
}