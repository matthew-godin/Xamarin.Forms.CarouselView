using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform
{
    public partial class CarouselViewRenderer
    {
        class PositionUpdater : RecyclerView.AdapterDataObserver
        {
            #region Fields
            readonly CarouselViewRenderer _carouselView;
            #endregion

            internal PositionUpdater(CarouselViewRenderer carouselView)
            {
                _carouselView = carouselView;
            }

            public override void OnItemRangeInserted(int positionStart, int itemCount)
            {
                if (positionStart > _carouselView._position)
                {
                    // removal after the current position won't change current position
                }
                else
                {
                    // raise position changed
                    _carouselView._position += itemCount;
                    _carouselView.OnPositionChanged();
                }

                base.OnItemRangeInserted(positionStart, itemCount);
            }
            public override void OnItemRangeRemoved(int positionStart, int itemCount)
            {
                System.Diagnostics.Debug.Assert(itemCount == 1);

                if (positionStart > _carouselView._position)
                {
                    // removal after the current position won't change current position
                }
                else if (positionStart == _carouselView._position &&
                    positionStart != _carouselView.Adapter.ItemCount)
                {
                    // raise item changed
                    _carouselView.OnItemChanged();
                    return;
                }
                else
                {
                    // raise position changed
                    _carouselView._position -= itemCount;
                    _carouselView.OnPositionChanged();
                }

                base.OnItemRangeRemoved(positionStart, itemCount);
            }
        }
    }
}
