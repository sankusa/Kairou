using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public static class GUICommon
    {
        public static float GetIndentWidth(int indentLevel) => indentLevel * 15f;
        public static Color ValidColor => Color.green;
        public static Color InvalidColor => Color.red;
        public static Color HoverOverlayColor => new(0, 0, 0, 0.2f);
        public static Color SelectedOverlayColor => new(1, 1, 0.95f, 0.15f);
        public static Color DefaultLabelColor = new(0.8f, 0.8f, 0.8f);

        static Texture2D _infoIcon;
        public static Texture2D InfoIcon
        {
            get
            {
                if (_infoIcon == null)
                {
                    _infoIcon = (Texture2D)EditorGUIUtility.Load("d_UnityEditor.InspectorWindow");
                }
                return _infoIcon;
            }
        }

        static Texture2D _validIcon;
        public static Texture2D ValidIcon
        {
            get
            {
                if (_validIcon == null)
                {
                    _validIcon = (Texture2D)EditorGUIUtility.Load("TestPassed");
                }
                return _validIcon;
            }
        }

        static Texture2D _invalidIcon;
        public static Texture2D InvalidIcon
        {
            get
            {
                if (_invalidIcon == null)
                {
                    _invalidIcon = (Texture2D)EditorGUIUtility.Load("TestFailed");
                }
                return _invalidIcon;
            }
        }

        static Texture2D _dropdownIcon;
        public static Texture2D DropdownIcon
        {
            get
            {
                if (_dropdownIcon == null)
                {
                    _dropdownIcon = (Texture2D)EditorGUIUtility.Load("d_dropdown");
                }
                return _dropdownIcon;
            }
        }
    }
}