using Android.Content;
using Android.Support.V7.Widget;
using AV = Android.Views;
using System.Collections.Generic;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.Platform
{
    internal class ItemViewAdapter : RecyclerView.Adapter
    {
        #region Fields
        readonly IVisualElementRenderer _renderer;
        readonly Dictionary<int, object> _typeByTypeId;
        readonly Dictionary<object, int> _typeIdByType;
        private readonly Context _context;
        int _nextItemTypeId;
        #endregion

        public ItemViewAdapter(IVisualElementRenderer carouselRenderer, Context context)
        {
            _renderer = carouselRenderer;
            _typeByTypeId = new Dictionary<int, object>();
            _typeIdByType = new Dictionary<object, int>();
            _nextItemTypeId = 0;
            _context = context;
        }

        #region Private Members
        ItemsView Element
        {
            get
            {
                return (ItemsView)_renderer.Element;
            }
        }
        IItemViewController Controller
        {
            get
            {
                return Element;
            }
        }

        private IList<RecyclerView.ViewHolder> AllCreatedViewHolders { get; } = new List<RecyclerView.ViewHolder>();
        #endregion

        public override int ItemCount => Controller.Count;
        public override int GetItemViewType(int position)
        {
            // get item and type from ItemSource and ItemTemplate
            object item = Controller.GetItem(position);
            object type = Controller.GetItemType(item);

            // map type as DataTemplate to type as Id
            int id = default(int);
            if (!_typeIdByType.TryGetValue(type, out id))
            {
                id = _nextItemTypeId++;
                _typeByTypeId[id] = type;
                _typeIdByType[type] = id;
            }
            return id;
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(AV.ViewGroup parent, int viewType)
        {
            // create view from type
            var type = _typeByTypeId[viewType];
            var view = Controller.CreateView(type);

            // create renderer for view
            var renderer = Android.Platform.CreateRendererWithContext(view, _context);
            Android.Platform.SetRenderer(view, renderer);

            var newHolder = new CarouselViewHolder(view, renderer);

            AllCreatedViewHolders.Add(newHolder);

            // package renderer + view
            return newHolder;
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var carouselHolder = (CarouselViewHolder)holder;

            var item = Controller.GetItem(position);
            Controller.BindView(carouselHolder.View, item);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var holder in AllCreatedViewHolders)
                {
                    holder.Dispose();
                }
                AllCreatedViewHolders.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
