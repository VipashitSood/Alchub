using System;
using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Nop.Plugin.Api.DTO.Base;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Orders
{
    public partial record SubmitReturnResponseModel 
    {
        public SubmitReturnResponseModel()
        {
            Items = new List<OrderItemModel>();
            AvailableReturnReasons = new List<ReturnRequestReasonModel>();
            AvailableReturnActions= new List<ReturnRequestActionModel>();
        }
        [JsonProperty("order_id")]
        public int OrderId { get; set; }
        [JsonProperty("custom_ordernumber")]
        public string CustomOrderNumber { get; set; }

        public IList<OrderItemModel> Items { get; set; }
      
        [JsonProperty("return_reasonid")]
        public int ReturnRequestReasonId { get; set; }
        public IList<ReturnRequestReasonModel> AvailableReturnReasons { get; set; }
      
        [JsonProperty("return_actionid")]
        public int ReturnRequestActionId { get; set; }
        public IList<ReturnRequestActionModel> AvailableReturnActions { get; set; }
        
        [JsonProperty("returnrequests_comments")]
        public string Comments { get; set; }

        public bool AllowFiles { get; set; }
    
        [JsonProperty("returnrequests_uploadedfile")]
        public Guid UploadedFileGuid { get; set; }

        public string Result { get; set; }
        
        #region Nested classes

        public class OrderItemModel : BaseDto
        {
            [JsonProperty("productid")]
            public int ProductId { get; set; }

            [JsonProperty("product_name")]
            public string ProductName { get; set; }

            [JsonProperty("product_sename")]
            public string ProductSeName { get; set; }

            [JsonProperty("attribute_info")]
            public string AttributeInfo { get; set; }

            [JsonProperty("unit_price")]
            public string UnitPrice { get; set; }
            [JsonProperty("quantity")]
            public int Quantity { get; set; }
        }

        public class ReturnRequestReasonModel : BaseDto
        {
            [JsonProperty("returnrequests_uploadedfile")]
            public string Name { get; set; }
        }

        public class ReturnRequestActionModel : BaseDto
        {
            [JsonProperty("returnrequests_uploadedfile")]
            public string Name { get; set; }
        }

        #endregion
    }

}