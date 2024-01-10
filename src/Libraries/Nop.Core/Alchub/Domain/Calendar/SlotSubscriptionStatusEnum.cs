namespace Nop.Core.Domain.Calendar
{
    /// <summary>
    /// Represents the zone type
    /// </summary>
    public enum SlotSubscriptionStatusEnum
    {
        Active, // Skip past and leading time events
        NotAvailable, // Is in unvaialable date
        NotActive, // users order subscirptions

    }
}