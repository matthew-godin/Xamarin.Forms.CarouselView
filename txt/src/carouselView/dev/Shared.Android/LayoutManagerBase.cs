using System;
using Int = System.Drawing;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform
{
    internal abstract class LayoutManagerBase
    {
        internal abstract Tuple<int, int> GetPositions(
            int positionOrigin,
            int itemCount,
            Int.Rectangle viewport
        );

        internal abstract Int.Rectangle LayoutItem(int positionOrigin, int position);
        internal abstract bool CanScrollHorizontally
        {
            get;
        }
        internal abstract bool CanScrollVertically
        {
            get;
        }

        internal abstract void Layout(int positionOrigin, Int.Size viewportSize, ref Vector offset);
        internal abstract Int.Rectangle GetBounds(int positionOrigin, RecyclerView.State state);
    }
}
