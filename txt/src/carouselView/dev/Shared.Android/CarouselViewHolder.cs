using Android.Support.V7.Widget;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.Platform
{
    class CarouselViewHolder : RecyclerView.ViewHolder
    {
        public CarouselViewHolder(View view, IVisualElementRenderer renderer)
            : base(renderer.View)
        {
            VisualElementRenderer = renderer;
            View = view;
        }

        public View View { get; }
        public IVisualElementRenderer VisualElementRenderer { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (VisualElementRenderer != null)
                {
                    VisualElementRenderer.Dispose();
                    VisualElementRenderer = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
