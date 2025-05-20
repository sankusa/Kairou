using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    // インスタンス利用中にObjectの破棄やBookPropertyPathの不正化が起きるケースは考慮しない
    // そのようなケースではインスタンスをリセットすべき
    [Serializable]
    public class BookHolder
    {
        [SerializeField] Object _owner;
        public Object Owner => _owner;

        [SerializeField] string _bookPropertyPath;
        public string BookPropertyPath => _bookPropertyPath;

        ScriptBook _book;
        public ScriptBook Book
        {
            get
            {
                if (_book == null)
                {
                    if (_owner == null) return null;
                    string path = SerializedPropertyUtil.ConvertToReflectionPath(BookPropertyPath);
                    _book = ReflectionUtil.GetObject(_owner, path) as ScriptBook;
                    if (_book == null) throw new InvalidOperationException($"{nameof(BookPropertyPath)} is invalid.");
                }
                return _book;
            }
        }

        public bool HasValidBook
        {
            get
            {
                if (_owner == null) return false;
                return Book != null;
            }
        }

        public void Reset(Object owner)
        {
            if (owner == null && owner.IsDestroyed()) Debug.LogWarning($"{nameof(owner)} is destroyed.");
            _owner = owner;
            _book = null;
        }

        public void Reset(Object owner, string bookPropertyPath)
        {
            _owner = owner;
            _bookPropertyPath = bookPropertyPath;
            _book = null;
        }
    }
}