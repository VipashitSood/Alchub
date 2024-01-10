using System.Collections.Generic;

namespace Nop.Web.Models.Catalog
{
    public partial record ProductOverviewModel
    {
        /// <summary>
        /// Product size (It will be used for grouped product variant)
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Product container (It will be used for grouped product variant)
        /// </summary>
        public string Container { get; set; }

        #region Nested Classes
        public partial record ProductPriceModel
        {
            public ProductPriceModel()
            {
                PriceRangeProducts = new List<PriceRangeProductModel>();
            }

            /// <summary>
            /// Price range (min to max)
            /// </summary>
            public string PriceRange { get; set; }
            
            public IList<PriceRangeProductModel> PriceRangeProducts { get; set; }

            #region Nested Classes

            public partial record PriceRangeProductModel
            {
                public int ProductId { get; set; }
                public string Price { get; set; }
                public decimal PrcieValue { get; set; }
            }

            public bool IsWishlist { get; set; }

            #endregion
        }

        #endregion
    }
}