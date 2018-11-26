using System;
using System.Linq;

namespace Xamarin.Forms.Platform.Extensions
{
    internal static class TupleExtensions
    {
        internal static int[] ToRange(this Tuple<int, int> startAndCount)
        {
            return Enumerable.Range(startAndCount.Item1, startAndCount.Item2).ToArray();
        }
    }
}
