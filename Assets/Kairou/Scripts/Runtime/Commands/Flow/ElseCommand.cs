namespace Kairou
{
    [CommandInfo("Flow", "Else")]
    public partial class ElseCommand : Command, IBlockEnd, IBlockStart
    {
        public string BlockCategory => "If";

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
                else
                {
                    ifBlock.EvaluationFinished = true;
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
    }
}