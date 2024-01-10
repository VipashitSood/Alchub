using System.Threading.Tasks;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.ScheduledOrderCancel.Services
{
    /// <summary>
    /// Represents a schedule task to change the status
    /// </summary>
    public class ScheduledOrderCancelTask : IScheduleTask
    {
        #region Fields

        private readonly ScheduledOrderCancelService _scheduledOrderCancelService;

        #endregion

        #region Ctor

        public ScheduledOrderCancelTask(ScheduledOrderCancelService scheduledOrderCancelService)
        {
            _scheduledOrderCancelService = scheduledOrderCancelService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            await _scheduledOrderCancelService.ProcessOrderCancellationAsync();
        }

        #endregion
    }
}