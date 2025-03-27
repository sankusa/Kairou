using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    internal class RectUtil
    {
        public static List<Rect> SplitRect(
            Rect rect,
            float spacing,
            bool horizontal = true,
            params (float size, bool isFixed)[] sizes)
        {
            var rects = new List<Rect>(sizes.Length);
            if (sizes.Length == 0) return rects;

            float totalFixedSize = 0;
            float totalRatio = 0;
            foreach (var (size, isFixed) in sizes)
            {
                if (isFixed) totalFixedSize += size;
                else totalRatio += size;
            }

            float totalSpacing = (sizes.Length - 1) * spacing;
            float remainingSize = (horizontal ? rect.width : rect.height) - totalFixedSize - totalSpacing;
            remainingSize = Mathf.Max(0, remainingSize);

            float offset = horizontal ? rect.x : rect.y;
            for (int i = 0; i < sizes.Length; i++)
            {
                float size = sizes[i].isFixed ? sizes[i].size : (remainingSize * sizes[i].size / totalRatio);
                Rect item = horizontal
                    ? new Rect(offset, rect.y, size, rect.height)
                    : new Rect(rect.x, offset, rect.width, size);
                rects.Add(item);
                offset += size + spacing;
            }

            return rects;
        }
    }
}