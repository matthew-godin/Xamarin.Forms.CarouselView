using System;
using Int = System.Drawing;
using Xamarin.Forms.Platform.Extensions;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform
{
    internal class VirtualLayoutManager : LayoutManagerBase
    {
        #region Fields
        const int Columns = 1;
        Int.Size _itemSize;
        #endregion

        #region Private Members
        int GetPosition(int itemCount, int positionOrigin, int x, bool exclusive = false)
        {
            int position = x / _itemSize.Width + positionOrigin;
            bool hasRemainder = x % _itemSize.Width != 0;

            if (hasRemainder && x < 0)
                position--;

            if (!hasRemainder && exclusive)
                position--;

            position = position.Clamp(0, itemCount - 1);
            return position;
        }
        #endregion

        internal override bool CanScrollHorizontally => true;
        internal override bool CanScrollVertically => false;

        internal override Int.Rectangle GetBounds(int originPosition, RecyclerView.State state) =>
            new Int.Rectangle(
                LayoutItem(originPosition, 0).Location,
                new Int.Size(_itemSize.Width * state.ItemCount, _itemSize.Height)
            );

        internal override Tuple<int, int> GetPositions(
            int positionOrigin,
            int itemCount,
            Int.Rectangle viewport)
        {
            // a delete could happen next; we need to layout elements that may slide on screen after a delete
            int buffer = 1;

            int left = GetPosition(itemCount, positionOrigin - buffer, viewport.Left);
            int right = GetPosition(itemCount, positionOrigin + buffer, viewport.Right, exclusive: true);

            int start = left;
            int count = right - left + 1;
            return new Tuple<int, int>(start, count);
        }
        internal override void Layout(int positionOffset, Int.Size viewportSize, ref Vector offset)
        {
            int width = viewportSize.Width / Columns;
            int height = viewportSize.Height;

            if (_itemSize.Width != 0)
                offset *= (double)width / _itemSize.Width;

            _itemSize = new Int.Size(width, height);
        }
        internal override Int.Rectangle LayoutItem(int positionOffset, int position)
        {
            // measure
            Int.Size size = _itemSize;

            // layout
            var location = new Vector((position - positionOffset) * size.Width, 0);

            // allocate
            return new Int.Rectangle(location, size);
        }

        public override string ToString()
        {
            return $"itemSize={_itemSize}";
        }
    }
}
