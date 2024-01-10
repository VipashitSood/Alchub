using Nop.Core.Configuration;

namespace Nop.Core.Alchub.Domain
{
    /// <summary>
    /// Alchub settings
    /// </summary>
    public partial class AlchubSettings : ISettings
    {
        #region General

        /// <summary>
        /// Gets or sets the google api key
        /// </summary>
        public string GoogleApiKey { get; set; }

        #endregion

        #region Product

        /// <summary>
        /// Gets or sets the product sizes comma sapareted string 
        /// </summary>
        public string ProductSizes { get; set; }

        /// <summary>
        /// Gets or sets the product container comma sapareted string 
        /// </summary>
        public string ProductContainers { get; set; }

        /// <summary>
        /// Gets or sets the value which determines whether is product sku editable
        /// </summary>
        public bool IsProductSkuEditable { get; set; }

        #region Group product 

        public bool AlchubProductDetailPageVendorTakeOne { get; set; }

        public string AlchubGroupProductsTheirVariantsOrderBySizes { get; set; }

        public string AlchubGroupProductsTheirVariantsOrderByContainers { get; set; }

        public string AlchubGroupProductsTheirVariantsOrderByVariantName { get; set; }

        #endregion

        #endregion

        #region Common

        /// <summary>
        /// Gets or sets the android app store download link string 
        /// </summary>
        public string AndroidAppLink { get; set; }

        /// <summary>
        /// Gets or sets the IOS app store download link string 
        /// </summary>
        public string IOSAppLink { get; set; }

        #endregion

        #region Vendor

        /// <summary>
        /// Gets or sets the value which indicates vendor products excel file fTP path. 
        /// </summary>
        public string ExcelFileFTPPath { get; set; }

        #endregion

        #region API

        /// <summary>
        /// Gets or sets the cache time in minutes for all filter api    
        /// </summary>
        public int AllFilterApiCacheTime { get; set; }

        #endregion

        #region Picture

        /// <summary>
        /// Alchub Picture CDN URL
        /// </summary>
        public string AlchubPictureCDNURL { get; set; } = "https://futurisoft.azureedge.net/alchub/";

        public string AlchubPictureCDNPossibleFileExtensions { get; set; } = "png,jpg,jpeg,webp";

        public bool TempAddWidthParam { get; set; }
        public bool TempAddHeightParam { get; set; }
        public bool TempAddStreachModeParam { get; set; }

        #endregion Picture

        #region Sync catalog

        /// <summary>
        /// Gets or sets the value which indicates wether to log served data information before sending response.    
        /// </summary>
        public bool LogServedDataInformation { get; set; }

        #endregion

        #region DoorDash Api

        public string AlchubDoorDashDeveloperId { get; set; }

        public string AlchubDoorDashKeyId { get; set; }

        public string AlchubDoorDashSigningSecret { get; set; }

        public int AlchubDoorDashExternalDeliveryAddSeconds { get; set; }

        #endregion


    }
}
