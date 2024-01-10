var defaultTimeFormate = 'h(:mm)a';
var fullCalendarBookedModulePackage = (function () {
	var calendarPlaceholder = "#calendar";
	var selectCallback;
	var selectDayCallback;
	var calendarView;
	var calendarStartDate;
	var calendarEndDate;
	var calendarBusyDay = "#F1A94E";
	var calendarClosedDay = "#BEB9B5";
	var events = [];
    var calendarRepository = calendarRepositoryModule();
	return {
		init: function () {
			console.log('full Calendar Init');
			this.initFullCalendar();
		},
		initSelectCallback: function (callback, self) {
			calendarView = self;
			selectCallback = callback;
		},
		initSelectDayCallback: function (callback, self) {
			calendarView = self;
			selectDayCallback = callback;
		},
		initFullCalendar: function () {
			var self = this;
			var today = moment().day();
			$(calendarPlaceholder).fullCalendar({
				header: {
					left: 'prev,next today',
					center: 'title',
					right: 'agendaWeek'
				},
				defaultView: 'agendaWeek',
				minTime: "00:00",
				maxTime: "24:00",
				height: 625,
				slotDuration: '00:60:00',
				selectable: true,
				selectHelper: true,
				defaultDate: new Date(),
				firstDay: today,
				allDaySlot: false,
				editable: false,
				eventLimit: true, // allow "more" link when too many events
				viewRender: function (view, element) {
					calendarStartDate = self.formatDate(view.start.toDate());
					calendarEndDate = self.formatDate(view.end.toDate());
					self.loadEvents();
				},
				eventAfterRender: function (event, element, view) {
					element.css('background-color', event.color);
					element.css('border-color', event.color);

					if (event.type == 1) {
						$('.fc-day[data-date="' + event.start.format('YYYY-MM-DD') + '"]').css('background', calendarClosedDay);
					} else if (event.type == 2) {
						$('.fc-day[data-date="' + event.start.format('YYYY-MM-DD') + '"]').css('background', calendarBusyDay);
					}

					if (event.start.hour() === 0 && event.start.minute() === 0) {
						element.find('.fc-time').addClass('hide');
					}
				},
				eventClick: function (calEvent, jsEvent, view) {
					if (selectCallback) {
						selectCallback(calendarView, calEvent.start, calEvent.end, calEvent.id, calEvent.sequenceId, calEvent.upcomingweek);
					}
				},
				select: function (start, end) {
					if (selectDayCallback) {
						var localStartDate = ConvertUtcToLocal(start);
						var localEndDate = ConvertUtcToLocal(end);
						selectDayCallback(calendarView, localStartDate, localEndDate, 0);
					}
				},
				views: {
					month: {
                      timeFormat: defaultTimeFormate
					}
				}
			});
		},
		createEvent: function (start, end, title, id, color, capacity) {
			var event = {
				id: id,
				start: start,
				end: end,
				sequenceId: id, /*load event sequenceid*/
				upcomingweek: title,   /*load event upcomingweek*/
				color: color,
				title: "Capacity: " + capacity
			};
			events.push(event);

			$(calendarPlaceholder).fullCalendar('renderEvent', event, true);
			$(calendarPlaceholder).fullCalendar('unselect');
			
		},
		loadEvents: function () {
			var self = this;
			$(calendarPlaceholder).fullCalendar("removeEvents");
			var selectedId = $("#SelectedZoneId").val();
			Get("admin/slot", "GetBookedSlotCalendarEvents", "id={0}&start={1}&end={2}".format(selectedId, calendarStartDate, calendarEndDate), function (data) {
				events = [];
				for (var i = 0; i < data.length; i++) {
					self.createEvent(
						self.formatDate(moment(data[i].Start)),
						self.formatDate(moment(data[i].End)),
						data[i].Title,
						data[i].Id,
						data[i].Color,
						data[i].Capacity);
				}
			});
		},
		removeEvent: function (id) {
			var event = calendarRepository.getEventById(events, id);
			var date = new Date(event.start).format(DateFormatPattern());
			$(calendarPlaceholder).fullCalendar('removeEvents', id);

			events = calendarRepository.removeEvent(events, event);

			var eventsInGivenDate = calendarRepository.getEventsByDate(events, date);
			var closedOrBusy = calendarRepository.getBusyOrClosedEvent(eventsInGivenDate);

			if (isNull(closedOrBusy)) {
				this.getCell(date).css('background', 'white');
			}
		},
		getCell: function (date) {
			return $('.fc-day[data-date="' + date + '"]');
		},
		formatDate: function (date) {
			return dateFormat(date, DateTimeFormatPattern());
		},
		getEvents: function () {
			return events;
		}
	}
});