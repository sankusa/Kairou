using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Debug", "Log")]
    public partial class LogCommand : Command
    {
        [SerializeField, TextArea] string _format;
        [SerializeField] List<VariableValueGetterKey<object>> _args;

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            Debug.Log(
                $"PageIndex = {ParentPage.Index}, CommandIndex = {Index}\n"
                + string.Format(
                    _format,
                    _args.Select(x => 
                    {
                        var getter = x.Find(process);
                        return getter.HasVariable ? getter.GetValue()?.ToString() : "\"variable not found\"";
                    })
                    .ToArray()
                )
            );
        }

        public override string GetSummary()
        {
            return _format;
        }
    }
}