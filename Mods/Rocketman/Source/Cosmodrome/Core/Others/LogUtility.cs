using System;
namespace RocketMan
{
    public static class LogUtility
    {
        private static readonly int _ROCKETMAN_HEADER_LENGHT = "ROCKETMAN:".Length;
        private static readonly int _SOYUZ_HEADER_LENGHT = "SOYUZ:".Length;
        private static readonly int _ROCKETERR_HEADER_LENGHT = "ROCKETEER:".Length;
        private static readonly int _PROTON_HEADER_LENGHT = "PROTON:".Length;
        private static readonly int _GAGARIN_HEADER_LENGHT = "GAGARIN:".Length;
        private static string rocketColor = "orange";

        public static string StylizeRocketLog(this string text)
        {
            int startIndex;
            string replacement;
            try
            {
                if (text.StartsWith("ROCKETMAN:"))
                {
                    replacement = $"<color={rocketColor}>ROCKETMAN:</color> ";
                    startIndex = _ROCKETMAN_HEADER_LENGHT;
                }
                else if (text.StartsWith("SOYUZ:"))
                {
                    replacement = $"<color={rocketColor}>ROCKETMAN</color>+<color=red>SOYUZ:</color> ";
                    startIndex = _SOYUZ_HEADER_LENGHT;
                }
                else if (text.StartsWith("ROCKETEER:"))
                {
                    replacement = $"<color={rocketColor}>ROCKETMAN</color>+<color=yellow>ROCKETEER:</color> ";
                    startIndex = _ROCKETERR_HEADER_LENGHT;
                }
                else if (text.StartsWith("PROTON:"))
                {
                    replacement = $"<color={rocketColor}>ROCKETMAN</color>+<color=green>PROTON:</color> ";
                    startIndex = _PROTON_HEADER_LENGHT;
                }
                else if (text.StartsWith("GAGARIN:"))
                {
                    replacement = $"<color={rocketColor}>ROCKETMAN</color>+<color=blue>GAGARIN:</color>[<color=red>EXPERIMENTAL</color>] ";
                    startIndex = _GAGARIN_HEADER_LENGHT;
                }
                else return text;
                return replacement + text.Substring(startIndex).Trim();
            }
            catch { }
            return text;
        }
    }
}
