namespace Kairou
{
    public partial class EndIfCommand : Command, IBlockEnd
    {
        public string BlockCategory => "If";
        public bool IsLoopBlock => false;

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