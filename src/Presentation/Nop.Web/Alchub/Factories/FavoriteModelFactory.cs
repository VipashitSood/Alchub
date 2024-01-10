using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain.Google;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Services.Alchub.Vendors;
using Nop.Services.Common;
using Nop.Services.DeliveryFees;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Vendors;
using Nop.Web.Models.Favorite;
using Nop.Web.Factories;
using Nop.Core.Alchub.Domain;
using Nop.Services.Alchub.Common;
using Nop.Services.Localization;
using Nop.Core.Alchub.Domain.Vendors;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;

namespace Nop.Web.Alchub.Factories
{
    public partial class FavoriteModelFactory : IFavoriteModelFactory
    {
        #region Fields
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly IVendorService _vendorService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IStoreContext _storeContext;
        private readonly IVendorTimingService _vendorTimingService;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly IPictureService _pictureService;
        private readonly VendorSettings _vendorSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly AddressSettings _addressSettings;
        private readonly IFavoriteVendorService _favoriteVendorService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor
        public FavoriteModelFactory(IShoppingCartService shoppingCartService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IVendorModelFactory vendorModelFactory,
            IVendorService vendorService,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IStoreContext storeContext,
            IVendorTimingService vendorTimingService,
            IDeliveryFeeService deliveryFeeService,
            IPictureService pictureService,
            VendorSettings vendorSettings,
            MediaSettings mediaSettings,
            AddressSettings addressSettings,
            IFavoriteVendorService favoriteVendorService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            _shoppingCartService = shoppingCartService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _vendorModelFactory = vendorModelFactory;
            _vendorService = vendorService;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _storeContext = storeContext;
            _vendorTimingService = vendorTimingService;
            _deliveryFeeService = deliveryFeeService;
            _pictureService = pictureService;
            _vendorSettings = vendorSettings;
            _mediaSettings = mediaSettings;
            _addressSettings = addressSettings;
            _favoriteVendorService = favoriteVendorService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _workContext = workContext;
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Prepare vendor timing
        /// </summary>
        /// <param name="model"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        protected async Task<FavoriteVendorModel> PrepareVendorTiming(FavoriteVendorModel model, Vendor vendor)
        {
            var currentUserDateTime = (await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.UtcNow, DateTimeKind.Utc));
            var vendorTimings = await _vendorTimingService.GetVendorWeeklyTimingsByVendorIdAsync(vendor.Id);
            if (vendorTimings == null || !vendorTimings.Any())
                return model;

            var timingLabel = string.Empty;
            var timingText = string.Empty;

            //todays timing
            var vendorTodaysTiming = vendorTimings.FirstOrDefault(x => x.DayId == (int)currentUserDateTime.DayOfWeek);
            if (vendorTodaysTiming != null && !vendorTodaysTiming.DayOff && vendorTodaysTiming.OpenTimeUtc.HasValue && vendorTodaysTiming.CloseTimeUtc.HasValue)
            {
                if (currentUserDateTime.TimeOfDay < vendorTodaysTiming.CloseTimeUtc.Value.TimeOfDay &&
                    currentUserDateTime.TimeOfDay > vendorTodaysTiming.OpenTimeUtc.Value.TimeOfDay)
                {
                    timingLabel = await _localizationService.GetResourceAsync("Vendor.Favorite.Timing.OpenUntil");
                    timingText = vendorTodaysTiming.CloseTimeUtc.Value.ToString("hh:mmtt").ToLowerInvariant();
                }
                //else
                //{
                //    //show next working day opening time
                //    timingLabel = await _localizationService.GetResourceAsync("Vendor.Favorite.Timing.OpenAt");
                //    timingText = vendorTodaysTiming.OpenTimeUtc.Value.ToString("hh:mm tt");
                //}
            }

            if (string.IsNullOrEmpty(timingText))
            {
                //here means, show open at timing for next working day.
                //find next working day, sort by week day accoring current day.
                var sortedVendorTimings = vendorTimings.SkipWhile(x => x.DayId != (int)currentUserDateTime.DayOfWeek)
                    .Concat(vendorTimings.TakeWhile(x => x.DayId != (int)currentUserDateTime.DayOfWeek))
                    .ToList();

                foreach (var vTiming in sortedVendorTimings)
                {
                    if (vTiming.DayId != (int)currentUserDateTime.DayOfWeek && !vTiming.DayOff && vTiming.OpenTimeUtc != null)
                    {
                        var dayGap = vTiming.DayId - (int)currentUserDateTime.DayOfWeek;
                        //show next working day opening time
                        timingLabel = await _localizationService.GetResourceAsync("Vendor.Favorite.Timing.OpenAt");
                        //show day prefix if its not very next day.
                        if (dayGap == 1 || dayGap == -1 || dayGap == 6)
                            timingText = $"{vTiming.OpenTimeUtc.Value.ToString("hh:mmtt").ToLowerInvariant()}";
                        else
                            timingText = $"{((DayOfWeek)vTiming.DayId).ToString().Substring(0, 3)} {vTiming.OpenTimeUtc.Value.ToString("hh:mmtt").ToLowerInvariant()}";

                        break;
                    }
                }
            }

            model.TimingLabel = timingLabel;
            model.TimingText = timingText;

            return model;
        }

