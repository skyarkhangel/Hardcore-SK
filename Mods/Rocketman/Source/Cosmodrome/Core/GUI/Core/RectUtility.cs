using System;
using System.Collections.Generic;
using UnityEngine;

namespace RocketMan
{
    public static class RectUtility
    {
        public static Rect[] Columns(this Rect rect, int pieces, float gap = 5f)
        {
            if (pieces <= 1)
            {
                throw new InvalidOperationException("Can't divide into 1 or less pieces");
            }
            float step = rect.width / pieces - gap * (pieces - 1);
            Rect[] rects = new Rect[pieces];
            Rect current = new Rect(rect.position, new Vector2(step, rect.height));
            for (int i = 0; i < pieces; i++)
            {
                rects[i] = new Rect(current);
                current.x += step;
            }
            return rects;
        }

        public static Rect MoveTopLeftCorner(this Rect rect, float dx = 0, float dy = 0)
        {
            return new Rect(rect.xMin + dx, rect.yMin + dy, rect.width - dx, rect.height - dy);
        }

        public static Rect MoveBottomRightCorner(this Rect rect, float dx = 0, float dy = 0)
        {
            return new Rect(rect.xMin, rect.yMin, rect.width + dx, rect.height + dy);
        }

        public static Rect[] Rows(this Rect rect, int pieces, float gap = 5f)
        {
            if (pieces <= 1)
            {
                throw new InvalidOperationException("Can't divide into 1 or less pieces");
            }
            float step = rect.height / pieces - gap * (pieces - 1);
            Rect[] rects = new Rect[pieces];
            Rect current = new Rect(rect.position, new Vector2(rect.width, step));
            for (int i = 0; i < pieces; i++)
            {
                rects[i] = new Rect(current);
                current.y += step;
            }
            return rects;
        }
    }
}
