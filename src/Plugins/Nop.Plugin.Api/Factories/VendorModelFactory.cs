using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Google;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Api.Models;
using Nop.Services.Alchub.Common;
using Nop.Services.Alchub.Vendors;
using Nop.Services.Common;
using Nop.Services.DeliveryFees;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Vendors;

namespace Nop.Plugin.Api.Factories
{
    public class VendorModelFactory : IVendorModelFactory
    {

        #region Fields
        private readonly IVendorService _vendorService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IVendorTimingService _vendorTimingService;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly IPictureService _pictureService;
        private readonly VendorSettings _vendorSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly AddressSettings _addressSettings;
        private readonly IFavoriteVendorService _favoriteVendorService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor
        public VendorModelFactory(IVendorService vendorService,
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
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext)
        {
            _vendorService = vendorService;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _vendorTimingService = vendorTimingService;
            _deliveryFeeService = deliveryFeeService;
            _pictureService = pictureService;
            _vendorSettings = vendorSettings;
            _mediaSettings = mediaSettings;
            _addressSettings = addressSettings;
            _favoriteVendorService = favoriteVendorService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
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
        protected async Task<FavoriteVendorModel.FavoriteVendor> PrepareVendorTiming(FavoriteVendorModel.FavoriteVendor model, Vendor vendor)
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

            model.VendorTimeLabel = timingLabel;
            model.VendorTime = timingText;

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
        /// Prepare favorite vendor/store model
        /// </summary>
        /// <param name="favoriteModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public virtual async Task<FavoriteVendorModel> PrepareFavoriteVendorModelAsync(FavoriteVendorModel favoriteModel, Customer customer)
        {
            if (favoriteModel == null)
                throw new ArgumentNullException(nameof(favoriteModel));

            //instruction
            favoriteModel.ToggleInstruction = await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendor.Toggle.Instruction");
            favoriteModel.IsFavoriteToggleOn = customer.IsFavoriteToggleOn;

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
                var model = new FavoriteVendorModel.FavoriteVendor
                {
                    VendorId = vendor.Id,
                    ContactNumber = _vendorSettings.ShowStoreAddressInFavoriteSection ? vendor.PhoneNumber : string.Empty, //apply same ShowStoreAddress setting for phone - 15-05-23
                    DisplayContactNumber = AlchubCommonHelper.FormatPhoneNumber(vendor.PhoneNumber, NopAlchubDefaults.PHONE_NUMBER_US_FORMATE),
                    Name = vendor.Name,
                    DisplayOrder = vendor.DisplayOrder,
                    IsDeliver = deliveryAvailable,
                    IsPickup = vendor.PickAvailable,
                    IsFavorite = await _favoriteVendorService.IsFavoriteVendorAsync(vendor.Id, customer.Id),
                    ShowDistance = !string.IsNullOrEmpty(vendor.GeoLocationCoordinates) && !string.IsNullOrEmpty(customer.LastSearchedCoordinates),
                    PickupAddress = _vendorSettings.ShowStoreAddressInFavoriteSection ? vendor.PickupAddress?.Replace(", USA", "") : string.Empty //remove USA from store name - 14-06-23
                };

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
                (model.Image.Src, _) = picture != null ? await _pictureService.GetPictureUrlAsync(picture, 0) : (string.Empty, null);

                //prepare vendor open or close timing
                await PrepareVendorTiming(model, vendor);

                //fill geo lant and long
                var pointArr = vendor.GeoLocationCoordinates?.Split(NopAlchubDefaults.LATLNG_SEPARATOR);
                if (pointArr != null && pointArr.Length == 2)
                {
                    model.GeoLant = pointArr[0];
                    model.GeoLong = pointArr[1];
                }

                favoriteModel.FavoriteVendors.Add(model);
            }

            if (!favoriteModel.FavoriteVendors.Any())
                return favoriteModel;

            //by default closest vendor will come first, if no address is selected then sorting by vendor display order
            if (!string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                favoriteModel.FavoriteVendors = favoriteModel.FavoriteVendors.OrderBy(x => x.DistanceValue).ToList();
            else
                favoriteModel.FavoriteVendors = favoriteModel.FavoriteVendors.OrderBy(x => x.DisplayOrder).ToList();

            //sorting by favorite vendor 
            if (favoriteModel.FavoriteVendors.Any(x => x.IsFavorite))
            {
                var favoriteStores = favoriteModel.FavoriteVendors.Where(x => x.IsFavorite).ToList();
                favoriteModel.FavoriteVendors = favoriteModel.FavoriteVendors.OrderByDescending(x => x.IsFavorite).ThenBy(x => x.DistanceValue).ToList();
            }

            return favoriteModel;
        }

        #endregion
    }
}
