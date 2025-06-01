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
                if (_instance == null) _instance = AssetUtil.LoadAllAssets<GUISkin>().FirstOrDefault();
                return _instance;
            }
        }

        public Texture2D copyIcon;
        public Texture2D deleteIcon;
    }
}