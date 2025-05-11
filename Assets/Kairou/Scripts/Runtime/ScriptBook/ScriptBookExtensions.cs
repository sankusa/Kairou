namespace Kairou
{
    public static class ScriptBookExtensions
    {
        public static bool ExistsCommandAt(this ScriptBook book, int pageIndex, int commandIndex)
        {
            return book.Pages.HasElementAt(pageIndex) && book.Pages[pageIndex].Commands.HasElementAt(commandIndex);
        }
    }
}