using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kairou
{
    public readonly struct ScriptBookId
    {
        public Object Object { get; }
        public string ScriptBookPath { get; }

        public ScriptBookId(Object obj, string scriptBookPath)
        {
            Object = obj;
            ScriptBookPath = scriptBookPath;
        }
    }
}