namespace Kairou
{
    public partial class ExitCommand : Command
    {
        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            process.GoToEnd();
        }
    }
}