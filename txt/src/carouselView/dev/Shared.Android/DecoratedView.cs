using Android.Support.V7.Widget;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Extensions;
using AV = Android.Views;
using Int = System.Drawing;

namespace Xamarin.Forms.Platform
{
    internal struct DecoratedView
    {
        public static implicit operator AV.View(DecoratedView view)
        {
            return view._view;
        }

        #region Fields
        readonly PhysicalLayoutManager _layout;
        readonly AV.View _view;
        #endregion

        internal DecoratedView(
            PhysicalLayoutManager layout,
            AV.View view)
        {
            _layout = layout;
            _view = view;
        }

        internal int Left => _layout.GetDecoratedLeft(_view);
        internal int Top => _layout.GetDecoratedTop(_view);
        internal int Bottom => _layout.GetDecoratedBottom(_view);
        internal int Right => _layout.GetDecoratedRight(_view);
        internal int Width => Right - Left;
        internal int Height => Bottom - Top;
        internal Int.Rectangle Rectangle => new Int.Rectangle(Left, Top, Width, Height);

        internal void Measure(int widthUsed, int heightUsed)
        {
            _layout.MeasureChild(_view, widthUsed, heightUsed);
        }
        internal void MeasureWithMargins(int widthUsed, int heightUsed)
        {
            _layout.MeasureChildWithMargins(_view, widthUsed, heightUsed);
        }
        internal void Layout(Int.Rectangle position)
        {
            var renderer = _view as IVisualElementRenderer;
            renderer.Element.Layout(position.ToFormsRectangle(_layout.Context));

            // causes the private LAYOUT_REQUIRED flag to be set so we can be sure the Layout call will properly chain through to all children
            Measure(position.Width, position.Height);
            _layout.LayoutDecorated(_view,
                left: position.Left,
                top: position.Top,
                right: position.Right,
                bottom: position.Bottom
            );
        }
        internal void Add()
        {
            _layout.AddView(_view);
        }
        internal void DetachAndScrap(RecyclerView.Recycler recycler)
        {
            _layout.DetachAndScrapView(_view, recycler);
        }
    }
}
