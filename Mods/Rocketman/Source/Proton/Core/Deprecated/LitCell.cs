using RimWorld;
using Verse;

namespace Proton
{
    public struct LitCell
    {
        private readonly ColorInt colorInt;

        public readonly int index;

        public readonly float distance;

        public readonly LitGlowerInfo glowerInfo;

        public ColorInt Color
        {
            get => new ColorInt(colorInt.r, colorInt.g, colorInt.b, colorInt.a);
        }

        public CompProperties_Glower Props
        {
            get => glowerInfo.glower.Props;
        }

        public bool ColorIsZeros
        {
            get => this.colorInt.r == 0 && this.colorInt.g == 0 && this.colorInt.b == 0;
        }

        public LitCell(LitGlowerInfo glowerInfo, ColorInt colorInt, int index, float distance)
        {
            this.glowerInfo = glowerInfo;
            this.colorInt = colorInt;
            this.index = index;
            this.distance = distance;
        }

        public static ColorInt operator +(LitCell first, ColorInt secondInt)
        {
            if (first.ColorIsZeros)
                return secondInt;
            ColorInt result = first.colorInt + secondInt;
            if (first.distance < first.Props.overlightRadius)
                result.a = 1;
            return result;
        }

        public static ColorInt operator +(LitCell first, LitCell other)
        {
            ColorInt second = other.Color;
            if (other.distance < other.Props.overlightRadius)
                second.a = 1;
            return first + other.colorInt;
        }
    }
}
