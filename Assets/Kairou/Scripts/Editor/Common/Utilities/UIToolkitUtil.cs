using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public static class UIToolkitUtil
    {
        public static (VisualElement leftPane, VisualElement rightPane) CreateSplitView(
            VisualElement parent,
            int fixedPaneIndex = 0,
            float fixedPaneStartDimension = 100,
            TwoPaneSplitViewOrientation orientation = TwoPaneSplitViewOrientation.Horizontal,
            string viewDataKey = null,
            int defaultCollapseIndex = -1)
        {
            return CreateSplitView(out TwoPaneSplitView splitView, parent, fixedPaneIndex, fixedPaneStartDimension, orientation, viewDataKey, defaultCollapseIndex);
        }

        public static (VisualElement leftPane, VisualElement rightPane) CreateSplitView(
            out TwoPaneSplitView splitView,
            VisualElement parent,
            int fixedPaneIndex = 0,
            float fixedPaneStartDimension = 100,
            TwoPaneSplitViewOrientation orientation = TwoPaneSplitViewOrientation.Horizontal,
            string viewDataKey = null,
            int defaultCollapseIndex = -1)
        {
            var view = new TwoPaneSplitView(fixedPaneIndex, fixedPaneStartDimension, orientation);
            if (viewDataKey != null) view.viewDataKey = viewDataKey;
            
            parent.Add(view);

            var leftPane = new VisualElement();
            view.Add(leftPane);
            var rightPane = new VisualElement();
            view.Add(rightPane);

            if (defaultCollapseIndex != -1)
            {
                view.schedule.Execute(() =>
                    view.CollapseChild(defaultCollapseIndex)
                );
            }

            splitView = view;

            return (leftPane, rightPane);
        }
    }
}