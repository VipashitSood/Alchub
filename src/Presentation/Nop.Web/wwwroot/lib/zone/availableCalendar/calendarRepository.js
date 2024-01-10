var calendarRepositoryModule = (function () {

    return {
        getEventById: function (events, id) {
            var event = Enumerable.From(events)
                .FirstOrDefault(null, function (x) {
                    return x.id === id;
                });

            return event;
        },
        getEventsByDate: function (events, startDate) {
            var startDate = new Date(startDate).format(DateFormatPattern());

            var resultEvents = Enumerable.From(events)
                .Where(function (x) {
                    var startFormat = new Date(x.start).format(DateFormatPattern());
                    return startFormat === startDate;
                });

            return resultEvents.ToArray();
        },
        getBusyOrClosedEvent: function(events) {
            var event = Enumerable.From(events)
                .FirstOrDefault(null, function (x) {
                return x.type === 2 || x.type === 1;
            });

            return event;
        },
        removeEvent: function (events, eventToRemove) {
            var filterEvents = _.filter(events, function (item) {
                return item.id !== eventToRemove.id;
            });

            return filterEvents;
        }

    }
});