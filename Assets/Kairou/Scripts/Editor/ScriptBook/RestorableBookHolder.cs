using System;
using Kairou.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    [Serializable]
    public class RestorableBookHolder
    {
        [SerializeField] BookHolder _holder = new();
        [SerializeField] GlobalObjectId _globalObjectId;

        public Object Owner => _holder.Owner;
        public string BookPropertyPath => _holder.BookPropertyPath;
        public ScriptBook Book => _holder.Book;

        public BookId BookId => new(Owner, BookPropertyPath);

        public bool HasValidBook => _holder.HasValidBook;

        public void Reset(BookId bookId) => Reset(bookId.Object, bookId.BookPropertyPath);

        public void Reset(Object obj, string bookPropertyPath)
        {
            _holder.Reset(obj, bookPropertyPath);
            _globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(obj);
        }

        public bool RestoreObjectIfNull()
        {
            if (_holder.Owner == null)
            {
                _holder.Reset(GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_globalObjectId));
                return _holder.Owner != null;
            }
            return false;
        }
    }
}