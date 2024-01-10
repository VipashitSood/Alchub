using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Alchub.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Alchub.General;
using Nop.Services.Alchub.Google;
using Nop.Services.Alchub.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Html;
using Nop.Services.Slots;

namespace Nop.Services.Vendors
{
    /// <summary>
    /// Vendor service
    /// </summary>
    public partial class VendorService : IVendorService
    {
        #region Fields

        private readonly IHtmlFormatter _htmlFormatter;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<VendorNote> _vendorNoteRepository;
        private readonly IGeoService _geoService;
        private readonly IFavoriteVendorService _favoriteVendor;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ISlotService _slotService;

        #endregion

        #region Ctor

        public VendorService(IHtmlFormatter htmlFormatter,
            IRepository<Customer> customerRepository,
            IRepository<Product> productRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<VendorNote> vendorNoteRepository,
            IGeoService geoService,
            IFavoriteVendorService favoriteVendor,
            IStaticCacheManager staticCacheManager,
            ISlotService slotService)
        {
            _htmlFormatter = htmlFormatter;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _vendorNoteRepository = vendorNoteRepository;
            _geoService = geoService;
            _favoriteVendor = favoriteVendor;
            _staticCacheManager = staticCacheManager;
            _slotService = slotService;
        }

        #endregion

        #region Methods

        #region Geo

        /// <summary>
        /// Gets the vendor idetifiers who are avialble within geo radius of current customers's searched location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IList<int>> GetAvailableGeoRadiusVendorIdsAsync(Customer customer, bool applyToggleFilter = true)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //all vendors
            var vendors = await GetAllCachedVendorsAsync(checkSlotCreated: true);

            //25-04-23 - toggle favorite vendor filter
            if (applyToggleFilter)
                vendors = (await ApplyFavoriteToggleFilterAsync(vendors, customer)).ToList();

            //check wether customer searched coordinates is avialable (18-8-22: allow all vendors if location not searched)
            if (string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                return vendors?.Select(x => x.Id)?.ToList();
            var pointArr = customer.LastSearchedCoordinates.Split(NopAlchubDefaults.LATLNG_SEPARATOR);
            if (pointArr.Length != 2)
                return new List<int>();

            //get target coordinates
            var customerLatitudeStr = pointArr[0];
            var customerLongitudeStr = pointArr[1];

            var availableDistanceVendors = new List<Vendor>();
            foreach (var vendor in vendors)
            {
                //check vendor within range
                var (isVendorWithinRange, distanceValue, distance) = await _geoService.IsVendorAvailableInDistanceRadius(customerLatitudeStr, customerLongitudeStr, vendor.GeoLocationCoordinates);
                if (isVendorWithinRange)
                    availableDistanceVendors.Add(vendor);
            }

            return availableDistanceVendors?.Select(x => x.Id)?.ToList();
        }

        /// <summary>
        /// Gets the vendor idetifiers who are avialble within geo radius of passed coordinates location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IList<int>> GetAvailableGeoRadiusVendorIdsAsync(string latlongCoordinates, IList<Vendor> allVendors = null)
        {
            //all vendors
            if (allVendors == null)
                allVendors = await GetAllCachedVendorsAsync(checkSlotCreated: true);

            //validate
            if (string.IsNullOrEmpty(latlongCoordinates))
                return allVendors?.Select(x => x.Id)?.ToList();
            var pointArr = latlongCoordinates.Split(NopAlchubDefaults.LATLNG_SEPARATOR);
            if (pointArr.Length != 2)
                return new List<int>();

            //get target coordinates
            var lat = pointArr[0];
            var lng = pointArr[1];

            var availableDistanceVendors = new List<Vendor>();
            foreach (var vendor in allVendors)
            {
                //check vendor within range
                var (isVendorWithinRange, distanceValue, distance) = await _geoService.IsVendorAvailableInDistanceRadius(lat, lng, vendor.GeoLocationCoordinates);
                if (isVendorWithinRange)
                    availableDistanceVendors.Add(vendor);
            }

            return availableDistanceVendors?.Select(x => x.Id)?.ToList();
        }

