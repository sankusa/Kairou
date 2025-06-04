using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    [UxmlElement]
    public partial class ToolbarSeparator : VisualElement
    {
        public ToolbarSeparator()
        {
            style.width = 1;
            style.marginLeft = -1;
            style.backgroundColor = new Color(0, 0, 0, 0.4f);
            style.height = Length.Percent(100);
            style.alignSelf = Align.Stretch;
        }
    }
}