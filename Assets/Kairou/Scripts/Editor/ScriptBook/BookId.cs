using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    public readonly struct BookId
    {
        public Object Object { get; }
        public string BookPropertyPath { get; }

        public BookId(Object obj, string bookPropertyPath)
        {
            Object = obj;
            BookPropertyPath = bookPropertyPath;
        }
    }
}