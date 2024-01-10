namespace Nop.Plugin.Api.Models.ProductsParameters
{
    /// <summary>
    /// Represents a vendor api filter model
    /// </summary>
    public partial record VendorFilterModel
    {
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }

        #endregion
    }
}
