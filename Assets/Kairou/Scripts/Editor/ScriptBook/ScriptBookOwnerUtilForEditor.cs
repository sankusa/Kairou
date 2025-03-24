using System.Linq;
using UnityEditor;

namespace Kairou.Editor
{
    public static class ScriptBookOwnerUtilForEditor
    {
        public static void AddSetUpPage(IScriptBookOwner scriptBookOwner)
        {
            scriptBookOwner.ChangeWithUndo("Add Page", owner =>
            {
                string pageName = ObjectNames.GetUniqueName(
                    owner.ScriptBook.Pages.Select(x => x.Name).ToArray(),
                    "Page"
                );
                owner.ScriptBook.AddPage(new Page() { Name = pageName });
            });
        }

        public static void RemovePage(IScriptBookOwner scriptBookOwner, Page page)
        {
            scriptBookOwner.ChangeWithUndo("Remove Page", owner =>
            {
                owner.ScriptBook.RemovePage(page);
            });
        }

        public static void MovePage(IScriptBookOwner scriptBookOwner, int fromIndex, int toIndex)
        {
            scriptBookOwner.ChangeWithUndo("Move Page", owner =>
            {
                owner.ScriptBook.MovePage(fromIndex, toIndex);
            });
        }

        public static void AddCommand(IScriptBookOwner scriptBookOwner, int pageIndex, Command command)
        {
            scriptBookOwner.ChangeWithUndo("Add Command", owner =>
            {
                owner.ScriptBook.Pages[pageIndex].AddCommand(command);
            });
        }

        public static void RemoveCommand(IScriptBookOwner scriptBookOwner, int pageIndex, int commandIndex)
        {
            scriptBookOwner.ChangeWithUndo("Remove Command", owner =>
            {
                owner.ScriptBook.Pages[pageIndex].RemoveCommandAt(commandIndex);
            });
        }

        public static void MoveCommand(IScriptBookOwner scriptBookOwner, int pageIndex, int fromIndex, int toIndex)
        {
            scriptBookOwner.ChangeWithUndo("Move Command", owner =>
            {
                owner.ScriptBook.Pages[pageIndex].MoveCommand(fromIndex, toIndex);
            });
        }
    }
}