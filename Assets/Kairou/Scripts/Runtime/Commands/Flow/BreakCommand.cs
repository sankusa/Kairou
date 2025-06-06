namespace Kairou
{
    public partial class BreakCommand : Command
    {
        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            for (int i = Index + 1; i < ParentPage.Commands.Count; i++)
            {
                if (ParentPage.Commands[i] is IBlockEnd blockEnd && blockEnd.IsLoopBlock == true)
                {
                    process.GoToIndex(i + 1);
                    return;
                }
            }
        }
    }
}