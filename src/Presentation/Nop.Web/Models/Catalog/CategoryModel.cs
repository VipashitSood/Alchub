﻿using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial record CategoryModel : BaseNopEntityModel
    {
        //public CategoryModel()
        //{
        //    PictureModel = new PictureModel();
        //    FeaturedProducts = new List<ProductOverviewModel>();
        //    CategoryBreadcrumb = new List<CategoryModel>();
        //}

        public string Name { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        
        public bool DisplayCategoryBreadcrumb { get; set; }
        public PictureModel PictureModel { get; set; }
        public IList<CategoryModel> CategoryBreadcrumb { get; set; }
        
        public IList<ProductOverviewModel> FeaturedProducts { get; set; }

        #region Nested Classes
        public partial record SubCategoryModel : BaseNopEntityModel
        {
            public SubCategoryModel()
            {
                PictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string SeName { get; set; }

            public string Description { get; set; }

            public PictureModel PictureModel { get; set; }
        }

		#endregion
    }
}