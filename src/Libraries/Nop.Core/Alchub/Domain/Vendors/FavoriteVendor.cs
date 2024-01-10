namespace Nop.Core.Alchub.Domain.Vendors
{
    public partial class FavoriteVendor : BaseEntity
    {
        public int CustomerId { get; set; }

        public int VendorId { get; set; }
    }
}
