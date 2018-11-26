using Android.Content;
using Android.Support.V7.Widget;
using AV = Android.Views;
using System.Collections.Generic;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.Platform
{
    internal class ItemViewAdapter : RecyclerView.Adapter
    {
        #region Private Definitions
        class CarouselViewHolder : RecyclerView.ViewHolder
        {
            public CarouselViewHolder(View view, IVisualElementRenderer renderer)
                : base(renderer.View)
            {
                VisualElementRenderer = renderer;
                View = view;
            }

            public View View { get; }
            public IVisualElementRenderer VisualElementRenderer { get; }
        }
        #endregion

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

            // package renderer + view
            return new CarouselViewHolder(view, renderer);
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var carouselHolder = (CarouselViewHolder)holder;

            var item = Controller.GetItem(position);
            Controller.BindView(carouselHolder.View, item);
        }
    }
}
