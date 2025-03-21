using UnityEngine;

namespace Kairou
{
    [CreateAssetMenu(fileName = nameof(ScriptBookAsset), menuName = nameof(Kairou) + "/" + nameof(ScriptBookAsset))]
    public class ScriptBookAsset : ScriptableObject, IScriptBookOwner
    {
        [SerializeField] ScriptBook _scriptBook = new();
        public ScriptBook ScriptBook => _scriptBook;

        public Object AsObject() => this;
        public string ScriptBookPropertyPath => nameof(_scriptBook);
    }
}