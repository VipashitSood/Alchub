namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents default values related to customers data
    /// </summary>
    public static partial class NopCustomerDefaults
    {
        #region Customer attributes

        /// <summary>
        /// Gets a name of generic attribute to store the value of 'Slot'
        /// </summary>
        public static string Slot => "SlotProduct";

        /// <summary>
        /// Gets a name of generic attribute to store the value of 'TipTypeId'
        /// </summary>
        public static string TipTypeIdAttribute => "TipTypeId";

        /// <summary>
        /// Gets a name of generic attribute to store the value of 'CustomTipAmount'
        /// </summary>
        public static string CustomTipAmountAttribute => "CustomTipAmount";

        #endregion

        #region API customer role

        /// <summary>
        /// Gets a system name of 'api' customer role // for nop.api plugin user accessibility 
        /// </summary>
        public static string API_ROLE_SYSTEM_NAME => "ApiUserRole";
        #endregion

        /// <summary>
        /// Gets a system name of multi vendors role
        /// </summary>
        public static string MultiVendorsRole => "MultiVendors";
    }
}