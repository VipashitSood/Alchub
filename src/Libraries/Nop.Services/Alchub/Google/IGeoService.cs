using System.Threading.Tasks;
using Nop.Core.Alchub.Domain.Google;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Alchub.Google
{
    /// <summary>
    /// Represents geo(google api) interface
    /// </summary>
    public interface IGeoService
    {
        /// <summary>
        /// Return a value indicates latlng is inside gefence or not.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="geoFenceCoordinates"></param>
        /// <returns>value indicates wether provided coordinates falls inside provided geofence</returns>
        Task<bool> IslatlngInsideGeoFence(string latitude, string longitude, string geoFenceCoordinates);

        /// <summary>
        /// Get a value indicating whether a latlng cooridnates lies inside given polygon 
        /// </summary>
        /// <param name="latitudeStr"></param>
        /// <param name="longitudeStr"></param>
        /// <param name="vendor"></param>
        /// <returns>value indicates wether provided coordinates falls inside provided geofence</returns>
        Task<bool> IslatlngInsideGeoFence(string latitudeStr, string longitudeStr, Vendor vendor);

        /// <summary>
        /// Get a value indicating whether a latlng cooridnates lies inside radius(circle) polygon 
        /// </summary>
        /// <param name="baseLatitudeStr"></param>
        /// <param name="baseLongitudeStr"></param>
        /// <param name="targetLocationCoordinatesStr"></param>
        /// <param name="totalDistance"></param>
        /// <param name="distanceUnit"></param>
        /// <returns>value indicates wether provided coordinates falls inside provided geofence</returns>
        Task<(bool, double, string)> IslatlngInsideRadiusGeoFence(string baseLatitudeStr, string baseLongitudeStr, string targetLocationCoordinatesStr, decimal totalDistance, DistanceUnit distanceUnit);

        /// <summary>
        /// Get distance between two coordinates location point according unit type
        /// </summary>
        /// <param name="baseLatitudeStr"></param>
        /// <param name="baseLongitudeStr"></param>
        /// <param name="targetLattitudeStr"></param>
        /// <param name="targetLongitudeStr"></param>
        /// <param name="distanceUnit"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<double> GetDistanceTo(string baseLatitudeStr, string baseLongitudeStr,
            string targetLattitudeStr, string targetLongitudeStr,
            DistanceUnit distanceUnit = DistanceUnit.Mile);

        /// <summary>
        /// Async method which returns value if base location distance is in range of vendors location based on setting value 
        /// </summary>
        /// <param name="baseLatitudeStr"></param>
        /// <param name="baseLongitudeStr"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<(bool, double, string)> IsVendorAvailableInDistanceRadius(string baseLatitudeStr, string baseLongitudeStr, string targetLocationCoordinatesStr);
    }
}
