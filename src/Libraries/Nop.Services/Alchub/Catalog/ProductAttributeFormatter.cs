using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Tax;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product attribute formatter
    /// </summary>
    public partial class ProductAttributeFormatter
    {
        #region Custom product attributes formatter

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="separator">Separator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the attributes
        /// </returns>
        public virtual async Task<string> FormatCustomAttributesAsync(string attributesXml, string separator = " ")
        {
            var result = new StringBuilder();
            //attributes
            foreach (var attributeName in ParseCustomAttributeNames(attributesXml))
            {
                var formattedAttribute = string.Empty;
                var attValue = ParseCustomAttributeValues(attributesXml, attributeName);
                //14-09-22 CustomAttribute pattern change (att1Value att2Value)
                formattedAttribute = $"{attValue}";

                if (result.Length > 0)
                    result.Append(separator);
                result.Append(formattedAttribute);
            }

            return await Task.FromResult(result.ToString());
        }

        /// <summary>
        /// Gets selected attribute identifiers
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Selected attribute identifiers</returns>
        private IList<string> ParseCustomAttributeNames(string attributesXml)
        {
            var ids = new List<string>();
            if (string.IsNullOrEmpty(attributesXml))
                return ids;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var elements = xmlDoc.SelectNodes(@$"//Attributes/ProductAttribute");

                if (elements == null)
                    return Array.Empty<string>();

                foreach (XmlNode node in elements)
                {
                    if (node.Attributes?["NAME"] == null)
                        continue;

                    var attributeValue = node.Attributes["NAME"].InnerText.Trim();
                    if (!string.IsNullOrEmpty(attributeValue))
                        ids.Add(attributeValue);
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return ids;
        }

        /// <summary>
        /// Gets selected product attribute values
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="attName">Product attribute mapping identifier</param>
        /// <returns>Product attribute values</returns>
        private string ParseCustomAttributeValues(string attributesXml, string attName)
        {
            var selectedValues = string.Empty;
            if (string.IsNullOrEmpty(attributesXml))
                return selectedValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["NAME"] == null)
                        continue;

                    var str1 = node1.Attributes["NAME"].InnerText.Trim();
                    if (string.IsNullOrEmpty(str1))
                        continue;

                    if (str1 != attName)
                        continue;

                    var nodeList2 = node1.SelectNodes(@"ProductAttributeValue/Value");
                    foreach (XmlNode node2 in nodeList2)
                    {
                        selectedValues = node2.InnerText.Trim();
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return selectedValues;
        }

        #endregion
    }
}