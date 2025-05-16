namespace Kairou
{
    [CommandInfo("Flow", "EndIf")]
    public partial class EndIfCommand : Command, IBlockEnd
    {
        public string BlockCategory => "If";

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            if(process.PeekBlock() is IfBlock ifBlock)
            {
                process.PopBlock();
                ifBlock.Dispose();
            }
        }
    }
}