using System;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Alchub.Catalog
{
    /// <summary>
    /// Represents the product helper
    /// </summary>
    public partial class ProductHelper
    {
        #region Methods

        /// <summary>
        /// Generate auto SKU
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GenerateSKU(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var produtIdStr = product.Id.ToString();
            if (produtIdStr.Length <= 6)
                produtIdStr = produtIdStr.PadLeft(6, '0');
            else
                produtIdStr = produtIdStr.PadLeft(produtIdStr.Length, '0');

            return string.Format(NopAlchubDefaults.PRODUCT_SKU_PATTERN, produtIdStr); //{AH000000}
        }

        #endregion
    }
}
