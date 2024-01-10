using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product attribute parser
    /// </summary>
    public partial class ProductAttributeParser : BaseAttributeParser, IProductAttributeParser
    {
        #region Custom product attributes

        /// <summary>
        /// Get product attributes from the passed form
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form values</param>
        /// <param name="errors">Errors</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the attributes in XML format
        /// </returns>
        public virtual async Task<string> ParseCustomProductAttributesAsync(Product masterProduct, int groupedProductId)
        {
            if (masterProduct == null)
                return string.Empty;

            //if (groupedProductId == 0)
            //    return string.Empty;

            //product size & container attributes
            var attributesXml = "";
            if (!string.IsNullOrEmpty(masterProduct.Size))
                attributesXml = AddCustomProductAttribute(attributesXml, nameof(Product.Size), masterProduct.Size);
            if (!string.IsNullOrEmpty(masterProduct.Container))
                attributesXml = AddCustomProductAttribute(attributesXml, nameof(Product.Container), masterProduct.Container);

            return await Task.FromResult(attributesXml);
        }

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <param name="value">Value</param>
        /// <param name="quantity">Quantity (used with AttributeValueType.AssociatedToProduct to specify the quantity entered by the customer)</param>
        /// <returns>Updated result (XML format)</returns>
        private string AddCustomProductAttribute(string attributesXml, string attributeName, string value)
        {
            var result = string.Empty;
            try
            {
                var xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(attributesXml))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributesXml);
                }

                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                XmlElement attributeElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["NAME"] == null)
                        continue;

                    var str1 = node1.Attributes["NAME"].InnerText.Trim();
                    if (str1 != attributeName)
                        continue;

                    attributeElement = (XmlElement)node1;
                    break;
                }

                //create new one if not found
                if (attributeElement == null)
                {
                    attributeElement = xmlDoc.CreateElement("ProductAttribute");
                    attributeElement.SetAttribute("NAME", attributeName);
                    rootElement.AppendChild(attributeElement);
                }

                var attributeValueElement = xmlDoc.CreateElement("ProductAttributeValue");
                attributeElement.AppendChild(attributeValueElement);

                var attributeValueValueElement = xmlDoc.CreateElement("Value");
                attributeValueValueElement.InnerText = value;
                attributeValueElement.AppendChild(attributeValueValueElement);

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return result;
        }

        #endregion
    }
}