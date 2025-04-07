namespace Kairou
{
    public static class ScriptBookExtensions
    {
        public static bool ExistsCommandAt(this ScriptBook scriptBook, int pageIndex, int commandIndex)
        {
            return scriptBook.Pages.HasElementAt(pageIndex) && scriptBook.Pages[pageIndex].Commands.HasElementAt(commandIndex);
        }
    }
}