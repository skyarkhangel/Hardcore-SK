using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QOLTweaksPack.utilities
{
    public static class Extensions
    {
        public static float Clamp(this float val, float min, float max)
        {
            float newVal = val;
            if (newVal < min)
                newVal = min;
            if (newVal > max)
                newVal = max;
            return newVal;
        }

    }
}
