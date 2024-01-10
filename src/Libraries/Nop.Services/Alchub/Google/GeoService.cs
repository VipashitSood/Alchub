using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Google;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Vendors;
using Nop.Services.Configuration;

namespace Nop.Services.Alchub.Google
{
    /// <summary>
    /// Represents geo(google) interface
    /// </summary>
    public class GeoService : IGeoService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public GeoService(ISettingService settingService,
            IStoreContext storeContext)
        {
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Utilties

        /// <summary>
        /// BK Algorithm which return flag if a point lies within a polygon
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="polygonPoints"></param>
        /// <returns></returns>
        private bool IsPointInsidePolygon(double latitude, double longitude, IList<CoordinatePoint> polygon)
        {
            //algorithm
            double minX = polygon[0].Latitude;
            double maxX = polygon[0].Latitude;
            double minY = polygon[0].Longitude;
            double maxY = polygon[0].Longitude;
            for (int i = 1; i < polygon.Count; i++)
            {
                var q = polygon[i];
                minX = Math.Min(q.Latitude, minX);
                maxX = Math.Max(q.Latitude, maxX);
                minY = Math.Min(q.Longitude, minY);
                maxY = Math.Max(q.Longitude, maxY);
            }
            if (latitude < minX || latitude > maxX || longitude < minY || longitude > maxY)
            {
                return false;
            }
            // https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].Longitude > longitude) != (polygon[j].Longitude > longitude) &&
                     latitude < (polygon[j].Latitude - polygon[i].Latitude) * (longitude - polygon[i].Longitude) / (polygon[j].Longitude - polygon[i].Longitude) + polygon[i].Latitude)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        #endregion

        /// <summary>
        /// Get a value indicating whether a latlng cooridnates lies inside given polygon 
        /// </summary>
        /// <param name="latitudeStr"></param>
        /// <param name="longitudeStr"></param>
        /// <param name="geoFenceCoordinatesStr"></param>
        /// <returns>value indicates wether provided coordinates falls inside provided geofence</returns>
        public async Task<bool> IslatlngInsideGeoFence(string latitudeStr, string longitudeStr, string geoFenceCoordinatesStr)
        {
            if (string.IsNullOrEmpty(latitudeStr))
                throw new ArgumentNullException(nameof(latitudeStr));

            if (string.IsNullOrEmpty(longitudeStr))
                throw new ArgumentNullException(nameof(longitudeStr));

            if (string.IsNullOrEmpty(geoFenceCoordinatesStr))
                return false;

            //check with algorithm
            return await Task.FromResult(IsPointInsidePolygon(
                GeoCoordinatesHelper.ConvertCoordinate(latitudeStr),
                GeoCoordinatesHelper.ConvertCoordinate(longitudeStr),
                GeoCoordinatesHelper.PrepareGeoFence(geoFenceCoordinatesStr)));
        }

        /// <summary>
        /// Get a value indicating whether a latlng cooridnates lies inside given polygon 
        /// </summary>
        /// <param name="latitudeStr"></param>
        /// <param name="longitudeStr"></param>
        /// <param name="vendor"></param>
        /// <returns>value indicates wether provided coordinates falls inside provided geofence</returns>
        public async Task<bool> IslatlngInsideGeoFence(string latitudeStr, string longitudeStr, Vendor vendor)
        {
            if (string.IsNullOrEmpty(latitudeStr))
                throw new ArgumentNullException(nameof(latitudeStr));

            if (string.IsNullOrEmpty(longitudeStr))
                throw new ArgumentNullException(nameof(longitudeStr));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            //define which method to be used to check coordinates
            if (vendor.GeoFenceShapeType == GeoFenceShapeType.Manual)
                return await IslatlngInsideGeoFence(latitudeStr, longitudeStr, vendor.GeoFencingCoordinates);

            //radius polygon type?
            var (isVendorWithinRange, _, _) = await IslatlngInsideRadiusGeoFence(latitudeStr, longitudeStr, vendor.GeoLocationCoordinates, vendor.RadiusDistance, DistanceUnit.Mile);
            return isVendorWithinRange;
        }

        /// <summary>
        /// Get a value indicating whether a latlng cooridnates lies inside radius(circle) polygon 
        /// </summary>
        /// <param name="baseLatitudeStr"></param>
        /// <param name="baseLongitudeStr"></param>
        /// <param name="targetLocationCoordinatesStr"></param>
        /// <param name="totalDistance"></param>
        /// <param name="distanceUnit"></param>
        /// <returns>value indicates wether provided coordinates falls inside provided geofence</returns>
        public async Task<(bool, double, string)> IslatlngInsideRadiusGeoFence(string baseLatitudeStr, string baseLongitudeStr, string targetLocationCoordinatesStr, decimal totalDistance, DistanceUnit distanceUnit)
        {
            if (string.IsNullOrEmpty(baseLatitudeStr))
                throw new ArgumentNullException(nameof(baseLatitudeStr));

            if (string.IsNullOrEmpty(baseLongitudeStr))
                throw new ArgumentNullException(nameof(baseLongitudeStr));

            bool available = false;
            double distanceValue = 0;
            string distance = "";

            //check vandor has set location 
            if (string.IsNullOrEmpty(targetLocationCoordinatesStr))
                return (available, distanceValue, distance);

            var pointArr = targetLocationCoordinatesStr.Split(NopAlchubDefaults.LATLNG_SEPARATOR);
            if (pointArr.Length != 2)
                return (available, distanceValue, distance);

            //get target coordinates
            var toLatitudeStr = pointArr[0];
            var toLongitudeStr = pointArr[1];

            //validate distance value
            if (totalDistance <= 0)
                return (available, distanceValue, distance);

            //get distance
            distanceValue = await GetDistanceTo(baseLatitudeStr, baseLongitudeStr, toLatitudeStr, toLongitudeStr, distanceUnit);

            if (distanceValue <= decimal.ToDouble(totalDistance))
                available = !available;

            //set distance formate
            switch (distanceUnit)
            {
                case DistanceUnit.Mile:
                    distance = string.Format("{0} {1}", distanceValue.ToString("F"), "mi");
                    break;
                case DistanceUnit.Kilometer:
                    distance = string.Format("{0} {1}", distanceValue.ToString("F"), "km");
                    break;
                default:
                    distance = string.Format("{0} {1}", distanceValue.ToString("F"), "m");
                    break;
            }

            return (available, distanceValue, distance);
        }

        /// <summary>
        /// Get distance between two coordinates location point according unit type
        /// </summary>
        /// <param name="baseLatitudeStr"></param>
        /// <param name="baseLongitudeStr"></param>
        /// <param name="targetLattitudeStr"></param>
        /// <param name="targetLongitudeStr"></param>
        /// <param name="distanceUnit"></param>
        /// <returns>distance value</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<double> GetDistanceTo(string baseLatitudeStr, string baseLongitudeStr,
            string targetLattitudeStr, string targetLongitudeStr,
            DistanceUnit distanceUnit = DistanceUnit.Mile)
        {
            if (string.IsNullOrEmpty(baseLatitudeStr))
                throw new ArgumentNullException(nameof(baseLatitudeStr));

            if (string.IsNullOrEmpty(baseLongitudeStr))
                throw new ArgumentNullException(nameof(baseLongitudeStr));

            if (string.IsNullOrEmpty(targetLattitudeStr))
                throw new ArgumentNullException(nameof(targetLattitudeStr));

            if (string.IsNullOrEmpty(targetLongitudeStr))
                throw new ArgumentNullException(nameof(targetLongitudeStr));

            //from
            var basePoint = new GeoCoordinate(GeoCoordinatesHelper.ConvertCoordinate(baseLatitudeStr), GeoCoordinatesHelper.ConvertCoordinate(baseLongitudeStr));
            //to
            var targetPoint = new GeoCoordinate(GeoCoordinatesHelper.ConvertCoordinate(targetLattitudeStr), GeoCoordinatesHelper.ConvertCoordinate(targetLongitudeStr));

            //get distance in meter
            double distanceMeter = basePoint.GetDistanceTo(targetPoint);

            //make sure there's distance
            if (distanceMeter <= 0)
                return await Task.FromResult(0);

            //convert distance 
            switch (distanceUnit)
            {
                //miles
                case DistanceUnit.Mile:
                    return await Task.FromResult(distanceMeter * 0.000621371192);
                //kilomete
                case DistanceUnit.Kilometer:
                    return await Task.FromResult(distanceMeter / 1000);
                //meter
                default:
                    return await Task.FromResult(distanceMeter);
            }
        }

