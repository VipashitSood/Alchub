﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.UI.Paging;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
    /// Represents a model to get the catalog products
    /// </summary>
    public partial record CatalogProductsCommand : BasePageableModel
    {
        #region Properties
        /// <summary>
        /// Gets or sets the price ('min-max' format)
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option ids
        /// </summary>
        [FromQuery(Name = "specs")]
        public List<int> SpecificationOptionIds { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer ids
        /// </summary>
        [FromQuery(Name = "ms")]
        public List<int> ManufacturerIds { get; set; }

        /// <summary>
        /// Gets or sets a order by
        /// </summary>
        public int? OrderBy { get; set; }

        /// <summary>
        /// Gets or sets a product sorting
        /// </summary>
        public string ViewMode { get; set; }

        #endregion
    }
}
