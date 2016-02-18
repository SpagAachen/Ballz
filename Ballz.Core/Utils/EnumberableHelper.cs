using System.Collections.Generic;

namespace Ballz.Utils
{
    public static class EnumberableHelper
    {
        //src: https://stackoverflow.com/questions/1290603/how-to-get-the-index-of-an-element-in-an-ienumerable
        public static int IndexOf<T>(this IEnumerable<T> source, T value)
        {
            var index = 0;
            var comparer = EqualityComparer<T>.Default; // or pass in as a parameter
            foreach (var item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }
            return -1;
        }
    }
}
