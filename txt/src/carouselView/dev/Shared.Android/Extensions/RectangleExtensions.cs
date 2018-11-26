using Android.Content;
using Android.Graphics;
using System.Diagnostics;
using Xamarin.Forms.Platform.Android;
using Int = System.Drawing;

namespace Xamarin.Forms.Platform.Extensions
{
    static internal class RectangleExtensions
    {
        internal static Vector BoundTranslation(this Int.Rectangle viewport, Vector delta, Int.Rectangle bound)
        {
            // TODO: generalize the math
            Debug.Assert(delta.X == 0 || delta.Y == 0);

            Vector start = viewport.LeadingCorner(delta);
            Vector end = start + delta;
            Vector clampedEnd = end.Clamp(bound);
            Vector clampedDelta = clampedEnd - start;
            return clampedDelta;
        }

        internal static Vector LeadingCorner(this Int.Rectangle rectangle, Vector delta)
        {
            return new Vector(
                x: delta.X < 0 ? rectangle.Left : rectangle.Right,
                y: delta.Y < 0 ? rectangle.Top : rectangle.Bottom
            );
        }
        internal static Vector Center(this Int.Rectangle rectangle)
        {
            return (Vector)rectangle.Location + (Vector)rectangle.Size / 2;
        }
        internal static int Area(this Int.Rectangle rectangle)
        {
            return rectangle.Width * rectangle.Height;
        }

        internal static Rectangle ToFormsRectangle(this Int.Rectangle rectangle, Context context)
        {
            return new Rectangle(
                x: context.FromPixels(rectangle.Left),
                y: context.FromPixels(rectangle.Top),
                width: context.FromPixels(rectangle.Width),
                height: context.FromPixels(rectangle.Height)
            );
        }
        internal static Rect ToAndroidRectangle(this Int.Rectangle rectangle)
        {
            return new Rect(
                left: rectangle.Left,
                right: rectangle.Right,
                top: rectangle.Top,
                bottom: rectangle.Bottom
            );
        }

    }
}