        /// <summary>
        /// Async method which returns value if base location distance is in range of vendors location based on setting value 
        /// </summary>
        /// <param name="baseLatitudeStr"></param>
        /// <param name="baseLongitudeStr"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<(bool, double, string)> IsVendorAvailableInDistanceRadius(string baseLatitudeStr, string baseLongitudeStr, string targetLocationCoordinatesStr)
        {
            if (string.IsNullOrEmpty(baseLatitudeStr))
                throw new ArgumentNullException(nameof(baseLatitudeStr));

            if (string.IsNullOrEmpty(baseLongitudeStr))
                throw new ArgumentNullException(nameof(baseLongitudeStr));

            bool available = false;
            double distanceValue = 0;
            string distance = "";

            //check vandor has set location 
            if (string.IsNullOrEmpty(targetLocationCoordinatesStr))
                return (available, distanceValue, distance);

            var pointArr = targetLocationCoordinatesStr.Split(NopAlchubDefaults.LATLNG_SEPARATOR);
            if (pointArr.Length != 2)
                return (available, distanceValue, distance);

            //get target coordinates
            var toLatitudeStr = pointArr[0];
            var toLongitudeStr = pointArr[1];

            //get radius value from setting(default 60)
            var store = await _storeContext.GetCurrentStoreAsync();
            var vendorSettings = await _settingService.LoadSettingAsync<VendorSettings>(store.Id);
            if (vendorSettings.DistanceRadiusValue <= 0)
                return (available, distanceValue, distance);

            //get distance
            distanceValue = await GetDistanceTo(baseLatitudeStr, baseLongitudeStr, toLatitudeStr, toLongitudeStr, vendorSettings.DistanceUnit);

            if (distanceValue <= decimal.ToDouble(vendorSettings.DistanceRadiusValue))
                available = !available;

            //set distance formate
            switch (vendorSettings.DistanceUnit)
            {
                case DistanceUnit.Mile:
                    distance = string.Format("{0} {1}", distanceValue.ToString("F"), "mi");
                    break;
                case DistanceUnit.Kilometer:
                    distance = string.Format("{0} {1}", distanceValue.ToString("F"), "km");
                    break;
                default:
                    distance = string.Format("{0} {1}", distanceValue.ToString("F"), "m");
                    break;
            }

            return (available, distanceValue, distance);
        }
    }
}
