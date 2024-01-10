namespace Nop.Core.Domain.Messages
{
    public static partial class MessageTemplateSystemNames
    {
        #region Import

        /// <summary>
        /// Represents system name of message about invalid product
        /// </summary>
        public const string InvalidProductMessage = "InvalidProduct.Message";

        /// <summary>
        /// Represents system name of message about invalid product message for vendor
        /// </summary>
        public const string InvalidProductMessageForVendor = "InvalidProduct.Message.Vendor";

        /// <summary>
        /// Represents system name of message about master product not imported
        /// </summary>
        public const string ProductNotImportedMessage = "Product.NotImported.Message";

        /// <summary>
        /// Represents system name of message to vendor about product sku is duplicate
        /// </summary>
        public const string DuplicateProductSkuMessageForVendor = "Duplicate.Product.Sku.Message.Vendor";

        /// <summary>
        /// Represents system name of message about unprocessed product message for vendor
        /// </summary>
        public const string UnprocessedProductMessageForVendor = "UnprocessedProduct.Message.Vendor";

        /// <summary>
        /// Represents system name of message about unprocessed product message for admin
        /// </summary>
        public const string UNPROCESSED_PRODUCT_MESSAGE_TO_ADMIN = "UnprocessedProduct.Message.Admin";

        #endregion

        #region Order flow

        /// <summary>
        /// Represents system name of notification customer about delivered order items
        /// </summary>
        public const string OrderItemsDeliveredCustomerNotification = "OrderItemsDelivered.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about delivered order items
        /// </summary>
        public const string OrderItemsPickupCompletedCustomerNotification = "OrderItemsPickupCompleted.CustomerNotification";
        
        /// <summary>
        /// Represents system name of notification customer about delivered order items
        /// </summary>
        public const string OrderItemsDispatchedCustomerNotification = "OrderItemsDispatched.CustomerNotification";


        /// <summary>
        /// Represents system name of notification customer about Cancel order items
        /// </summary>
        public const string OrderItemCancelNotificationsCustomer = "OrderItemsCancel.CustomerNotification";


        /// <summary>
        /// Represents system name of notification vendor about Cancel order items
        /// </summary>
        public const string OrderItemCancelNotificationsVendor = "OrderItemsCancel.VendorNotification";


        /// <summary>
        /// Represents system name of notification customer about DeliveryDenied order items
        /// </summary>
        public const string OrderItemDeliveryDeniedNotificationsCustomer = "OrderItemsDeliveryDenied.CustomerNotification";


        /// <summary>
        /// Represents system name of notification vendor about DeliveryDenied order items
        /// </summary>
        public const string OrderItemDeliveryDeniedNotificationsVendor = "OrderItemsDeliveryDenied.VendorNotification";


        public const string OrderItemPickupNotificationsCustomer = "OrderItemPickup.CustomerNotification";

        #endregion
    }
}
