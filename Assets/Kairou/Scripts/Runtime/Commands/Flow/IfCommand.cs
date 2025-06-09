using UnityEngine;

namespace Kairou
{
    public partial class IfCommand : Command, IBlockStart
    {
        public string BlockCategory => "If";
        public bool IsLoopBlock => false;

        [GenerateValidation]
        [SerializeReference] Condition _condition = new Condition<int>();

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            var ifBlock = IfBlock.Rent();
            ifBlock.SetStartAndEnd(this);
            process.PushBlock(ifBlock);

            // 評価
            bool result = _condition.Evaluate(process);
            ifBlock.EvaluationFinished = result;

            // Trueなら続行、FalseならBlockEndまで飛ぶ
            if(result == false)
            {
                process.GoToIndex(ifBlock.EndIndex);
            }
        }

        public override string GetSummary()
        {
            return _condition == null ? "null" : _condition.GetSummary();
        }
    }
}