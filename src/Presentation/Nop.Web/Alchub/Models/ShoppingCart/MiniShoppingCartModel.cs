using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.ShoppingCart
{
    public partial record MiniShoppingCartModel
    {
        #region Nested Classes

        public partial record ShoppingCartItemModel
        {
            public string CustomAttributeInfo { get; set; }
        }

        #endregion
    }
}