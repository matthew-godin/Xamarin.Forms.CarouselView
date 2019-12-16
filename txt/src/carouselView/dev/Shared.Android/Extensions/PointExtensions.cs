using Int = System.Drawing;

namespace Xamarin.Forms.Platform.Extensions
{
    internal static class PointExtensions
    {
        internal static bool LexicographicallyLess(this Int.Point source, Int.Point target)
        {
            if (source.X < target.X)
                return true;

            if (source.X > target.X)
                return false;

            return source.Y < target.Y;
        }
    }
}
