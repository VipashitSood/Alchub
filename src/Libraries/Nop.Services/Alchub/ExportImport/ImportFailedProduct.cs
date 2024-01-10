namespace Nop.Services.Alchub.ExportImport
{
    public partial class ImportFailedProduct
    {
        /// <summary>
        /// Gets or sets product sku
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets product upc
        /// </summary>
        public string Upc { get; set; }

        /// <summary>
        /// Gets or sets product name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets product size
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Gets or sets product stock
        /// </summary>
        public int? Stock { get; set; }

        /// <summary>
        /// Gets or sets product price
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Gets or sets exception or error
        /// </summary>
        public string Exception { get; set; }

    }
}
