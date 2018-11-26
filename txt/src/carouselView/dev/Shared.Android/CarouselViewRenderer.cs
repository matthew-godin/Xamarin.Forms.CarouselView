using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.Platform
{
    public partial class CarouselViewRenderer : ViewRenderer<CarouselView, RecyclerView>
	{
		// http://developer.android.com/reference/android/support/v7/widget/RecyclerView.html
		// http://developer.android.com/training/material/lists-cards.html
		// http://wiresareobsolete.com/2014/09/building-a-recyclerview-layoutmanager-part-1/

		#region Fields
		PhysicalLayoutManager _physicalLayout;
		int _position;
		bool _disposed;
        #endregion

        public CarouselViewRenderer(Context context) 
            : base(context)
        {
            AutoPackage = false;
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Element != null)
					Controller.CollectionChanged -= OnCollectionChanged;

				RemoveAllViews();
			}

			base.Dispose(disposing);
		}

		#region Private Members
		void Initialize()
		{
			// cache hit? Check if the view page is already created
			RecyclerView recyclerView = Control;
			if (recyclerView != null)
				return;

			// cache miss
			recyclerView = new RecyclerView(Context);
			SetNativeControl(recyclerView);

			// layoutManager
			recyclerView.SetLayoutManager(
				layout: _physicalLayout = new PhysicalLayoutManager(
					context: Context,
					virtualLayout: new VirtualLayoutManager()
				)
			);

			// swiping
			recyclerView.AddOnScrollListener(
				new CarouselScrollListener(
					onDragStart: () => { },
					onDragEnd: () => {
						var velocity = _physicalLayout.Velocity;

						var target = velocity.X > 0 ?
							_physicalLayout.VisiblePositions().Max() :
							_physicalLayout.VisiblePositions().Min();
						_physicalLayout.ScrollToPosition(target);
					},
					onScrollSettled: () => {
						var visiblePositions = _physicalLayout.VisiblePositions().ToArray();
						_position = visiblePositions.Single();

						OnPositionChanged();
						OnItemChanged();
					},
					visibleViewCount: () => _physicalLayout.VisiblePositions().Count()
				)
			);

			// adapter
			InitializeAdapter();
		}
		void InitializeAdapter()
		{
			_position = Element.Position;

			LayoutManager.Reset(_position);

			var adapter = new ItemViewAdapter(this, Context);
			adapter.RegisterAdapterDataObserver(new PositionUpdater(this));
			Control.SetAdapter(adapter);
		}

		ItemViewAdapter Adapter => (ItemViewAdapter)Control.GetAdapter();
		PhysicalLayoutManager LayoutManager => (PhysicalLayoutManager)Control.GetLayoutManager();

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Adapter.NotifyItemRangeInserted(
						positionStart: e.NewStartingIndex,
						itemCount: e.NewItems.Count
					);
					break;

				case NotifyCollectionChangedAction.Move:
					for (var i = 0; i < e.NewItems.Count; i++)
						Adapter.NotifyItemMoved(
							fromPosition: e.OldStartingIndex + i,
							toPosition: e.NewStartingIndex + i
						);
					break;

				case NotifyCollectionChangedAction.Remove:
					if (Controller.Count == 0)
						throw new InvalidOperationException("CarouselView must retain a least one item.");

					Adapter.NotifyItemRangeRemoved(
						positionStart: e.OldStartingIndex,
						itemCount: e.OldItems.Count
					);
					break;

				case NotifyCollectionChangedAction.Replace:
					Adapter.NotifyItemRangeChanged(
						positionStart: e.OldStartingIndex,
						itemCount: e.OldItems.Count
					);
					break;

				case NotifyCollectionChangedAction.Reset:
					Adapter.NotifyDataSetChanged();
					break;

				default:
					throw new Exception($"Enum value '{(int)e.Action}' is not a member of NotifyCollectionChangedAction enumeration.");
			}
		}
		ICarouselViewController Controller => Element;
		IVisualElementController VisualElementController => Element;
		void OnPositionChanged()
		{
			Controller.Position = _position;
			Controller.SendSelectedPositionChanged(_position);
		}
		void OnItemChanged()
		{
			object item = Controller.GetItem(_position);
			Controller.SendSelectedItemChanged(item);
		}
		#endregion

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);

			CarouselView oldElement = e.OldElement;
			CarouselView newElement = e.NewElement;
			if (oldElement != null)
			{
				((ICarouselViewController)e.OldElement).CollectionChanged -= OnCollectionChanged;
			}

			if (newElement != null)
			{
				if (Control == null)
					Initialize();

				// initialize events
				((ICarouselViewController)e.NewElement).CollectionChanged += OnCollectionChanged;
			}
		}
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Element.Position) && _position != Element.Position && !Controller.IgnorePositionUpdates)
				_physicalLayout.ScrollToPosition(Element.Position);

			if (e.PropertyName == nameof(Element.ItemsSource))
				InitializeAdapter();

			base.OnElementPropertyChanged(sender, e);
		}
		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			int width = right - left;
			int height = bottom - top;

			LayoutManager.Layout(width, height);

			base.OnLayout(changed, left, top, right, bottom);

			Control.Measure(
				widthMeasureSpec: new MeasureSpecification(width, MeasureSpecification.MeasureSpecificationType.Exactly),
				heightMeasureSpec: new MeasureSpecification(height, MeasureSpecification.MeasureSpecificationType.Exactly)
			);

			Control.Layout(0, 0, width, height);
		}
		protected override Size MinimumSize()
		{
			return new Size(40, 40);
		}
	}
}