        /// <summary>
        /// Gets the vendors who are avialble within geo fence of current customers's searched location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IList<Vendor>> GetAvailableGeoFenceVendorsAsync(Customer customer, bool includePickupByDefault = true, Product product = null, bool applyToggleFilter = false)
        {
            var vendors = new List<Vendor>();

            //check wether customer searched coordinates is avialable
            if (string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                return vendors;
            var pointArr = customer.LastSearchedCoordinates.Split(NopAlchubDefaults.LATLNG_SEPARATOR);
            if (pointArr.Length != 2)
                return vendors;

            //get target coordinates
            var customerLatitudeStr = pointArr[0];
            var customerLongitudeStr = pointArr[1];

            //filter by product
            if (product != null)
            {
                //do not inject IAlchubGeneralService via constructor because it'll cause circular references
                var alchubGeneralService = EngineContext.Current.Resolve<IAlchubGeneralService>();

                //get available vendors for product
                vendors = (await alchubGeneralService.GetVendorByMasterProductIdAsync(product))?.ToList();
            }
            else
                vendors = (await GetAllCachedVendorsAsync(checkSlotCreated: true))?.ToList();

            //23-05-23 - toggle favorite vendor filter
            if (applyToggleFilter)
                vendors = (await ApplyFavoriteToggleFilterAsync(vendors, customer)).ToList();

            var availableVendors = new List<Vendor>();
            foreach (var vendor in vendors)
            {
                //check pickup available and also include pcikup by default
                if (includePickupByDefault)
                    if (vendor.PickAvailable)
                    {
                        //check vendor within range, geo radius wise (06-09-22)
                        var (isVendorWithinRange, distanceValue, distance) = await _geoService.IsVendorAvailableInDistanceRadius(customerLatitudeStr, customerLongitudeStr, vendor.GeoLocationCoordinates);
                        if (isVendorWithinRange)
                        {
                            availableVendors.Add(vendor);
                            continue;
                        }
                    }

                //check geofence wise
                var isVendorDelivereable = await _geoService.IslatlngInsideGeoFence(customerLatitudeStr, customerLongitudeStr, vendor);
                if (isVendorDelivereable)
                    availableVendors.Add(vendor);
            }

            return availableVendors;
        }

        #endregion

        /// <summary>
        /// Get vendors by identifiers
        /// </summary>
        /// <param name="vendorIds">Vendor identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        public virtual async Task<IList<Vendor>> GetVendorsByIdsAsync(int[] vendorIds)
        {
            return await _vendorRepository.GetByIdsAsync(vendorIds);
        }

        /// <summary>
        /// Gets all cached vendors
        /// </summary>
        /// <param name="storeId">Store identifiers</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="checkSlotCreated">A value indicating whether to check if vendor has created slot or not, pass false if you do not want to check slot created</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        public virtual async Task<IList<Vendor>> GetAllCachedVendorsAsync(int storeId = 0, bool showHidden = false, bool checkSlotCreated = false)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.VendorsAllCacheKey,
                storeId,
                showHidden,
                checkSlotCreated);

            var vendors = await _staticCacheManager
                .GetAsync(key, async () =>
                {
                    //all vendors
                    var allVendors = (await GetAllVendorsAsync()).ToList();

                    if (checkSlotCreated)
                    {
                        var removeVendorIds = new List<int>();
                        foreach (var v in allVendors)
                        {
                            //05-07-23
                            //cehck if any slot created (since vendor has not created slot, then his product will not be visible on front)
                            if (!await _slotService.HasVendorCreatedAnySlot(v))
                                removeVendorIds.Add(v.Id);
                        }
                        if (removeVendorIds.Any())
                            allVendors = allVendors.Where(x => !removeVendorIds.Contains(x.Id)).ToList();
                    }

                    return allVendors;
                });

            return vendors;
        }

        #endregion

        #region Favorite Toggle filter

        /// <summary>
        /// Apply favorite vendor toggle filter
        /// </summary>
        /// <param name="vendorIds">Vendor identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        public virtual async Task<IList<Vendor>> ApplyFavoriteToggleFilterAsync(IList<Vendor> vendors, Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (vendors == null || !vendors.Any())
                return new List<Vendor>();

            if (customer.IsFavoriteToggleOn)
            {
                //get favorite vendors
                var favoriteVendors = await _favoriteVendor.GetFavoriteVendorByCustomerIdAsync(customer.Id);
                //filtet by favorite vendor
                vendors = vendors.Where(av => favoriteVendors.Select(f => f.VendorId).Contains(av.Id))?.ToList();
            }
            return vendors;
        }

        #endregion
    }
}