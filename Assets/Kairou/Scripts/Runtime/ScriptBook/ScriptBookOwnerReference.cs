using System;
using UnityEngine;
using UnityEngine.Search;
using Object = UnityEngine.Object;

namespace Kairou
{
    [Serializable]
    public class ScriptBookOwnerReference
    {
        [SerializeField] Object _scriptBookOwner;
        public IScriptBookOwner ScriptBookOwner => _scriptBookOwner as IScriptBookOwner;
    }
}