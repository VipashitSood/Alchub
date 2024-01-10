using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Alchub.Factories;
using Nop.Web.Models.Favorite;
using Nop.Web.Controllers;
using Nop.Services.Vendors;
using Nop.Services.Alchub.Vendors;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Services.Localization;
using System.Linq;
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;

namespace Nop.Web.Alchub.Controllers
{
    public partial class FavoriteController : BasePublicController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IFavoriteModelFactory _favoriteModelFactory;
        private readonly IFavoriteVendorService _favoriteVendorService;
        private readonly IVendorService _vendorService;
        private readonly ILocalizationService _localizationService;
        private readonly IShoppingCartService _shoppingCartService;

        #endregion

        #region Ctor
        public FavoriteController(IPermissionService permissionService,
            ICustomerService customerService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IFavoriteModelFactory favoriteModelFactory,
            IFavoriteVendorService favoriteVendorService,
            IVendorService vendorService,
            ILocalizationService localizationService,
            IShoppingCartService shoppingCartService)
        {
            _permissionService = permissionService;
            _customerService = customerService;
            _workContext = workContext;
            _storeContext = storeContext;
            _favoriteModelFactory = favoriteModelFactory;
            _favoriteVendorService = favoriteVendorService;
            _vendorService = vendorService;
            _localizationService = localizationService;
            _shoppingCartService = shoppingCartService;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> Favorite(Guid? customerGuid)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("Homepage");

            var customer = customerGuid.HasValue ?
                await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
                : await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return RedirectToRoute("Homepage");

            var model = new FavoriteModel();
            model = await _favoriteModelFactory.PrepareFavoriteModelAsync(model, customer, !customerGuid.HasValue);
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteFavoriteVendor(int vendorId)
        {
            bool status = false;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            if (customer == null)
            {
                status = false;
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Customer.NotFound"),
                    status
                });
            }

            if (vendor == null)
            {
                status = false;
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Vendor.NotFound"),
                    status
                });
            }

            try
            {
                var favoriteVendor = await _favoriteVendorService.GetFavoriteVendorByVendorIdAsync(vendor.Id, customer.Id);
                if (favoriteVendor != null)
                    await _favoriteVendorService.DeleteFavoriteVendorAsync(favoriteVendor);

                //check if no favorite vendor remain, then also trun off the toggle 
                var favoriteVendors = await _favoriteVendorService.GetFavoriteVendorByCustomerIdAsync(customer.Id);
                if (!favoriteVendors.Any())
                {
                    //update IsFavorite toggle for customer
                    customer.IsFavoriteToggleOn = false;
                    await _customerService.UpdateCustomerAsync(customer);
                }

                status = true;
            }
            catch (Exception ex)
            {
                status = false;
                return Json(new
                {
                    status,
                    message = ex.Message
                });
            }

            var model = new FavoriteModel();
            model = await _favoriteModelFactory.PrepareFavoriteModelAsync(model, customer, false);

            return PartialView("_FavoriteVendors", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AddFavoriteVendor(int vendorId)
        {
            bool status = false;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            if (customer == null)
            {
                status = false;
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Customer.NotFound"),
                    status
                });
            }

            if (vendor == null)
            {
                status = false;
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Vendor.NotFound"),
                    status
                });
            }

            try
            {
                var favoriteVendor = await _favoriteVendorService.GetFavoriteVendorByVendorIdAsync(vendor.Id, customer.Id);
                if (favoriteVendor != null)
                {
                    status = false;
                    return Json(new
                    {
                        status
                    });
                }

                favoriteVendor = new FavoriteVendor()
                {
                    CustomerId = customer.Id,
                    VendorId = vendor.Id
                };

                //add a favorite vendor
                await _favoriteVendorService.InsertFavoriteVendorAsync(favoriteVendor);

                status = true;
            }
            catch (Exception ex)
            {
                status = false;
                return Json(new
                {
                    status,
                    message = ex.Message
                });
            }

            var model = new FavoriteModel();
            model = await _favoriteModelFactory.PrepareFavoriteModelAsync(model, customer, false);

            return PartialView("_FavoriteVendors", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CanSwitchFavoriteToggleOn(bool toggle)
        {
            //if switched off, then d=do not proceed.
            if (!toggle)
            {
                return Json(new
                {
                    success = true,
                    showClearCartPopup = false,
                });
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            //cart 
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
            if (!cart.Any())
            {
                return Json(new
                {
                    success = true,
                    showClearCartPopup = false,
                });
            }

            //get favorite vendors
            var favoriteVendors = await _favoriteVendorService.GetFavoriteVendorByCustomerIdAsync(customer.Id);
            if (!favoriteVendors.Any())
            {
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Toggle.On.Error"),
                    success = false,
                    showClearCartPopup = false,
                });
            }

            var pIds = cart.Select(x => x.ProductId).ToArray();
            var shoppingCartVendors = await _vendorService.GetVendorsByProductIdsAsync(pIds);
            var shoppingCartHasVendorExeptFavorite = false;

            foreach (var vendor in shoppingCartVendors)
            {
                if (!favoriteVendors.Any(x => x.VendorId == vendor.Id))
                {
                    shoppingCartHasVendorExeptFavorite = true;
                    break;
                }
            }

            //check shopping cart has any other vendor then favorite
            if (shoppingCartHasVendorExeptFavorite)
            {
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Toggle.CanSwitchToggleOn.ShoppingCartVendor.Error"),
                    success = true,
                    showClearCartPopup = true,
                });
            }

            //here means success
            return Json(new
            {
                success = true,
                showClearCartPopup = false,
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> FavoriteToggle(bool? toggle = null, bool? clearShoppingCart = null)
        {
            bool status = false;
            bool reloadPage = false;
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (toggle == null)
            {
                status = false;
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Toggle.Empty"),
                    status
                });
            }

            if (customer == null)
            {
                status = false;
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Customer.NotFound"),
                    status
                });
            }

            try
            {
                //check switch to toggle ON possible 
                if (toggle.Value)
                {
                    var favoriteVendors = await _favoriteVendorService.GetFavoriteVendorByCustomerIdAsync(customer.Id);
                    if (favoriteVendors == null || !favoriteVendors.Any())
                    {
                        status = false;
                        return Json(new
                        {
                            message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Toggle.On.Error"),
                            status
                        });
                    }
                }

                //update IsFavorite toggle for customer
                customer.IsFavoriteToggleOn = toggle.Value;
                await _customerService.UpdateCustomerAsync(customer);

                //clear shoppingcart
                if (clearShoppingCart.HasValue && clearShoppingCart.Value == true)
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                    foreach (var item in cart)
                    {
                        await _shoppingCartService.DeleteShoppingCartItemAsync(item);
                    }

                    reloadPage = true;
                }

                status = true;
            }
            catch (Exception ex)
            {
                status = false;
                return Json(new
                {
                    status,
                    message = ex.Message
                });
            }

            var model = new FavoriteModel();
            model = await _favoriteModelFactory.PrepareFavoriteModelAsync(model, customer, false);

            //success and update html
            return Json(new
            {
                status,
                reloadPage = reloadPage,
                favoriteVendorsHtml = await RenderPartialViewToStringAsync("_FavoriteVendors", model)
            });

            //return PartialView("_FavoriteVendors", model);
        }
        #endregion
    }
}
