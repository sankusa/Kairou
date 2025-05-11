using UnityEngine;

namespace Kairou
{
    public class ScriptBookComponent : MonoBehaviour
    {
        [SerializeField] ScriptBook _book = new();
        public ScriptBook Book => _book;

        public string BookPropertyPath => nameof(_book);
    }
}