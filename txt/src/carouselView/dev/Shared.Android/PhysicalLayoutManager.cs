using Android.Content;
using Android.Support.V7.Widget;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms.Platform.Extensions;
using AV = Android.Views;
using Int = System.Drawing;

namespace Xamarin.Forms.Platform
{
    // RecyclerView virtualizes indexes (adapter position <-> viewGroup child index) 
    // PhysicalLayoutManager virtualizes location (regular layout <-> screen)
    internal class PhysicalLayoutManager : RecyclerView.LayoutManager
    {
        // ObservableCollection is our public entryway to this method and it only supports single item removal
        internal const int MaxItemsRemoved = 1;

        #region Private Defintions
        enum AdapterChangeType
        {
            Removed = 1,
            Added,
            Moved,
            Updated,
            Changed
        }
        #endregion

        #region Static Fields
        readonly static int s_samplesCount = 5;
        #endregion

        #region Fields
        public Context Context { get; }
        readonly VirtualLayoutManager _virtualLayout;
        readonly Queue<Action<RecyclerView.Recycler, RecyclerView.State>> _deferredLayout;
        readonly Dictionary<int, AV.View> _viewByAdaptorPosition;
        readonly HashSet<int> _visibleAdapterPosition;
        readonly SeekAndSnapScroller _scroller;

        int _positionOrigin; // coordinates are relative to the upper left corner of this element
        Vector _locationOffset; // upper left corner of screen is positionOrigin + locationOffset
        List<Vector> _samples;
        AdapterChangeType _adapterChangeType;
        #endregion

        internal PhysicalLayoutManager(Context context, VirtualLayoutManager virtualLayout)
        {
            Context = context;
            _virtualLayout = virtualLayout;
            _viewByAdaptorPosition = new Dictionary<int, AV.View>();
            _visibleAdapterPosition = new HashSet<int>();
            _samples = Enumerable.Repeat(Vector.Origin, s_samplesCount).ToList();
            _deferredLayout = new Queue<Action<RecyclerView.Recycler, RecyclerView.State>>();
            _scroller = new SeekAndSnapScroller(
                context: context,
                vectorToPosition: adapterPosition => {
                    var end = virtualLayout.LayoutItem(_positionOrigin, adapterPosition).Center();
                    var begin = Viewport.Center();
                    return end - begin;
                }
            );

            Reset(0);
        }

