using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Verse;

namespace RocketMan
{
    public static class StringUtility
    {
        private const char _A = 'A';
        private const char _Z = 'Z';

        private const int MAX_CACHE_SIZE = 10000;

        private static readonly Dictionary<string, string> splitingCache = new Dictionary<string, string>();

        public static string SplitStringByCapitalLetters(this string inputString)
        {
            if (MAX_CACHE_SIZE < splitingCache.Count) splitingCache.Clear();
            if (splitingCache.TryGetValue(inputString, out string outputString))
            {
                return outputString;
            }
            outputString = string.Empty;
            for (int i = 0; i < inputString.Length; i++)
            {
                if (inputString[i] >= _A && inputString[i] <= _Z)
                {
                    outputString += " ";
                    outputString += inputString[i];
                    i++;
                    while (i < inputString.Length && inputString[i] >= _A && inputString[i] <= _Z)
                    {
                        outputString += inputString[i];
                        i++;
                    }
                }
                outputString += inputString[i];
            }
            return splitingCache[inputString] = outputString;
        }
    }
}
