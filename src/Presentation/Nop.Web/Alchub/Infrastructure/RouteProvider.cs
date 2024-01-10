using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Services.Installation;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Web.Alchub.Infrastructure
{
    /// <summary>
    /// Represents alchub provider that provided basic routes
    /// </summary>
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //get language pattern
            //it's not needed to use language pattern in AJAX requests and for actions returning the result directly (e.g. file to download),
            //use it only for URLs of pages that the user can go to
            var lang = GetLanguageRoutePattern();

            //set customer searched coordinates (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "SetCustomerSearchedCoordinates",
                pattern: $"setcustomersearchedcoordinates",
                defaults: new { controller = "Common", action = "SetCustomerSearchedCoordinates" });

            //delete shopping cart
            endpointRouteBuilder.MapControllerRoute(name: "DeleteProductFromCart-Details",
                pattern: $"deleteproductfromcart/details/{{productId:min(0)}}/{{shoppingCartTypeId:min(0)}}",
                defaults: new { controller = "ShoppingCart", action = "DeleteProductFromCart_Details" });

            //Show Add To Cart Confirm Box
            endpointRouteBuilder.MapControllerRoute(name: "ShowAddToCartConfirmBox",
                pattern: $"showaddtocartconfirmbox/details/{{productId:min(0)}}/{{isPickup}}",
                defaults: new { controller = "ShoppingCart", action = "ShowAddToCartConfirmBox" });

            //Show Add to cart confirm box for multi vendor
            endpointRouteBuilder.MapControllerRoute(name: "ShowAddToCartConfirmBoxForMultiVendorsSelect",
                pattern: $"showaddtocartconfirmboxformultivendorselect/details/{{masterProductId:min(0)}}/{{productId:min(0)}}",
                defaults: new { controller = "ShoppingCart", action = "ShowAddToCartConfirmBoxForMultiVendorsSelect" });

            endpointRouteBuilder.MapControllerRoute(name: "MultiSelectedCategories",
               pattern: $"category/products/",
               defaults: new { controller = "Catalog", action = "MultiSelectedCategory" });

            endpointRouteBuilder.MapControllerRoute(name: "UpdateItemQuantity",
                pattern: $"updateitemquantity/catalog/{{productId:min(0)}}/{{shoppingCartTypeId:min(0)}}/{{quantity:min(-1)}}/{{manuallyChange?}}",
                defaults: new { controller = "ShoppingCart", action = "UpdateItemQuantity" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomSearch",
               pattern: $"{lang}/search/",
               defaults: new { controller = "Catalog", action = "CustomSearch" });

            //logout as multi vendor impersonate
            endpointRouteBuilder.MapControllerRoute(name: "LogoutImpersonatedVendor",
                pattern: $"{lang}/logoutImpersonate/",
                defaults: new { controller = "Customer", action = "LogoutImpersonatedVendor" });

            //product box fastest slot (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "GetProductFastestSlot",
                pattern: $"getproductfastestslot",
                defaults: new { controller = "Product", action = "GetProductFastestSlot" });

            //favorite link
            endpointRouteBuilder.MapControllerRoute(name: "CustomerFavorite",
                pattern: $"{lang}/customer/favorite/{{customerGuid?}}",
                defaults: new { controller = "Favorite", action = "Favorite" });

            //add favorite vendor
            endpointRouteBuilder.MapControllerRoute(name: "AddFavoriteVendor",
                pattern: $"customer/addfavoritevendor/{{vendorId:min(0)}}",
                defaults: new { controller = "Favorite", action = "AddFavoriteVendor" });

            //delete favorite vendor
            endpointRouteBuilder.MapControllerRoute(name: "DeleteFavoriteVendor",
                pattern: $"customer/deletefavoritevendor/{{vendorId:min(0)}}",
                defaults: new { controller = "Favorite", action = "DeleteFavoriteVendor" });

            endpointRouteBuilder.MapControllerRoute(name: "FavoriteToggle",
              pattern: $"customer/favoritetoggle/{{toggle?}}",
              defaults: new { controller = "Favorite", action = "FavoriteToggle" });

            endpointRouteBuilder.MapControllerRoute(name: "CanSwitchFavoriteToggleOn",
              pattern: $"customer/CanSwitchFavoriteToggleOn/{{toggle?}}",
              defaults: new { controller = "Favorite", action = "CanSwitchFavoriteToggleOn" });

            endpointRouteBuilder.MapControllerRoute(name: "UpdateStatus",
              pattern: $"updatestatus",
              defaults: new { controller = "DoorDash", action = "UpdateStatus" });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;

        #endregion
    }
}