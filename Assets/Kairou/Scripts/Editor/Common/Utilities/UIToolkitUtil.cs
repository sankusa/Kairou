using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public static class UIToolkitUtil
    {
        public static (VisualElement leftPane, VisualElement rightPane) CreateSplitView(
            VisualElement parent,
            int fixedPaneIndex = 0,
            float fixedPaneStartDimension = 100,
            TwoPaneSplitViewOrientation orientation = TwoPaneSplitViewOrientation.Horizontal)
        {
            var splitView = new TwoPaneSplitView(fixedPaneIndex, fixedPaneStartDimension, orientation);
            parent.Add(splitView);

            var leftPane = new VisualElement();
            splitView.Add(leftPane);
            var rightPane = new VisualElement();
            splitView.Add(rightPane);

            return (leftPane, rightPane);
        }
    }
}