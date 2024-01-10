namespace Nop.Services.Alchub.API.Filter.V1
{
    public class SpecificationAttributeOptionFilterResult
    {
        public int Id { get; set; }
        public string MainFilter { get; set; }
        public int MainFilterDisplayOrder { get; set; }
        public int? SpecificationOptionId { get; set; }
        public string SpecificationOptionName { get; set; }
        public int? ItemCount { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
