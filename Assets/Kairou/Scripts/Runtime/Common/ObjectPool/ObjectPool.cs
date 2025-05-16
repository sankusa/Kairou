using System;
using System.Collections.Generic;

namespace Kairou
{
    internal class ObjectPool<T> where T : class
    {
        int _maxCapacity;
        readonly Stack<T> _stack;
        readonly Func<T> _createFunc;
        readonly Action<T> _onRent;
        readonly Action<T> _onReturn;

        public ObjectPool(Func<T> createFunc, int initialCapacity = 8, int maxCapacity = 8, Action<T> onRent = null, Action<T> onReturn = null)
        {
            if (createFunc == null) throw new ArgumentNullException(nameof(createFunc));

            _maxCapacity = maxCapacity;
            _stack = new Stack<T>(initialCapacity);
            _createFunc = createFunc;
            _onRent = onRent;
            _onReturn = onReturn;
        }

        public T Rent()
        {
            T item;
            if (_stack.Count == 0)
            {
                item = _createFunc();
            }
            else
            {
                item = _stack.Pop();
            }
            _onRent?.Invoke(item);
            return item;
        }

        public void Return(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _onReturn?.Invoke(item);
            if (_stack.Count < _maxCapacity) _stack.Push(item);
        }
    }
}