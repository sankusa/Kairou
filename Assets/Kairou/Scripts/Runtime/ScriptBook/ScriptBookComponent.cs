using UnityEngine;

namespace Kairou
{
    public class ScriptBookComponent : MonoBehaviour
    {
        [SerializeField] ScriptBook _scriptBook = new();
        public ScriptBook ScriptBook => _scriptBook;

        public string ScriptBookPropertyPath => nameof(_scriptBook);
    }
}