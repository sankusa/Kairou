using System;
using UnityEngine;

namespace Kairou
{
    public interface IScriptBookSlot
    {
        public abstract ScriptBook ScriptBook { get; }
    }

    [Serializable]
    public class ScriptBookComponentSlot : IScriptBookSlot
    {
        [SerializeField] ScriptBookComponent _scriptBookComponent;
        public ScriptBook ScriptBook => _scriptBookComponent?.ScriptBook;
    }

    [Serializable]
    public class ScriptBookAssetSlot : IScriptBookSlot
    {
        [SerializeField] ScriptBookAsset _scriptBookAsset;
        public ScriptBook ScriptBook => _scriptBookAsset?.ScriptBook;
    }
}