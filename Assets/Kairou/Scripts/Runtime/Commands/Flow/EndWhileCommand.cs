namespace Kairou
{
    public partial class EndWhileCommand : Command, IBlockEnd
    {
        public string BlockCategory => "While";
        public bool IsLoopBlock => true;

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            int startIndex = ParentPage.FindBlockStartIndex(this);
            if (startIndex == -1) return;
            process.GoToIndex(startIndex);
        }
    }
}