var defaultTimeFormate = 'h(:mm)a';
var fullCalendarModulePackage = (function () {

  var calendarPlaceholder = "#calendar";
  var selectCallback;
  var selectDayCallback;
  var calendarView;
  var calendarStartDate;
  var calendarEndDate;
  var calendarBusyDay = "#F1A94E";
  var calendarClosedDay = "#BEB9B5";  /*changes done by tmotions*/
  var events = [];
  var calendarRepository = calendarRepositoryModule();
  var modalModule = modalViewModule();
  var zoneId;
  return {
    init: function (id) {
      console.log('full Calendar Init');
      zoneId = id;
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
        minTime: "00:00:00",  /* min time changes done by tmotions*/
        maxTime: "24:00:00",  /* max time changes done by tmotions*/
        height: 625,
        slotDuration: '00:60:00',
        contentHeight: 'auto',
        slotLabelFormat: defaultTimeFormate,  /*slot label format changes done by tmotions*/
        timeFormat: defaultTimeFormate,  /*time format chnage changes done by tmotions*/
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

          if (event.id == -1) {
            element.css('background-color', 'green');
            element.css('border-color', 'green');
          }
          if (event.start.hour() === 0 && event.start.minute() === 0) {
            element.find('.fc-time').addClass('hide');
          }
        },
        eventClick: function (calEvent, jsEvent, view) {
          if (selectCallback) {
            selectCallback(calendarView, calEvent.start, calEvent.end, calEvent.id, calEvent.sequenceId, calEvent.isrecurring, calEvent.recurringtype, calEvent.weekdays, calEvent.capacity, calEvent.price, calEvent.isunavailable, calEvent.name);
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
    createEvent: function (start, end, title, id, color, sequenceId, isRecurring, recurringType, weekDays, capacity, price, isUnavailable, name) {
      var event = {
        id: id,
        start: start,
        end: end,
        title: "$" + price,
        sequenceId: sequenceId, /*load event sequenceid*/
        isrecurring: isRecurring,   /*load event isRecurring*/
        recurringtype: recurringType,
        weekdays: weekDays,
        capacity: capacity, /*load event capacity*/
        price: price,  /*load event price*/
        isunavailable: isUnavailable,
        name: name,
        color: color
      };

      events.push(event);

      $(calendarPlaceholder).fullCalendar('renderEvent', event, true);
      $(calendarPlaceholder).fullCalendar('unselect');
    },
    loadEvents: function () {
      var self = this;
      $(calendarPlaceholder).fullCalendar("removeEvents");
      /*changes done by Tmotions*/
      Get("admin/slot", "GetAvailableCalendarEvents", "start={0}&end={1}&zoneId={2}".format(calendarStartDate, calendarEndDate, zoneId), function (data) {
        events = [];
        for (var i = 0; i < data.length; i++) {
          self.createEvent(
            self.formatDate(moment(data[i].Start)),
            self.formatDate(moment(data[i].End)),
            data[i].Title,
            data[i].Id,
            data[i].Color,
            data[i].SequenceId,
            data[i].IsRecurring,
            data[i].RecurringType,
            data[i].WeekDays,
            data[i].Capacity,
            data[i].Price,
            data[i].IsUnavailable,
            data[i].Name
          );
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

var fullCalendarModulePackageForUnavailableCalendar = (function () {

  var calendarPlaceholder = "#calendar";
  var selectCallback;
  var selectDayCallback;
  var calendarView;
  var calendarStartDate;
  var calendarEndDate;
  var calendarBusyDay = "#F1A94E";
  var calendarClosedDay = "#BEB9B5";  /*changes done by tmotions*/
  var events = [];
  var calendarRepository = calendarRepositoryModule();
  var modalModule = modalViewModuleForUnavailableCalendar();
  var zoneId;
  return {
    init: function (id) {
      console.log('full Calendar Init');
      zoneId = id;
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
        minTime: "00:00:00",  /* min time changes done by tmotions*/
        maxTime: "24:00:00",  /* max time changes done by tmotions*/
        height: 625,
        slotDuration: '00:60:00',
        contentHeight: 'auto',
        slotLabelFormat: defaultTimeFormate,  /*slot label format changes done by tmotions*/
        timeFormat: defaultTimeFormate,  /*time format chnage changes done by tmotions*/
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

          if (event.id == -1) {
            element.css('background-color', 'green');
            element.css('border-color', 'green');
          }
          if (event.start.hour() === 0 && event.start.minute() === 0) {
            element.find('.fc-time').addClass('hide');
          }
        },
        eventClick: function (calEvent, jsEvent, view) {
          if (selectCallback) {
            selectCallback(calendarView, calEvent.start, calEvent.end, calEvent.id, calEvent.sequenceId, calEvent.isrecurring, calEvent.recurringtype, calEvent.weekdays, calEvent.capacity, calEvent.price, calEvent.isunavailable, calEvent.name);
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
    createEvent: function (start, end, title, id, color, sequenceId, isRecurring, recurringType, weekDays, capacity, price, isUnavailable, name) {
      var event = {
        id: id,
        start: start,
        end: end,
        title: "$" + price,
        sequenceId: sequenceId, /*load event sequenceid*/
        isrecurring: isRecurring,   /*load event isRecurring*/
        recurringtype: recurringType,
        weekdays: weekDays,
        capacity: capacity, /*load event capacity*/
        price: price,  /*load event price*/
        isunavailable: isUnavailable,
        name: name,
        color: color
      };

      events.push(event);

      $(calendarPlaceholder).fullCalendar('renderEvent', event, true);
      $(calendarPlaceholder).fullCalendar('unselect');
    },
    loadEvents: function () {
      var self = this;
      $(calendarPlaceholder).fullCalendar("removeEvents");
      /*changes done by Tmotions*/
      Get("admin/slot", "GetUnavailableCalendarEvents", "start={0}&end={1}&zoneId={2}".format(calendarStartDate, calendarEndDate, zoneId), function (data) {
        events = [];
        for (var i = 0; i < data.length; i++) {
          self.createEvent(
            self.formatDate(moment(data[i].Start)),
            self.formatDate(moment(data[i].End)),
            data[i].Title,
            data[i].Id,
            data[i].Color,
            data[i].SequenceId,
            data[i].IsRecurring,
            data[i].RecurringType,
            data[i].WeekDays,
            data[i].Capacity,
            data[i].Price,
            data[i].IsUnavailable,
            data[i].Name
          );
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


var fullCalendarModulePackagePickup = (function () {

  var calendarPlaceholder = "#calendar";
  var selectCallback;
  var selectDayCallback;
  var calendarView;
  var calendarStartDate;
  var calendarEndDate;
  var calendarBusyDay = "#F1A94E";
  var calendarClosedDay = "#BEB9B5";  /*changes done by tmotions*/
  var events = [];
  var calendarRepository = calendarRepositoryModule();
  var modalModule = modalViewModule();
  var zoneId;
  return {
    init: function (id) {
      console.log('full Calendar Init');
      zoneId = id;
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
        minTime: "00:00:00",  /* min time changes done by tmotions*/
        maxTime: "24:00:00",  /* max time changes done by tmotions*/
        height: 625,
        slotDuration: '00:60:00',
        contentHeight: 'auto',
        slotLabelFormat: defaultTimeFormate,  /*slot label format changes done by tmotions*/
        timeFormat: defaultTimeFormate,  /*time format chnage changes done by tmotions*/
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

          if (event.id == -1) {
            element.css('background-color', 'green');
            element.css('border-color', 'green');
          }
          if (event.start.hour() === 0 && event.start.minute() === 0) {
            element.find('.fc-time').addClass('hide');
          }
        },
        eventClick: function (calEvent, jsEvent, view) {
          if (selectCallback) {
            selectCallback(calendarView, calEvent.start, calEvent.end, calEvent.id, calEvent.sequenceId, calEvent.isrecurring, calEvent.recurringtype, calEvent.weekdays, calEvent.capacity, calEvent.price, calEvent.isunavailable, calEvent.name);
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
    createEvent: function (start, end, title, id, color, sequenceId, isRecurring, recurringType, weekDays, capacity, price, isUnavailable, name) {
      var event = {
        id: id,
        start: start,
        end: end,
        title: "$" + price,
        sequenceId: sequenceId, /*load event sequenceid*/
        isrecurring: isRecurring,   /*load event isRecurring*/
        recurringtype: recurringType,
        weekdays: weekDays,
        capacity: capacity, /*load event capacity*/
        price: price,  /*load event price*/
        isunavailable: isUnavailable,
        name: name,
        color: color
      };

      events.push(event);

      $(calendarPlaceholder).fullCalendar('renderEvent', event, true);
      $(calendarPlaceholder).fullCalendar('unselect');
    },
    loadEvents: function () {
      var self = this;
      $(calendarPlaceholder).fullCalendar("removeEvents");
      /*changes done by Tmotions*/
      Get("admin/pickupslot", "GetAvailableCalendarEvents", "start={0}&end={1}&zoneId={2}".format(calendarStartDate, calendarEndDate, zoneId), function (data) {
        events = [];
        for (var i = 0; i < data.length; i++) {
          self.createEvent(
            self.formatDate(moment(data[i].Start)),
            self.formatDate(moment(data[i].End)),
            data[i].Title,
            data[i].Id,
            data[i].Color,
            data[i].SequenceId,
            data[i].IsRecurring,
            data[i].RecurringType,
            data[i].WeekDays,
            data[i].Capacity,
            data[i].Price,
            data[i].IsUnavailable,
            data[i].Name
          );
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
