using HugsLib.Settings;
using Verse;

namespace Hospitality
{
    /// <summary>
    /// So we can save a range as settings.
    /// </summary>
    public class ConvertibleIntRange : SettingHandleConvertible
    {
        private IntRange internalRange;

        public ConvertibleIntRange() { }

        public ConvertibleIntRange(int min, int max)
        {
            internalRange.min = min;
            internalRange.max = max;
        }

        public int Min
        {
            get => internalRange.TrueMin;
            set => internalRange.min = value;
        }

        public int Max
        {
            get => internalRange.TrueMax;
            set => internalRange.max = value;
        }

        public override void FromString(string settingValue)
        {
            var strings = settingValue.Split('|');
            if (strings.Length < 2) return;
            if (int.TryParse(strings[0], out int min)) internalRange.min = min;
            if (int.TryParse(strings[1], out int max)) internalRange.max = max;
        }

        public override string ToString()
        {
            var output = $"{internalRange.TrueMin}|{internalRange.TrueMax}";
            return output;
        }
    }
}