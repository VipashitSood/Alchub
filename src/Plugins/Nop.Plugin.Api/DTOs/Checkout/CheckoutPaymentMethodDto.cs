using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.Checkout
{
    //response
    public class CheckoutPaymentMethodDto
    {
        public CheckoutPaymentMethodDto()
        {
            PaymentMethods = new List<PaymentMethodDto>();

            //set default true for payment work flow
            IsPaymentWorkflowRequired = true;
        }

        [JsonProperty("payment_methods")]
        public IList<PaymentMethodDto> PaymentMethods { get; set; }

        [JsonProperty("is_payment_workflow_required")]
        public bool IsPaymentWorkflowRequired { get; set; }

        #region Nested classes

        public partial class PaymentMethodDto
        {
            [JsonProperty("payment_method_system_name")]
            public string PaymentMethodSystemName { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("fee")]
            public string Fee { get; set; }

            [JsonProperty("logo_url")]
            public string LogoUrl { get; set; }
        }

        #endregion
    }

    //request
    public class CheckoutPaymentMethodRequest : CheckoutBaseDto
    {
    }

    //save payment method request
    public class CheckoutSavePaymentMethodRequest : CheckoutBaseDto
    {
        [JsonProperty("payment_method_system_name")]
        public string PaymentMethodSystemName { get; set; }
    }
}
