using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    public static class BookUtilForEditor
    {
        public static void AddSetUpPage(Object bookOwner, ScriptBook book)
        {
            if (bookOwner == null) throw new ArgumentNullException(nameof(bookOwner));
            if (book == null) throw new ArgumentNullException(nameof(book));

            Undo.RecordObject(bookOwner, "Add Page");
            string pageId = ObjectNames.GetUniqueName(
                book.Pages.Select(x => x.Id).ToArray(),
                "Page"
            );
            book.AddPage(new Page() { Id = pageId });
            EditorUtility.SetDirty(bookOwner);
        }

        public static void RemovePage(Object bookOwner, ScriptBook book, Page page)
        {
            if (bookOwner == null) throw new ArgumentNullException(nameof(bookOwner));
            if (book == null) throw new ArgumentNullException(nameof(book));

            Undo.RecordObject(bookOwner, "Remove Page");
            book.RemovePage(page);
            EditorUtility.SetDirty(bookOwner);
        }

        public static void MovePage(Object bookOwner, ScriptBook book, int fromIndex, int toIndex)
        {
            if (bookOwner == null) throw new ArgumentNullException(nameof(bookOwner));
            if (book == null) throw new ArgumentNullException(nameof(book));

            Undo.RecordObject(bookOwner, "Move Page");
            book.MovePage(fromIndex, toIndex);
            EditorUtility.SetDirty(bookOwner);
        }

        public static void AddCommand(Object bookOwner, ScriptBook book, int pageIndex, Command command)
        {
            if (bookOwner == null) throw new ArgumentNullException(nameof(bookOwner));
            if (book == null) throw new ArgumentNullException(nameof(book));
            
            Undo.RecordObject(bookOwner, "Add Command");
            book.Pages[pageIndex].AddCommand(command);
            EditorUtility.SetDirty(bookOwner);
        }

        public static void InsertCommand(Object bookOwner, ScriptBook book, int pageIndex, int insertIndex, Command command)
        {
            if (bookOwner == null) throw new ArgumentNullException(nameof(bookOwner));
            if (book == null) throw new ArgumentNullException(nameof(book));
            
            Undo.RecordObject(bookOwner, "Insert Command");
            book.Pages[pageIndex].InsertCommand(insertIndex, command);
            EditorUtility.SetDirty(bookOwner);
        }

        public static void InsertCommands(Object bookOwner, ScriptBook book, int pageIndex, int insertIndex, IEnumerable<Command> commands)
        {
            if (bookOwner == null) throw new ArgumentNullException(nameof(bookOwner));
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            
            Undo.RecordObject(bookOwner, "Insert Commands");
            book.Pages[pageIndex].InsertCommands(insertIndex, commands);
            EditorUtility.SetDirty(bookOwner);
        }

        public static void RemoveCommand(Object bookOwner, ScriptBook book, int pageIndex, int commandIndex)
        {
            if (bookOwner == null) throw new ArgumentNullException(nameof(bookOwner));
            if (book == null) throw new ArgumentNullException(nameof(book));
            
            Undo.RecordObject(bookOwner, "Remove Command");
            book.Pages[pageIndex].RemoveCommandAt(commandIndex);
            EditorUtility.SetDirty(bookOwner);
        }

        public static void RemoveCommands(Object bookOwner, ScriptBook book, int pageIndex, IEnumerable<Command> commands)
        {
            if (bookOwner == null) throw new ArgumentNullException(nameof(bookOwner));
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            
            Undo.RecordObject(bookOwner, "Remove Command");
            foreach (var command in commands)
            {
                book.Pages[pageIndex].RemoveCommand(command);
            }
            EditorUtility.SetDirty(bookOwner);
        }

        public static void MoveCommand(Object bookOwner, ScriptBook book, int pageIndex, int fromIndex, int toIndex)
        {
            if (bookOwner == null) throw new ArgumentNullException(nameof(bookOwner));
            if (book == null) throw new ArgumentNullException(nameof(book));
            
            Undo.RecordObject(bookOwner, "Move Command");
            book.Pages[pageIndex].MoveCommand(fromIndex, toIndex);
            EditorUtility.SetDirty(bookOwner);
        }
    }
}