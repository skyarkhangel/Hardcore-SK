using UnityEngine;

namespace MorePlanning.Common
{
    public class AdaptableColor
    {
        private string _hexColor;
        private float _h, _s, _v;
        private float _r, _g, _b, _a;

        // ReSharper disable once UnusedMember.Global
        public AdaptableColor()
        {
            Color = new Color();
        }

        public AdaptableColor(Color c)
        {
            Color = c;
        }

        public Color ObjColorH => Color.HSVToRGB(_h, 1, 1);

        public Color Color
        {
            get => new Color(_r, _g, _b, _a);

            set
            {
                _r = value.r;
                _g = value.g;
                _b = value.b;
                _a = value.a;
                Color.RGBToHSV(Color, out _h, out _s, out _v);
                _hexColor = ColorUtility.ToHtmlStringRGB(Color);
            }
        }

        public string HexColor
        {
            get => _hexColor;

            set
            {
                if (ColorUtility.TryParseHtmlString(value, out var color) == false)
                {
                    return;
                }

                color.a = _a;

                Color = color;
            }
        }

        public float H
        {
            get => _h;

            set
            {
                _h = value;
                Color c = Color.HSVToRGB(_h, _s, _v);
                _r = c.r;
                _g = c.g;
                _b = c.b;
                _hexColor = ColorUtility.ToHtmlStringRGB(Color);
            }
        }

        public float S
        {
            get => _s;

            set
            {
                _s = value;
                Color c = Color.HSVToRGB(_h, _s, _v);
                _r = c.r;
                _g = c.g;
                _b = c.b;
                _hexColor = ColorUtility.ToHtmlStringRGB(Color);
            }
        }

        public float V
        {
            get => _v;

            set
            {
                _v = value;
                Color c = Color.HSVToRGB(_h, _s, _v);
                _r = c.r;
                _g = c.g;
                _b = c.b;
                _hexColor = ColorUtility.ToHtmlStringRGB(Color);
            }
        }
    }
}
