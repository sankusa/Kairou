using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public static class GUIStyles
    {
        static GUIStyle _badge;
        public static GUIStyle Badge => _badge ??= new GUIStyle("Badge");

        static GUIStyle _groupBox;
        public static GUIStyle GroupBox => _groupBox ??= new GUIStyle("GroupBox");

        static GUIStyle _miniLabel;
        public static GUIStyle MiniLabel
        {
            get
            {
                if (_miniLabel == null)
                {
                    _miniLabel = new GUIStyle(EditorStyles.miniLabel);
                    _miniLabel.richText = true;
                }
                return _miniLabel;
            }
        }

        static GUIStyle _richTextLabel;
        public static GUIStyle RichTextLabel
        {
            get
            {
                if (_richTextLabel == null)
                {
                    _richTextLabel = new GUIStyle(EditorStyles.label);
                    _richTextLabel.richText = true;
                }
                return _richTextLabel;
            }
        }
    }
}