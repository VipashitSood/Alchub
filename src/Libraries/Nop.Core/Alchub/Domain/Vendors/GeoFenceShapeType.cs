namespace Nop.Core.Alchub.Domain.Vendors
{
    /// <summary>
    /// Represents an geo fence shape type enumeration
    /// </summary>
    public enum GeoFenceShapeType
    {
        /// <summary>
        /// Manual (manual edge to edge gragable polygon)
        /// </summary>
        Manual = 0,

        /// <summary>
        /// Radius (Auto defined by providing latlng coordinates and distance value)
        /// </summary>
        Radius = 5,
    }
}