        #region Private Members
        // helpers to deal with locations as IntRectangles and Vectors
        Int.Rectangle Rectangle => new Int.Rectangle(0, 0, Width, Height);
        void OffsetChildren(Vector delta)
        {
            OffsetChildrenHorizontal(-delta.X);
            OffsetChildrenVertical(-delta.Y);
        }
        void ScrollBy(ref Vector delta, RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            _adapterChangeType = default(AdapterChangeType);

            delta = Viewport.BoundTranslation(
                delta: delta,
                bound: _virtualLayout.GetBounds(_positionOrigin, state)
            );

            _locationOffset += delta;
            _samples.Insert(0, delta);
            _samples.RemoveAt(_samples.Count - 1);

            OffsetChildren(delta);
            OnLayoutChildren(recycler, state);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        internal void Reset(int positionOrigin)
        {
            _viewByAdaptorPosition.Clear();
            _positionOrigin = positionOrigin;
            _visibleAdapterPosition.Clear();
            _locationOffset = Vector.Origin;
        }
        internal Vector Velocity => _samples.Aggregate((o, a) => o + a) / _samples.Count;
        internal void Layout(int width, int height)
        {
            // e.g. when rotated the width and height are updated the virtual layout will 
            // need to resize and provide a new viewport offset given the current one.
            _virtualLayout.Layout(_positionOrigin, new Int.Size(width, height), ref _locationOffset);
        }
        internal Int.Rectangle Viewport => Rectangle + _locationOffset;
        internal IEnumerable<int> VisiblePositions() => _visibleAdapterPosition;
        internal IEnumerable<AV.View> Views()
        {
            return _viewByAdaptorPosition.Values;
        }

        public override void OnAdapterChanged(RecyclerView.Adapter oldAdapter, RecyclerView.Adapter newAdapter)
        {
            RemoveAllViews();
        }
        public override void OnItemsChanged(RecyclerView recyclerView)
        {
            _adapterChangeType = AdapterChangeType.Changed;

            // low-fidelity change event; assume everything has changed. If adapter reports it has "stable IDs" then 
            // RecyclerView will attempt to synthesize high-fidelity change events: added, removed, moved, updated.
            Reset(0);
            RemoveAllViews();
        }
        public override void OnItemsAdded(RecyclerView recyclerView, int positionStart, int itemCount)
        {
            _adapterChangeType = AdapterChangeType.Added;

            _deferredLayout.Enqueue((recycler, state) => {

                var viewByAdaptorPositionCopy = _viewByAdaptorPosition.ToArray();
                _viewByAdaptorPosition.Clear();
                foreach (KeyValuePair<int, AV.View> pair in viewByAdaptorPositionCopy)
                {
                    var view = pair.Value;
                    var position = pair.Key;

                    // position unchanged
                    if (position < positionStart)
                        _viewByAdaptorPosition[position] = view;

                    // position changed
                    else
                        _viewByAdaptorPosition[position + itemCount] = view;
                }

                if (_positionOrigin >= positionStart)
                    _positionOrigin += itemCount;
            });
            base.OnItemsAdded(recyclerView, positionStart, itemCount);
        }
        public override void OnItemsRemoved(RecyclerView recyclerView, int positionStart, int itemCount)
        {
            Debug.Assert(itemCount == MaxItemsRemoved);
            _adapterChangeType = AdapterChangeType.Removed;

            var positionEnd = positionStart + itemCount;

            _deferredLayout.Enqueue((recycler, state) => {
                if (state.ItemCount == 0)
                    throw new InvalidOperationException("Cannot delete all items.");

                // re-map views to their new positions
                var viewByAdaptorPositionCopy = _viewByAdaptorPosition.ToArray();
                _viewByAdaptorPosition.Clear();
                foreach (var pair in viewByAdaptorPositionCopy)
                {
                    var view = pair.Value;
                    var position = pair.Key;

                    // position unchanged
                    if (position < positionStart)
                        _viewByAdaptorPosition[position] = view;

                    // position changed
                    else if (position >= positionEnd)
                        _viewByAdaptorPosition[position - itemCount] = view;

                    // removed
                    else
                    {
                        _viewByAdaptorPosition[-1] = view;
                        if (_visibleAdapterPosition.Contains(position))
                            _visibleAdapterPosition.Remove(position);
                    }
                }

                // if removed origin then shift origin to first removed position
                if (_positionOrigin >= positionStart && _positionOrigin < positionEnd)
                {
                    _positionOrigin = positionStart;

                    // if no items to right of removed origin then set origin to item prior to removed set
                    if (_positionOrigin >= state.ItemCount)
                    {
                        _positionOrigin = state.ItemCount - 1;

                        if (!_viewByAdaptorPosition.ContainsKey(_positionOrigin))
                            throw new InvalidOperationException(
                                "VirtualLayoutManager must add items to the left and right of the origin"
                            );
                    }
                }

                // if removed before origin then shift origin left
                else if (_positionOrigin >= positionEnd)
                    _positionOrigin -= itemCount;
            });

            base.OnItemsRemoved(recyclerView, positionStart, itemCount);
        }
        public override void OnItemsMoved(RecyclerView recyclerView, int from, int toValue, int itemCount)
        {
            _adapterChangeType = AdapterChangeType.Moved;
            base.OnItemsMoved(recyclerView, from, toValue, itemCount);
        }
        public override void OnItemsUpdated(RecyclerView recyclerView, int positionStart, int itemCount)
        {
            _adapterChangeType = AdapterChangeType.Updated;

            // rebind rendered updated elements
            _deferredLayout.Enqueue((recycler, state) => {
                for (var i = 0; i < itemCount; i++)
                {
                    var position = positionStart + i;

                    AV.View view;
                    if (!_viewByAdaptorPosition.TryGetValue(position, out view))
                        continue;

                    recycler.BindViewToPosition(view, position);
                }
            });

            base.OnItemsUpdated(recyclerView, positionStart, itemCount);
        }

        public override RecyclerView.LayoutParams GenerateDefaultLayoutParams()
        {
            return new RecyclerView.LayoutParams(AV.ViewGroup.LayoutParams.WrapContent, AV.ViewGroup.LayoutParams.WrapContent);
        }
        public override AV.View FindViewByPosition(int adapterPosition)
        {
            // Used by SmoothScrollToPosition to know when the view 
            // for the targeted adapterPosition has been attached.

            AV.View view;
            if (!_viewByAdaptorPosition.TryGetValue(adapterPosition, out view))
                return null;
            return view;
        }

        public override void ScrollToPosition(int adapterPosition)
        {
            if (adapterPosition < 0 || adapterPosition >= ItemCount)
                throw new ArgumentException(nameof(adapterPosition));

            _scroller.TargetPosition = adapterPosition;
            StartSmoothScroll(_scroller);
        }
        public override void SmoothScrollToPosition(RecyclerView recyclerView, RecyclerView.State state, int adapterPosition)
        {
            ScrollToPosition(adapterPosition);
        }
        public override bool CanScrollHorizontally() => _virtualLayout.CanScrollHorizontally;
        public override bool CanScrollVertically() => _virtualLayout.CanScrollVertically;

        // entry points
        public override bool SupportsPredictiveItemAnimations() => true;
        public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            var adapterChangeType = _adapterChangeType;
            if (state.IsPreLayout)
                adapterChangeType = default(AdapterChangeType);

            // adapter updates
            if (!state.IsPreLayout)
            {
                while (_deferredLayout.Count > 0)
                    _deferredLayout.Dequeue()(recycler, state);
            }

            // get visible items
            var positions = _virtualLayout.GetPositions(
                positionOrigin: _positionOrigin,
                itemCount: state.ItemCount,
                viewport: Viewport
            ).ToRange();

            // disappearing
            var disappearing = _viewByAdaptorPosition.Keys.Except(positions).ToList();

            // defer cleanup of displaced items and lay them out off-screen so they animate off-screen
            if (adapterChangeType == AdapterChangeType.Added)
            {
                positions = positions.Concat(disappearing).OrderBy(o => o).ToArray();
                disappearing.Clear();
            }

            // recycle
            foreach (var position in disappearing)
            {
                var view = _viewByAdaptorPosition[position];

                // remove
                _viewByAdaptorPosition.Remove(position);

                // scrap
                new DecoratedView(this, view).DetachAndScrap(recycler);
            }

            // TODO: Generalize
            if (adapterChangeType == AdapterChangeType.Removed && _positionOrigin == state.ItemCount - 1)
            {
                var vlayout = _virtualLayout.LayoutItem(_positionOrigin, _positionOrigin);
                _locationOffset = new Vector(vlayout.Width - Width, _locationOffset.Y);
            }

            _visibleAdapterPosition.Clear();
            var nextLocationOffset = new Int.Point(int.MaxValue, int.MaxValue);
            var nextPositionOrigin = int.MaxValue;
            foreach (var position in positions)
            {
                // attach
                AV.View view;
                if (!_viewByAdaptorPosition.TryGetValue(position, out view))
                    AddView(_viewByAdaptorPosition[position] = view = recycler.GetViewForPosition(position));

                // layout
                var decoratedView = new DecoratedView(this, view);
                var layout = _virtualLayout.LayoutItem(_positionOrigin, position);
                var physicalLayout = layout - _locationOffset;
                decoratedView.Layout(physicalLayout);

                var isVisible = Viewport.IntersectsWith(layout);
                if (isVisible)
                    _visibleAdapterPosition.Add(position);

                // update offsets
                if (isVisible && position < nextPositionOrigin)
                {
                    nextLocationOffset = layout.Location;
                    nextPositionOrigin = position;
                }
            }

            // update origin
            if (nextPositionOrigin != int.MaxValue)
            {
                _positionOrigin = nextPositionOrigin;
                _locationOffset -= (Vector)nextLocationOffset;
            }

            // scrapped views not re-attached must be recycled (why isn't this done by Android, I dunno)
            foreach (var viewHolder in recycler.ScrapList.ToArray())
                recycler.RecycleView(viewHolder.ItemView);
        }

        public override int ScrollHorizontallyBy(int dx, RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            var delta = new Vector(dx, 0);
            ScrollBy(ref delta, recycler, state);
            return delta.X;
        }
        public override int ScrollVerticallyBy(int dy, RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            var delta = new Vector(0, dy);
            ScrollBy(ref delta, recycler, state);
            return delta.Y;
        }

        public override string ToString()
        {
            return $"offset={_locationOffset}";
        }
    }

}
