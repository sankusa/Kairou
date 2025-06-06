using System.Linq;
using UnityEngine;

namespace Kairou.Editor
{
    [CreateAssetMenu(fileName = nameof(GUISkin), menuName = nameof(Kairou) + "/Development/" + nameof(GUISkin))]
    public class GUISkin : ScriptableObject
    {
        static GUISkin _instance;
        public static GUISkin Instance
        {
            get
            {
                if (_instance == null) _instance = AssetUtil.LoadAllAssets<GUISkin>().FirstOrDefault(x => x.name == nameof(GUISkin));
                return _instance;
            }
        }

        [SerializeField] Texture2D _copyIcon;
        public Texture2D CopyIcon => _copyIcon;
        [SerializeField] Texture2D _deleteIcon;
        public Texture2D DeleteIcon => _deleteIcon;

        [SerializeField] Color _defaultSummaryColor = Color.white;
        public Color DefaultSummaryColor => _defaultSummaryColor;
    }
}