using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    // インスタンス利用中にObjectの破棄やScriptBookPathの不正化が起きるケースは考慮しない
    // そのようなケースではインスタンスをリセットすべき
    [Serializable]
    public class ScriptBookHolder
    {
        [SerializeField] Object _owner;
        public Object Owner => _owner;

        [SerializeField] string _scriptBookPath;
        public string ScriptBookPath => _scriptBookPath;

        ScriptBook _scriptBook;
        public ScriptBook ScriptBook
        {
            get
            {
                if (_scriptBook == null)
                {
                    if (_owner == null) return null;
                    string path = SerializedPropertyUtil.ConvertToReflectionPath(ScriptBookPath);
                    _scriptBook = ReflectionUtil.GetObject(_owner, path) as ScriptBook;
                    if (_scriptBook == null) throw new InvalidOperationException($"{nameof(ScriptBookPath)} is invalid.");
                }
                return _scriptBook;
            }
        }

        public void Reset(Object owner)
        {
            _owner = owner;
            _scriptBook = null;
        }

        public void Reset(Object owner, string scriptBookPath)
        {
            _owner = owner;
            _scriptBookPath = scriptBookPath;
            _scriptBook = null;
        }
    }
}