using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QOLTweaksPack.utilities
{
    internal static class Reflection
    {
        internal static object GetFieldValue(object src, string fieldName)
        {
            return src.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(src);
        }
    }
}