        /// <summary>
        /// Prepare vendor distance
        /// </summary>
        /// <param name="geoLocationCoordinates"></param>
        /// <param name="lastSearchedCoordinates"></param>
        /// <returns></returns>
        protected async Task<(string, decimal)> PrepareVendorDistance(string geoLocationCoordinates, string lastSearchedCoordinates)
        {
            var distance = "";
            decimal distanceValue = 0;
            distanceValue = await _deliveryFeeService.GetDistanceAsync(geoLocationCoordinates, lastSearchedCoordinates);

            //Convert Distance from Meter to Miles
            //distanceValue = Math.Round(distanceValue / Convert.ToDecimal(1609.34), 1);
            //set distance format
            switch (_vendorSettings.DistanceUnit)
            {
                case DistanceUnit.Mile:
                    distanceValue *= Convert.ToDecimal(0.000621371192);
                    distance = string.Format("{0}{1}", distanceValue.ToString("F1"), "mi");
                    break;
                case DistanceUnit.Kilometer:
                    distanceValue /= 1000;
                    distance = string.Format("{0}{1}", distanceValue.ToString("F1"), "km");
                    break;
                default:
                    distance = string.Format("{0}{1}", distanceValue.ToString("F1"), "m");
                    break;
            }

            return (distance, distanceValue);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Prepare favorite model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="customer"></param>
        /// <param name="isEditable"></param>
        /// <returns></returns>
        public virtual async Task<FavoriteModel> PrepareFavoriteModelAsync(FavoriteModel model, Customer customer, bool isEditable = true)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);

            //prepare wishlist model
            await _shoppingCartModelFactory.PrepareWishlistModelAsync(model.WishlistModel, cart, isEditable);

            //prepare favorite vendor/store model
            await PrepareFavoriteVendorModelAsync(model, customer);

            //prepare IsFavoriteToggle
            model.IsFavoriteToggle = customer.IsFavoriteToggleOn;

            return model;
        }

        /// <summary>
        /// Prepare favorite vendor/store model
        /// </summary>
        /// <param name="favoriteModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public virtual async Task<FavoriteModel> PrepareFavoriteVendorModelAsync(FavoriteModel favoriteModel, Customer customer)
        {
            if (favoriteModel == null)
                throw new ArgumentNullException(nameof(favoriteModel));

            var availableVendors = new List<Vendor>();
            //if location is searched then, show vendors which is available according x miles else show all vendors
            if (string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                availableVendors = (await _vendorService.GetAllVendorsAsync())?.ToList();
            else
            {
                var vendorIds = await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer, applyToggleFilter: false);
                availableVendors = (await _vendorService.GetVendorsByIdsAsync(vendorIds?.ToArray()))?.ToList();
                //availableVendors = (await _vendorService.GetAvailableGeoFenceVendorsAsync(customer, false))?.ToList();
            }

            //get vendors who provides delivery 
            var availableDeliverableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync(customer, false);
            foreach (var vendor in availableVendors)
            {
                var deliveryAvailable = vendor.DeliveryAvailable && (availableDeliverableVendors.Select(x => x.Id).Contains(vendor.Id) || string.IsNullOrEmpty(customer.LastSearchedCoordinates));
                var model = new FavoriteVendorModel
                {
                    Id = vendor.Id,
                    ContactNumber = _vendorSettings.ShowStoreAddressInFavoriteSection ? vendor.PhoneNumber : string.Empty, //apply same ShowStoreAddress setting for phone - 15-05-23
                    DisplayContactNumber = AlchubCommonHelper.FormatPhoneNumber(vendor.PhoneNumber, NopAlchubDefaults.PHONE_NUMBER_US_FORMATE),
                    VendorName = vendor.Name,
                    DisplayOrder = vendor.DisplayOrder,
                    IsDeliver = deliveryAvailable,
                    IsPickup = vendor.PickAvailable,
                    IsFavorite = await _favoriteVendorService.IsFavoriteVendorAsync(vendor.Id, customer.Id),
                    ShowDistance = !string.IsNullOrEmpty(vendor.GeoLocationCoordinates) && !string.IsNullOrEmpty(customer.LastSearchedCoordinates),
                    Address = _vendorSettings.ShowStoreAddressInFavoriteSection ? vendor.PickupAddress?.Replace(", USA", "") : string.Empty //remove USA from store name - 14-06-23
                };

                //fill geo lant and long
                var pointArr = vendor.GeoLocationCoordinates?.Split(NopAlchubDefaults.LATLNG_SEPARATOR);
                if (pointArr != null && pointArr.Length == 2)
                {
                    model.AddressLat = pointArr[0];
                    model.AddressLng = pointArr[1];
                }

                //prepare vendor distance
                if (!string.IsNullOrEmpty(vendor.GeoLocationCoordinates) && !string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                {
                    var (distance, distanceValue) = await PrepareVendorDistance(vendor.GeoLocationCoordinates, customer.LastSearchedCoordinates);
                    model.Distance = distance;
                    model.DistanceValue = distanceValue;
                }

                //prepare vendor picture
                var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);
                var pictureSize = _mediaSettings.AvatarPictureSize;
                (model.Picture.ImageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, 0);

                //prepare vendor open or close timing
                await PrepareVendorTiming(model, vendor);

                favoriteModel.Stores.Add(model);
            }

            if (!favoriteModel.Stores.Any())
                return favoriteModel;

            //by default closest vendor will come first, if no address is selected then sorting by vendor display order
            if (!string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                favoriteModel.Stores = favoriteModel.Stores.OrderByDescending(x => x.IsFavorite).ThenBy(x => x.DistanceValue).ToList();
            else
                favoriteModel.Stores = favoriteModel.Stores.OrderByDescending(x => x.IsFavorite).ThenBy(x => x.DisplayOrder).ToList();


            return favoriteModel;
        }

        #endregion
    }
}
