using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    public static class ScriptBookUtilForEditor
    {
        public static void AddSetUpPage(Object scriptBookOwner, ScriptBook scriptBook)
        {
            if (scriptBookOwner == null) throw new ArgumentNullException(nameof(scriptBookOwner));
            if (scriptBook == null) throw new ArgumentNullException(nameof(scriptBook));

            Undo.RecordObject(scriptBookOwner, "Add Page");
            string pageId = ObjectNames.GetUniqueName(
                scriptBook.Pages.Select(x => x.Id).ToArray(),
                "Page"
            );
            scriptBook.AddPage(new Page() { Id = pageId });
            EditorUtility.SetDirty(scriptBookOwner);
        }

        public static void RemovePage(Object scriptBookOwner, ScriptBook scriptBook, Page page)
        {
            if (scriptBookOwner == null) throw new ArgumentNullException(nameof(scriptBookOwner));
            if (scriptBook == null) throw new ArgumentNullException(nameof(scriptBook));

            Undo.RecordObject(scriptBookOwner, "Remove Page");
            scriptBook.RemovePage(page);
            EditorUtility.SetDirty(scriptBookOwner);
        }

        public static void MovePage(Object scriptBookOwner, ScriptBook scriptBook, int fromIndex, int toIndex)
        {
            if (scriptBookOwner == null) throw new ArgumentNullException(nameof(scriptBookOwner));
            if (scriptBook == null) throw new ArgumentNullException(nameof(scriptBook));

            Undo.RecordObject(scriptBookOwner, "Move Page");
            scriptBook.MovePage(fromIndex, toIndex);
            EditorUtility.SetDirty(scriptBookOwner);
        }

        public static void AddCommand(Object scriptBookOwner, ScriptBook scriptBook, int pageIndex, Command command)
        {
            if (scriptBookOwner == null) throw new ArgumentNullException(nameof(scriptBookOwner));
            if (scriptBook == null) throw new ArgumentNullException(nameof(scriptBook));
            
            Undo.RecordObject(scriptBookOwner, "Add Command");
            scriptBook.Pages[pageIndex].AddCommand(command);
            EditorUtility.SetDirty(scriptBookOwner);
        }

        public static void RemoveCommand(Object scriptBookOwner, ScriptBook scriptBook, int pageIndex, int commandIndex)
        {
            if (scriptBookOwner == null) throw new ArgumentNullException(nameof(scriptBookOwner));
            if (scriptBook == null) throw new ArgumentNullException(nameof(scriptBook));
            
            Undo.RecordObject(scriptBookOwner, "Remove Command");
            scriptBook.Pages[pageIndex].RemoveCommandAt(commandIndex);
            EditorUtility.SetDirty(scriptBookOwner);
        }

        public static void MoveCommand(Object scriptBookOwner, ScriptBook scriptBook, int pageIndex, int fromIndex, int toIndex)
        {
            if (scriptBookOwner == null) throw new ArgumentNullException(nameof(scriptBookOwner));
            if (scriptBook == null) throw new ArgumentNullException(nameof(scriptBook));
            
            Undo.RecordObject(scriptBookOwner, "Move Command");
            scriptBook.Pages[pageIndex].MoveCommand(fromIndex, toIndex);
            EditorUtility.SetDirty(scriptBookOwner);
        }
    }
}