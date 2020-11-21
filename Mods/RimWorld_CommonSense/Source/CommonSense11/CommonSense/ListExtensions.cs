using System.Collections.Generic;

namespace CommonSense
{
    public static class ListExtensions
    {
        public static IList<T> SwapIndices<T>(this IList<T> list, int indexA, int indexB)
        {
            if (indexA == indexB) return list;

            T temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
            return list;
        }
    }
}
