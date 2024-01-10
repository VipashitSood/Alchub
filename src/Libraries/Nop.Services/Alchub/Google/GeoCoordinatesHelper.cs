using System;
using System.Collections.Generic;
using Nop.Core.Alchub.Domain;

namespace Nop.Services.Alchub.Google
{
    /// <summary>
    /// Represents the app geo coordinates helper
    /// </summary>
    public partial class GeoCoordinatesHelper
    {
        #region Methods

        /// <summary>
        /// Convert coordinate string to double type
        /// </summary>
        /// <param name="coordinateStr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static double ConvertCoordinate(string coordinateStr)
        {
            if (string.IsNullOrEmpty(coordinateStr))
                throw new ArgumentNullException(nameof(coordinateStr));
            //converts
            double.TryParse(coordinateStr, out var coordinate);

            return coordinate;
        }

        /// <summary>
        /// Prepare geofencing points generic list from geofence db string
        /// </summary>
        /// <param name="geoFenceCoordinatesStr"></param>
        /// <returns></returns>
        public static IList<CoordinatePoint> PrepareGeoFence(string geoFenceCoordinatesStr)
        {
            if (string.IsNullOrEmpty(geoFenceCoordinatesStr))
                return new List<CoordinatePoint>();

            //geofence polygon
            var polygonPoints = new List<CoordinatePoint>();
            //string to array
            var geoFence = geoFenceCoordinatesStr.Split(NopAlchubDefaults.GEOFENCE_COORDINATE_SEPARATOR);
            foreach (var pointStr in geoFence)
            {
                var pointArr = pointStr.Split(NopAlchubDefaults.LATLNG_SEPARATOR);
                polygonPoints.Add(new CoordinatePoint { Latitude = ConvertCoordinate(pointArr[0].ToString()), Longitude = ConvertCoordinate(pointArr[1].ToString()) });
            }

            return polygonPoints;
        }

        #endregion
    }
}
