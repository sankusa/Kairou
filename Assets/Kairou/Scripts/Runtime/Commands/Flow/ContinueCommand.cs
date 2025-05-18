namespace Kairou
{
    [CommandInfo("Flow", "Continue")]
    public partial class ContinueCommand : Command
    {
        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            for (int i = Index - 1; i >= 0; i--)
            {
                if (ParentPage.Commands[i] is IBlockStart blockStart && blockStart.IsLoopBlock == true)
                {
                    process.GoToIndex(i);
                    return;
                }
            }
        }
    }
}