namespace Nop.Services.Alchub.API.Filter.V1
{
    public class ManufacturerFilterResult
    {
        public int Id { get; set; }
        public string MainFilter { get; set; }
        public string EntityName { get; set; }
        public int MainFilterDisplayOrder { get; set; }
        public int? ManufactureId { get; set; }
        public string Manufacturer { get; set; }
        public int? ManufacturerCount { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
