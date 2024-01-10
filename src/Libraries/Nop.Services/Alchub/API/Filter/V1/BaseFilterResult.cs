namespace Nop.Services.Alchub.API.Filter.V1
{
    public class BaseFilterResult
    {
        public int Id { get; set; }
        public string MainFilter { get; set; }
        public string EntityName { get; set; }
        public int MainFilterDisplayOrder { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? CategoryItemCount { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
