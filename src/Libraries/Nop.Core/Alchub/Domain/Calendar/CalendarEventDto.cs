using Nop.Core.Domain.Slots;
using System;

namespace Nop.Core.Domain.Calendar
{
    /// <summary>
    /// Represents Calendar Event Dto entity
    /// </summary>
    public partial class CalendarEventDto : BaseEntity
    {

		/// <summary>
		/// Gets or sets the start date
		/// </summary>
		public string Start { get; set; }

		/// <summary>
		/// Gets or sets the end date
		/// </summary>
		public string End { get; set; }

		/// <summary>
		/// Gets or sets the title
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the color
		/// </summary>
		public string Color { get; set; }

		/// <summary>
		/// Gets or sets the base color
		/// </summary>
		public string BaseColor { get; set; }

		/// <summary>
		/// Gets or sets the type
		/// </summary>
		public int Type { get; set; }

		/// <summary>
		/// Gets or sets the is recurring
		/// </summary>
		public bool IsRecurring { get; set; }

		/// <summary>
		/// Gets or sets the recurring type
		/// </summary>
		public RecurringType RecurringType { get; set; }

		/// <summary>
		/// Gets or sets the weekdays
		/// </summary>
		public string WeekDays { get; set; }

		/// <summary>
		/// Gets or sets the type
		/// </summary>
		public string SequenceId { get; set; }

		/// <summary>
		/// Gets or sets the capacity 
		/// </summary>
		public int Capacity { get; set; }

		/// <summary>
		/// Gets or sets the price
		/// </summary>
		public decimal Price { get; set; }

		/// <summary>
		/// Gets or sets the is unavailable
		/// </summary>
		public bool IsUnavailable { get; set; }

		/// <summary>
		/// Gets or sets the name
		/// </summary>
		public string Name { get; set; }

		public CalendarEventDto(int id, DateTime start, DateTime end, string title = "", string color = "", string baseColor = "", int type = 0, bool isRecurring = false, RecurringType recurringType = 0, string weekDays = "", string sequenceIds = "", int capacity = 0, decimal price = 0, bool isUnavailable = false, string name = "")
		{
			Id = id;
			Start = DateHelper.CovertToString(start);
			End = DateHelper.CovertToString(end);
			Color = color;
			BaseColor = baseColor;
			Title = title;
			Type = type;
			IsRecurring = isRecurring;
			RecurringType = recurringType;
			WeekDays = weekDays;
			SequenceId = sequenceIds;
			Capacity = capacity;
			Price = price;
			IsUnavailable = isUnavailable;
			Name = name;
		}

		public static string GetBaseColor(bool isPrefered)
		{
			return isPrefered ? "#5b8fb4" : "#7ab45b";
		}

		public static string GetColor(SlotSubscriptionStatusEnum status, bool isPrefered, bool isCustomerBooking)
		{
			if (status == SlotSubscriptionStatusEnum.Active && isCustomerBooking)
			{
				return "rgb(73, 125, 44)";
			}

			if (status == SlotSubscriptionStatusEnum.Active && isPrefered)
			{
				return "#5b8fb4";
			}

			if (status == SlotSubscriptionStatusEnum.NotAvailable)
			{
				return "#e0e5e4";
			}

			return status == SlotSubscriptionStatusEnum.Active
				? "#7ab45b"
				: "white";
		}

		public static int GetType(int status, bool isPrefered, bool isCustomerBooking)
		{
			if (isCustomerBooking)
			{
				return 20;
			}

			if (isPrefered)
			{
				return 10;
			}

			return status;
		}
	}
}
