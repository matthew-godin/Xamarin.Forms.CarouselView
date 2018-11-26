using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Platform
{
    sealed internal class SeekAndSnapScroller : LinearSmoothScroller
    {
        internal enum SnapPreference
        {
            None = 0,
            Begin = 1,
            End = -1
        }

        #region Fields
        readonly SnapPreference _snapPreference;
        readonly Func<int, Vector> _vectorToPosition;
        #endregion

        internal SeekAndSnapScroller(
            Context context,
            Func<int, Vector> vectorToPosition,
            SnapPreference snapPreference = SnapPreference.None)
            : base(context)
        {
            _vectorToPosition = vectorToPosition;
            _snapPreference = snapPreference;
        }

        protected override int HorizontalSnapPreference => (int)_snapPreference;

        public override PointF ComputeScrollVectorForPosition(int targetPosition)
        {
            var vector = _vectorToPosition(targetPosition);
            return new PointF(vector.X, vector.Y);
        }

    }
}
