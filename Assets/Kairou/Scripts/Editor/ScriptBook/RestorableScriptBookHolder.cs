using System;
using Kairou.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    [Serializable]
    public class RestorableScriptBookHolder
    {
        [SerializeField] ScriptBookHolder _holder = new();
        [SerializeField] GlobalObjectId _globalObjectId;

        public Object Owner => _holder.Owner;
        public string ScriptBookPath => _holder.ScriptBookPath;
        public ScriptBook ScriptBook => _holder.ScriptBook;

        public ScriptBookId ScriptBookId => new(Owner, ScriptBookPath);

        public void Reset(ScriptBookId scriptBookId) => Reset(scriptBookId.Object, scriptBookId.ScriptBookPath);

        public void Reset(Object obj, string scriptBookPath)
        {
            _holder.Reset(obj, scriptBookPath);
            _globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(obj);
        }

        public bool RestoreObjectIfNull()
        {
            if (_holder.Owner == null)
            {
                _holder.Reset(GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_globalObjectId));
                return true;
            }
            return false;
        }
    }
}