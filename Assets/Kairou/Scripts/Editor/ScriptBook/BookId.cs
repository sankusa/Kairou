using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    public readonly struct BookId : IEquatable<BookId>
    {
        public Object Object { get; }
        public string BookPropertyPath { get; }

        public BookId(Object obj, string bookPropertyPath)
        {
            Object = obj;
            BookPropertyPath = bookPropertyPath;
        }

        public bool Equals(BookId other)
        {
            return Object == other.Object && BookPropertyPath == other.BookPropertyPath;
        }
        public override bool Equals(object obj) => obj is BookId other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Object, BookPropertyPath);
        public static bool operator ==(BookId left, BookId right) => left.Equals(right);
        public static bool operator !=(BookId left, BookId right) => !left.Equals(right);
    }
}