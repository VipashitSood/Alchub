namespace Nop.Web.Models.ShoppingCart
{
    public partial record WishlistModel
    {
		#region Nested Classes

        public partial record ShoppingCartItemModel 
        {
            public int VendorId { get; set; }

            public string CustomAttributeInfo { get; set; }

            public string Size { get; set; }
        }

		#endregion
    }
}