using System;

namespace Nop.Web.Areas.Admin.Alchub.Models.Dispatch
{
    public class DropoffLocation
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class OrderContains
    {
        public bool alcohol { get; set; }
    }

    public class Root
    {
        public string external_delivery_id { get; set; }
        public string currency { get; set; }
        public string delivery_status { get; set; }
        public int fee { get; set; }
        public string feeStr { get; set; }
        public string pickup_address { get; set; }
        public string pickup_business_name { get; set; }
        public string pickup_phone_number { get; set; }
        public string pickup_instructions { get; set; }
        public string pickup_reference_tag { get; set; }
        public string pickup_external_store_id { get; set; }
        public string dropoff_address { get; set; }
        public string dropoff_business_name { get; set; }
        public DropoffLocation dropoff_location { get; set; }
        public string dropoff_phone_number { get; set; }
        public string dropoff_instructions { get; set; }
        public int order_value { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime pickup_time_estimated { get; set; }
        public DateTime dropoff_time_estimated { get; set; }
        public int tax { get; set; }
        public string tracking_url { get; set; }
        public bool contactless_dropoff { get; set; }
        public string action_if_undeliverable { get; set; }
        public decimal tip { get; set; }
        public OrderContains order_contains { get; set; }
        public bool dropoff_requires_signature { get; set; }
        public int dropoff_cash_on_delivery { get; set; }
        public string dasher_name { get; set; }
        public string dasher_dropoff_phone_number { get; set; }
        public string dasher_pickup_phone_number { get; set; }
        public string dasher_vehicle_make { get; set; }

        public string dasher_vehicle_model { get; set; }

        public string dasher_vehicle_year { get; set; }

        public string dropoff_signature_image_url { get; set; }

        public int orderItemId { get; set; }
        public string pickupdatetime { get; set; }
        public string dropoffdatetime { get; set; }
    }


}
