namespace Nop.Core.Alchub.Domain
{
    /// <summary>
    /// Represents alchub defaults
    /// </summary>
    public static class NopAlchubDefaults
    {
        /// <summary>
        /// Gets the latitude & longitude value separator
        /// </summary>
        public const string LATLNG_SEPARATOR = ",";

        /// <summary>
        /// Gets the geofence latitude & longitude value separator
        /// </summary>
        public const string GEOFENCE_COORDINATE_SEPARATOR = "___";

        /// <summary>
        /// Gets the pattern for autogenrate product SKU.
        /// </summary>
        public const string PRODUCT_SKU_PATTERN = "AH";

        /// <summary>
        /// Gets the Add product vendor .
        /// </summary>
        public const string TOPIC_ADD_NEW_PRODUCT_VENDOR_SYS_NAME = "AddNewProductVendorInstruction";

        /// <summary>
        /// Gets the USA phone number formate.
        /// </summary>
        public const string PHONE_NUMBER_US_FORMATE = "(###)###-####";

        /// <summary>
        /// Gets the number 11 for product upc.
        /// </summary>
        public const int PRODUCT_UPC_11_DIGIT = 11;

        #region Nop.API plugin

        /// <summary>
        /// Gets the nop api all filter cache optimization schedule task NAME.
        /// </summary>
        public const string API_ALL_FILTER_CACHE_OPTIMIZATION_SCHEDULE_TASK_NAME = "All Filter API cache optimization";
        
        /// <summary>
        /// Gets the nop api all filter cache optimization schedule task TYPE.
        /// </summary>
        public const string API_ALL_FILTER_CACHE_OPTIMIZATION_SCHEDULE_TASK_TYPE = "Nop.Services.Alchub.API.Filter.AllFilterCacheOptimizationTask";

        #endregion
    }
}
