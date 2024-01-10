var bookedCalendarModule = (function () {

	var fullCalendarModule = fullCalendarBookedModulePackage();
	return {
		init: function () {
			console.log('calendar index Init');
			fullCalendarModule.init();
			fullCalendarModule.initSelectCallback(this.selectCalendarEventOrDay, this);
			fullCalendarModule.initSelectDayCallback(this.selectCalendarEventOrDay, this);
			

		},
		selectCalendarEventOrDay: function (self, start, end, id, sequenceId, upcomingweek) {
			var scope = self;
			var formattedStartDate = fullCalendarModule.formatDate(start);
			var formattedEndDate = fullCalendarModule.formatDate(end);
			if (sequenceId == null) /*check sequenceid is null*/ {
				sequenceId = 0;
			}
			else {
				sequenceIds = sequenceId;
			}
			if (upcomingweek == null) /*check upcomingweek is null*/ {
				upcomingweeks = false;
			}
			else {
				upcomingweeks = upcomingweek;
			}
			if (id == -1) {
				return;
			}

			modalModule.title("Landing...");
			modalModule.clearBody();
			modalModule.show();

			Get("admin/slot", "AddUnavailableCalendarEvents", "start={0}&end={1}&id={2}&upcomingweeks={3}&sequenceIds={4}".format(formattedStartDate, formattedEndDate, id, upcomingweeks, sequenceIds), function (data) {
				modalModule.title("Set unvailable date");
				modalModule.addBody(data);
				modalModule.initSendForm(self.onCreate, self, null);
				modalModule.initDeleteCallback(self.onDelete, self, null);

				if (id > 0) {
					modalModule.showDelete();
				} bookedReservationModal

				if (id > 0) {
					$(".btnSaveModalForm").hide();
				} else {
					$(".btnSaveModalForm").show();
					$(".btnRemoveBtnForm").hide();
				}
			});
		},
		/*controller when create response*/
		onCreate: function (response, self) {
			if (response.IsValid == false) {
				modalModule.error(response);
			} else {
				fullCalendarModule.loadEvents();
				modalModule.hide();
			}
		},
		/*controller when delete response*/
		onDelete: function (response, self) {
			if (response.IsValid == false) {
				modalModule.error(response);
			} else {
				fullCalendarModule.loadEvents();
				modalModule.hide();
			}
		}
	}
});