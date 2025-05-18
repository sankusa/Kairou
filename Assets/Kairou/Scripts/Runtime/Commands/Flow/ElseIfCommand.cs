using UnityEngine;

namespace Kairou
{
    [CommandInfo("Flow", "ElseIf")]
    public partial class ElseIfCommand : Command, IBlockEnd, IBlockStart
    {
        public string BlockCategory => "If";
        public bool IsLoopBlock => false;

        [GenerateValidation]
        [SerializeReference] Condition _condition = new Condition<int>();

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            if (process.TryPopBlock<IfBlock>(out var ifBlock))
            {
                ifBlock.SetStartAndEnd(this);
                process.PushBlock(ifBlock);
                if (ifBlock.EvaluationFinished)
                {
                    process.GoToIndex(ifBlock.EndIndex);
                }
                // IfCommandと同じ動作
                else
                {
                    bool result = _condition.Evaluate(process);
                    ifBlock.EvaluationFinished = result;
                    if (result == false)
                    {
                        process.GoToIndex(ifBlock.EndIndex);
                    }
                }
            }
            // IfBlockがスタックに積まれていない場合、EvaluationFinished = trueだった場合と同じ状態になるようにする
            else
            {
                ifBlock = IfBlock.Rent();
                ifBlock.SetStartAndEnd(this);
                process.PushBlock(ifBlock);
                ifBlock.EvaluationFinished = true;
                process.GoToIndex(ifBlock.EndIndex);
            }
        }

        public override string GetSummary()
        {
            return _condition.GetSummary();
        }
    }
}