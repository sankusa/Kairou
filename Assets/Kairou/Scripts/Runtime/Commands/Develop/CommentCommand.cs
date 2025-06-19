using UnityEngine;

namespace Kairou
{
    public partial class CommentCommand : Command
    {
        [SerializeField, TextArea] string _comment;
        [CommandExecute] void Execute() {}
        public override string GetSummary()
        {
            return _comment;
        }
    }
}