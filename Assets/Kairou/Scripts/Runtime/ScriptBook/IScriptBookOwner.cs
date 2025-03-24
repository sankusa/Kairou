using UnityEngine;

namespace Kairou
{
    public interface IScriptBookOwner
    {
        ScriptBook ScriptBook { get; }
        Object AsObject();
        string ScriptBookPropertyPath { get; }
    }
}