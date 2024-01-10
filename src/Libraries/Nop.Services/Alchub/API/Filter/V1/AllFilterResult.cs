namespace Nop.Services.Alchub.API.Filter.V1
{
    public class AllFilterResult
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsMaster { get; set; }
        public int ParentGroupedProductId { get; set; }

        public int CategoryId { get; set; }
        public int ParentCategoryId { get; set; }
        public string CategoryName { get; set; }

        public int ManufactureId { get; set; }
        public string ManfecturerName { get; set; }

        public int SpecificationAttributeId { get; set; }
        public string SpecificationAttributeName { get; set; }

        public int SpecificationOptionId { get; set; }
        public string SpecificationOptionName { get; set; }

        //product count
        public int SpecificationItemCount { get; set; }
        public int CategoryItemCount { get; set; }
        public int ManufactureItemCount { get; set; }
        public int VendorItemCount { get; set; }
    }
}
