using Nop.Core.Configuration;

namespace Nop.Plugin.Tax.VendorWise
{
    /// <summary>
    /// Represents settings of the "vendor wise tax" tax plugin
    /// </summary>
    public class VendorWiseTaxSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the "vendor wise tax
        /// </summary>
        public bool Enable { get; set; }
    }
}