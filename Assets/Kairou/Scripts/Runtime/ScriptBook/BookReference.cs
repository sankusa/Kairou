using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    public static class BookReference
    {
        static readonly List<WeakReference<ScriptBook>> _references = new List<WeakReference<ScriptBook>>();

        public static ScriptBook Find(string bookId)
        {
            for (int i = _references.Count - 1; i >= 0; i--)
            {
                if (_references[i].TryGetTarget(out var target) == false)
                {
                    _references.RemoveAt(i);
                    continue;
                }

                if (target.Id == bookId) return target;
            }
            return null;
        }

        public static void Add(ScriptBook book)
        {
            if (book == null) return;
            if (ContainsWithCleanUp(book)) return;
            _references.Add(new WeakReference<ScriptBook>(book));
        }

        // public static void Remove(ScriptBook book)
        // {
        //     for (int i = _references.Count - 1; i >= 0; i--)
        //     {
        //         if (_references[i].TryGetTarget(out var target) == false)
        //         {
        //             _references.RemoveAt(i);
        //             continue;
        //         }

        //         if (target == book) _references.RemoveAt(i);
        //     }
        // }

        static bool ContainsWithCleanUp(ScriptBook book)
        {
            for (int i = _references.Count - 1; i >= 0; i--)
            {
                if (_references[i].TryGetTarget(out var target) == false)
                {
                    _references.RemoveAt(i);
                    continue;
                }

                if (target == book) return true;
            }
            return false;
        }

        static void CleanUp()
        {
            for (int i = _references.Count - 1; i >= 0; i--)
            {
                if (_references[i].TryGetTarget(out var target) == false) _references.RemoveAt(i);
            }
        }
    }
}