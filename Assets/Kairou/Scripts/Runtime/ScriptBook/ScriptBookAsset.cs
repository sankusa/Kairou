using UnityEngine;

namespace Kairou
{
    [CreateAssetMenu(fileName = nameof(ScriptBookAsset), menuName = nameof(Kairou) + "/" + nameof(ScriptBookAsset))]
    public class ScriptBookAsset : ScriptableObject
    {
        [SerializeField] ScriptBook _book = new();
        public ScriptBook Book => _book;

        public string BookPropertyPath => nameof(_book);
    }
}