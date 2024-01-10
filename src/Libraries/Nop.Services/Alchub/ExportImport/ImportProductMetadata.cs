using System.Collections.Generic;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Services.ExportImport.Help;

namespace Nop.Services.ExportImport
{
    public partial class ImportProductMetadata
    {
        /// <summary>
        /// Upc cell number
        /// </summary>
        public int UpcCellNum { get; internal set; }

        /// <summary>
        /// All upc list
        /// </summary>
        public List<string> AllUpc { get; set; }

        /// <summary>
        /// Attribute manager
        /// </summary>
        public PropertyManager<ImportSpecificationAttribute> AttributeManager { get; internal set; }

        /// <summary>
        /// Vendor inventory by item manager
        /// </summary>
        public PropertyManager<VendorInventoryByItem> VendorInventoryByItemManager { get; internal set; }

        /// <summary>
        /// Vendor inventory by item property list
        /// </summary>
        public IList<PropertyByName<VendorInventoryByItem>> VendorInventoryByItemProperties { get; set; }
    }
}
