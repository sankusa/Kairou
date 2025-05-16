using UnityEngine;

namespace Kairou
{
    [CommandInfo("Flow", "While")]
    public partial class WhileCommand : Command, IBlockStart
    {
        public string BlockCategory => "While";

        [GenerateValidation]
        [SerializeReference] Condition _condition = new Condition<int>();

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            WhileBlock whileBlock;
            if (process.PeekBlock() is WhileBlock block && block.Start == this)
            {
                whileBlock = block;
            }
            else
            {
                whileBlock = WhileBlock.Rent();
                whileBlock.SetStartAndEnd(this);
                process.PushBlock(whileBlock);
            }

            bool result = _condition.Evaluate(process);

            if (result == false)
            {
                process.GoToIndex(whileBlock.EndIndex + 1);
            }
        }
    }
}