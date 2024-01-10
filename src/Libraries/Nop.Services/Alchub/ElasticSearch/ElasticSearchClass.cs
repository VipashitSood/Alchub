using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Constraints;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Alchub.ElasticSearch
{
    public class VendorDetails
    {
        public Vendor Vendor { get; set; }
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
        public long TotalDocuments { get; set; }
    }
    public class ManufacturerDetails
    {
        public Manufacturer Manufacturer { get; set; }
        public long TotalDocuments { get; set; }
    }
    public class CategoryDetails
    {
        public Category Category { get; set; }
        public string SeName { get; set; }
        public long TotalDocuments { get; set; }
    }
    public class SpecificationAttributeOptionDetails
    {
        public SpecificationAttributeOption SpecificationAttributeOption { get; set; }
        public long TotalDocuments { get; set; }
    }



    public class SpecificationAttributeDetails
    {
        public SpecificationAttribute SpecificationAttribute { get; set; }
        public List<SpecificationAttributeOptionDetails> SpecificationAttributeOptionDetails { get; set; }

    }
    public class Master_products
    {
        public List<CategoryDetails> Categories { get; set; }
        public List<CategoryDetails> SubCategoryList { get; set; }
        public List<ManufacturerDetails> Manufacturers { get; set; }
        public List<VendorDetails> Vendors { get; set; }
        public List<SpecificationAttributeDetails> Specifications { get; set; }
        public Product Product { get; set; }
        public int Id { get; set; }
        public string SeName { get; set; }
    }

    public class Master_products_result
    {
        public List<Master_products> Master_Products { get; set; }
        public int TotalMasterProducts { get; set; }
        public List<CategoryDetails> ParentCategoryList { get; set; }
        public List<CategoryDetails> SubCategoryList { get; set; }
        public List<ManufacturerDetails> Manufacturers { get; set; }
        public List<VendorDetails> Vendors { get; set; }
        public List<SpecificationAttributeDetails> Specifications { get; set; }
        public long TotalCount { get; set; }
    }
}
