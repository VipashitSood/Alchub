namespace Nop.Services.Alchub.API.Filter.V1
{
    public class VendorFilterResult
    {
        public int Id { get; set; }
        public string MainFilter { get; set; }
        public string EntityName { get; set; }
        public int MainFilterDisplayOrder { get; set; }
        public int? VendorId { get; set; }
        public string Vendor { get; set; }
        public int? VendorCount { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
