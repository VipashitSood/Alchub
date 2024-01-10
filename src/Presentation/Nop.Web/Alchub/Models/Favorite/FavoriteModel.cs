using System;
using System.Collections.Generic;
using Nop.Web.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Web.Models.Favorite
{
    /// <summary>
    /// Represents a favorite model
    /// </summary>
    public partial record FavoriteModel : BaseNopModel
    {
        public FavoriteModel()
        {
            WishlistModel = new WishlistModel();
            Stores = new List<FavoriteVendorModel>();
        }

        public WishlistModel WishlistModel { get; set; }

        public IList<FavoriteVendorModel> Stores { get; set; }

        public bool IsFavoriteToggle { get; set; }
    }

    public partial record FavoriteVendorModel : BaseNopEntityModel
    {
        public FavoriteVendorModel()
        {
            Picture = new PictureModel();
        }
        public string VendorName { get; set; }

        public int DisplayOrder { get; set; }

        public PictureModel Picture { get; set; }

        public string Address { get; set; }

        public string ContactNumber { get; set; }

        public string DisplayContactNumber { get; set; }

        public decimal DistanceValue { get; set; }

        public string Distance { get; set; }

        public bool IsFavorite { get; set; }

        public string TimingLabel { get; set; }

        public string TimingText { get; set; }

        public bool IsDeliver { get; set; }

        public bool IsPickup { get; set; }

        public bool ShowDistance { get; set; }

        public string AddressLat { get; set; }

        public string AddressLng { get; set; }
    }
}
