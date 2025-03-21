using UnityEngine;

namespace Kairou
{
    public class ScriptBookComponent : MonoBehaviour, IScriptBookOwner
    {
        [SerializeField] ScriptBook _scriptBook = new();
        public ScriptBook ScriptBook => _scriptBook;

        public Object AsObject() => this;
        public string ScriptBookPropertyPath => nameof(_scriptBook);
    }
}