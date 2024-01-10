using Nop.Core.Domain.Slots;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Models.Slots
{
    /// <summary>
    /// Represents a zone model
    /// </summary>
    public partial record SlotModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.Start")]
        public DateTime Start { get; set; }

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.End")]
        public DateTime End { get; set; }

        public int ZoneId { get; set; }

        public int ProductId { get; set; }


        public int VendorId { get; set; }

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.IsRecurring")]
        public bool IsRecurring { get; set; }

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.RecurringType")]
        public RecurringType RecurringType { get; set; }

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.Sequence_Id")]
        public string Sequence_Id { get; set; }

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.Capacity")]
        public int Capacity { get; set; }


        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.Price")]
        public decimal Price { get; set; }

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.PriceString")]
        public string PriceString { get; set; }

        public virtual Zone Zone { get; set; }

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.EndDate")]
        [UIHint("DateNullable")]
        public DateTime EndDate { get; set; } // duplicate property
        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.Day")]
        public string Day { get; set; }

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.IsUnavailable")]
        public bool IsUnavailable { get; set; }

        [NopResourceDisplayName("Admin.Zone.Free.Slot.Fields.Name")]
        public string Name { get; set; }

        public int TimeLeft { get; set; }
        public string WeekDays { get; set; }

        public int BlockId { get; set; }
        public string TimeSlot { get; set; }
        public string CheckoutBy { get; set; }
        public string STime { get; set; }
        public string ETime { get; set; }
        public string SDate { get; set; }
        public string EDate { get; set; }
        public string SlotColor { get; set; }
        #endregion
    }
}