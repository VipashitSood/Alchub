using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ScheduledOrderCancel
{
    public class ScheduledOrderCancelSettings : ISettings
    {
        public bool Enabled { get; set; }
        public int Time { get; set; }
    }
}