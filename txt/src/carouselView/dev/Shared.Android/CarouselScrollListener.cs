using Android.Support.V7.Widget;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Platform
{
    internal class CarouselScrollListener : RecyclerView.OnScrollListener
    {
        enum ScrollState
        {
            Idle,
            Dragging,
            Settling
        }

        readonly Func<int> _visibleViewCount;
        readonly Action _onDragEnd;
        readonly Action _onDragStart;
        readonly Action _onScrollSettled;
        ScrollState _lastScrollState;

        internal CarouselScrollListener(
            Action onDragEnd,
            Action onDragStart,
            Action onScrollSettled,
            Func<int> visibleViewCount)
        {
            _onDragEnd = onDragEnd;
            _onDragStart = onDragStart;
            _onScrollSettled = onScrollSettled;
            _visibleViewCount = visibleViewCount;
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            var state = (ScrollState)newState;
            if (_lastScrollState != ScrollState.Dragging && state == ScrollState.Dragging)
                _onDragStart();

            if (_lastScrollState == ScrollState.Dragging && state != ScrollState.Dragging)
                _onDragEnd();

            if (_lastScrollState != ScrollState.Idle && state == ScrollState.Idle)
            {
                // Hack; android reporting idle while actually settling 
                if (_visibleViewCount() > 1)
                    state = ScrollState.Settling;
                else
                    _onScrollSettled();
            }

            _lastScrollState = state;
            base.OnScrollStateChanged(recyclerView, newState);
        }
    }
}
