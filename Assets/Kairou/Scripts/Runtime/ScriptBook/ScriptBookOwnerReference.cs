using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    [Serializable]
    public class ScriptBookOwnerReference
    {
        [SerializeField] Object _scriptBookOwner;
        public IScriptBookOwner ScriptBookOwner => (IScriptBookOwner)_scriptBookOwner;
        public ScriptBook ScriptBook => ScriptBookOwner?.ScriptBook;
    }
}