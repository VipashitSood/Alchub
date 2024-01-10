var indexModule = (function () {

  var modal = "#reservationModal";
  var modalModule = modalViewModule();
  var fullCalendarModule = fullCalendarModulePackage();
  var zoneId;
  var sequenceIds;
  var isRecurring;
  var recurringType;
  var weekDays;
  var Capacity;
  var Price;
  var isUnavailable;
  var Name;
  return {
    init: function (id) {
      console.log('calendar index Init');
      zoneId = id;
      fullCalendarModule.init(id);
      fullCalendarModule.initSelectCallback(this.selectCalendarEventOrDay, this);
      fullCalendarModule.initSelectDayCallback(this.selectCalendarEventOrDay, this);
      modalModule.init(modal);

    },
    selectCalendarEventOrDay: function (self, start, end, id, sequenceId, isrecurring, recurringtype, weekdays, capacity, price, isunavilable, name) {
      console.log('selectCalendarEventOrDay');
      var scope = self;
      var formattedStartDate = moment(start).format('DD/MM/YYYY HH:mm:ss');
      var formattedEndDate = moment(end).format('DD/MM/YYYY HH:mm:ss');
      if (sequenceId == null) /*check sequenceid is null*/ {
        sequenceId = 0;
      }
      else {
        sequenceIds = sequenceId;
      }
      if (isrecurring == null) /*check isrecurring is null*/ {
        isRecurring = false;
      }
      else {
        isRecurring = isrecurring;
      }
      if (recurringType == null) /*check isrecurring is null*/ {
        recurringType = 1;
      }
      else {
        recurringType = recurringtype;
      }
      if (weekDays == null) /*check isrecurring is null*/ {
        weekDays = '';
      }
      else {
        weekDays = weekdays;
      }
      if (capacity == null) /*check capacity is null*/ {
        Capacity = 25;
      }
      else {
        Capacity = capacity;
      }
      if (price == null) /*check price is null*/ {
        Price = 0;
      }
      else {
        Price = price;
      }
      if (isunavilable == null) /*check isrecurring is null*/ {
        isUnavailable = true;
      }
      else {
        isUnavailable = isunavilable;
      }
      if (name == null) /*check isrecurring is null*/ {
        Name = '';
      }
      else {
        Name = name;
      }
      if (id == -1) {
        return;
      }

      modalModule.title("Landing...");
      modalModule.clearBody();
      modalModule.show();
      Get("admin/slot", "AddAvailableCalendarEvents", "start={0}&end={1}&id={2}&zoneId={3}&isRecurring={4}&recurringType={5}&weekdays={6}&sequenceIds={7}&Capacity={8}&Price={9}&isUnavailable={10}&Name={11}".format(formattedStartDate, formattedEndDate, id, zoneId, isRecurring, recurringType, weekDays, sequenceIds, Capacity, Price, isUnavailable, Name), function (data) {
        modalModule.title("Set Available Slot");
        modalModule.addBody(data);
        modalModule.initSendForm(self.onCreate, self, null);
        modalModule.initDeleteCallback(self.onDelete, self, null);
        modalModule.initAllDeleteCallback(self.onDelete, self, null);
        modalModule.initUpdateCallback(self.onUpdate, self, null);

        if (id > 0) {
          modalModule.title("Edit Available Slot")
          modalModule.showDelete();
          modalModule.showUpdate();
        }

        if (id > 0) {
          $(".btnSaveModalForm").hide();
        } else {
          $(".btnSaveModalForm").show();
          $(".btnRemoveBtnForm").hide();
          $(".btnRemoveAllBtnForm").hide();
          $(".btnUpdateBtnForm").hide();
        }
      });
    },
    /*controller when create response*/
    onCreate: function (response, self) {
      if (response.result == false) {
        modalModule.error(response.message);
        $(".modalAlert").text(response.message);
        $('#Start').val(moment($('#Start').val(), "MM/DD/YYYY HH:mm:ss").format("DD/MM/YYYY HH:mm:ss"));
        $('#End').val(moment($('#End').val(), "MM/DD/YYYY HH:mm:ss").format("DD/MM/YYYY HH:mm:ss"));
      }
      else {
        if (response.value != null) {
          var msg = confirm("This slot is overlapping with below mentioned slots. If you create this one then other slots will be deleted. Do you still want to proceed ahead? \n It will take approx 2-5 minutes to remove and create new slots \n" + response.value + "\n")
          if (msg == true) {
            var ids = response.Id;
            ids.split(',')
            var categoryIds = response.selectedcategories;
            categoryIds.split(',')
            var start = moment(response.start).format('DD/MM/YYYY HH:mm:ss');
            var end = moment(response.end).format('DD/MM/YYYY HH:mm:ss');
            var enddate = moment(response.enddate).format('DD/MM/YYYY HH:mm:ss');
            Get("admin/slot", "RemoveEvents", "data={0}&start={1}&end={2}&enddate={3}&zoneId={4}&isrecurring={5}&recurringtype={6}&weekdays={7}&capacity={8}&price={9}&selectedcategories={10}".format(ids, start, end, enddate, response.zoneid, response.isrecurring, response.recurringtype, response.weekdays, response.capacity, response.price, categoryIds), function (response) {
              if (response.IsValid == false) {
                modalModule.error(response);
              } else {
                window.location.reload();
              }
            });
          }
          else {
            //$(".btnCloseModalForm").trigger("click");
            window.location.reload(); //Another possiblity
          }
        }
        setInterval(function () {
          location.reload();
        }, 3000);
        fullCalendarModule.loadEvents();
        modalModule.hide();
        window.location.reload();
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
    },
    onUpdate: function (response, self) {
      if (response.IsValid == false) {
        modalModule.error(response);
      } else {
        fullCalendarModule.loadEvents();
        modalModule.hide();
      }
    }
  }
});

var indexModuleForUnavailableCalendar = (function () {

  var modal = "#unavailableCalendarModal";
  var modalModule = modalViewModuleForUnavailableCalendar();
  var fullCalendarModule = fullCalendarModulePackageForUnavailableCalendar();
  var zoneId;
  var sequenceIds;
  var isRecurring;
  var recurringType;
  var weekDays;
  var Capacity;
  var Price;
  var isUnavailable;
  var Name;
  return {
    init: function (id) {
      console.log('calendar index Init');
      zoneId = id;
      fullCalendarModule.init(id);
      fullCalendarModule.initSelectCallback(this.selectCalendarEventOrDay, this);
      fullCalendarModule.initSelectDayCallback(this.selectCalendarEventOrDay, this);
      modalModule.init(modal);

    },
    selectCalendarEventOrDay: function (self, start, end, id, sequenceId, isrecurring, recurringtype, weekdays, capacity, price, isunavilable, name) {
      console.log('selectCalendarEventOrDay');
      var scope = self;
      var formattedStartDate = moment(start).format('DD/MM/YYYY HH:mm:ss');
      var formattedEndDate = moment(end).format('DD/MM/YYYY HH:mm:ss');
      if (sequenceId == null) /*check sequenceid is null*/ {
        sequenceId = 0;
      }
      else {
        sequenceIds = sequenceId;
      }
      if (isrecurring == null) /*check isrecurring is null*/ {
        isRecurring = false;
      }
      else {
        isRecurring = isrecurring;
      }
      if (recurringType == null) /*check isrecurring is null*/ {
        recurringType = 1;
      }
      else {
        recurringType = recurringtype;
      }
      if (weekDays == null) /*check isrecurring is null*/ {
        weekDays = '';
      }
      else {
        weekDays = weekdays;
      }
      if (capacity == null) /*check capacity is null*/ {
        Capacity = 25;
      }
      else {
        Capacity = capacity;
      }
      if (price == null) /*check price is null*/ {
        Price = 0;
      }
      else {
        Price = price;
      }
      if (isunavilable == null) /*check isrecurring is null*/ {
        isUnavailable = true;
      }
      else {
        isUnavailable = isunavilable;
      }
      if (name == null) /*check isrecurring is null*/ {
        Name = '';
      }
      else {
        Name = name;
      }
      if (id == -1) {
        return;
      }

      modalModule.title("Landing...");
      modalModule.clearBody();
      modalModule.show();
      Get("admin/slot", "AddUnavailableCalendarEvents", "start={0}&end={1}&id={2}&isRecurring={3}&recurringType={4}&weekdays={5}&sequenceIds={6}&Name={7}".format(formattedStartDate, formattedEndDate, id, isRecurring, recurringType, weekDays, sequenceIds, Name), function (data) {
        modalModule.title("Set UnAvailable Slot");
        modalModule.addBody(data);
        modalModule.initSendForm(self.onCreate, self, null);
        modalModule.initDeleteCallback(self.onDelete, self, null);
        modalModule.initAllDeleteCallback(self.onDelete, self, null);
        modalModule.initUpdateCallback(self.onUpdate, self, null);

        if (id > 0) {
          modalModule.title("Edit UnAvailable Slot")
          modalModule.showDelete();
          modalModule.showUpdate();
        }

        if (id > 0) {
          $(".btnSaveModalForm").hide();
        } else {
          $(".btnSaveModalForm").show();
          $(".btnRemoveBtnForm").hide();
          $(".btnRemoveAllBtnForm").hide();
          $(".btnUpdateBtnForm").hide();
        }
      });
    },
    /*controller when create response*/
    onCreate: function (response, self) {
      if (response.result == false) {
        modalModule.error(response.message);
        $(".modalAlert").text(response.message);
        $('#Start').val(moment($('#Start').val(), "MM/DD/YYYY HH:mm:ss").format("DD/MM/YYYY HH:mm:ss"));
        $('#End').val(moment($('#End').val(), "MM/DD/YYYY HH:mm:ss").format("DD/MM/YYYY HH:mm:ss"));
      }
      else {
        if (response.value != null) {
          var msg = confirm("This slot is overlapping with below mentioned slots. If you create this one then other slots will be deleted. Do you still want to proceed ahead? \n It will take approx 2-5 minutes to remove and create new slots \n" + response.value + "\n" )
          if (msg == true) {
            var ids = response.Id;
            ids.split(',')
            var start = moment(response.start).format('DD/MM/YYYY HH:mm:ss');
            var end = moment(response.end).format('DD/MM/YYYY HH:mm:ss');
            var enddate = moment(response.enddate).format('DD/MM/YYYY HH:mm:ss');
            Get("admin/slot", "RemoveUnavailableCalendarEvents", "data={0}&start={1}&end={2}&enddate={3}&isrecurring={4}&recurringtype={5}&weekdays={6}".format(ids, start, end, enddate, response.isrecurring, response.recurringtype, response.weekdays), function (response) {
              if (response.IsValid == false) {
                modalModule.error(response);
              } else {
                window.location.reload();
              }
            });
          }
          else {
            //$(".btnCloseModalForm").trigger("click");
            window.location.reload(); //Another possiblity
          }
        }
        fullCalendarModule.loadEvents();
        modalModule.hide();
        window.location.reload();
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
    },
    onUpdate: function (response, self) {
      if (response.IsValid == false) {
        modalModule.error(response);
      } else {
        fullCalendarModule.loadEvents();
        modalModule.hide();
      }
    }
  }
});


var indexModulePickup = (function () {

  var modal = "#reservationModalPickup";
  var modalModule = modalViewModulePickup();
  var fullCalendarModule = fullCalendarModulePackagePickup();
  var zoneId;
  var sequenceIds;
  var isRecurring;
  var recurringType;
  var weekDays;
  var Capacity;
  var Price;
  var isUnavailable;
  var Name;
  return {
    init: function (id) {
      console.log('calendar index Init');
      zoneId = id;
      fullCalendarModule.init(id);
      fullCalendarModule.initSelectCallback(this.selectCalendarEventOrDay, this);
      fullCalendarModule.initSelectDayCallback(this.selectCalendarEventOrDay, this);
      modalModule.init(modal);

    },
    selectCalendarEventOrDay: function (self, start, end, id, sequenceId, isrecurring, recurringtype, weekdays, capacity, price, isunavilable, name) {
      console.log('selectCalendarEventOrDay');
      var scope = self;
      var formattedStartDate = moment(start).format('DD/MM/YYYY HH:mm:ss');
      var formattedEndDate = moment(end).format('DD/MM/YYYY HH:mm:ss');
      if (sequenceId == null) /*check sequenceid is null*/ {
        sequenceId = 0;
      }
      else {
        sequenceIds = sequenceId;
      }
      if (isrecurring == null) /*check isrecurring is null*/ {
        isRecurring = false;
      }
      else {
        isRecurring = isrecurring;
      }
      if (recurringType == null) /*check isrecurring is null*/ {
        recurringType = 1;
      }
      else {
        recurringType = recurringtype;
      }
      if (weekDays == null) /*check isrecurring is null*/ {
        weekDays = '';
      }
      else {
        weekDays = weekdays;
      }
      if (capacity == null) /*check capacity is null*/ {
        Capacity = 25;
      }
      else {
        Capacity = capacity;
      }
      if (price == null) /*check price is null*/ {
        Price = 0;
      }
      else {
        Price = price;
      }
      if (isunavilable == null) /*check isrecurring is null*/ {
        isUnavailable = true;
      }
      else {
        isUnavailable = isunavilable;
      }
      if (name == null) /*check isrecurring is null*/ {
        Name = '';
      }
      else {
        Name = name;
      }
      if (id == -1) {
        return;
      }

      modalModule.title("Landing...");
      modalModule.clearBody();
      modalModule.show();
      Get("admin/pickupslot", "AddAvailableCalendarEvents", "start={0}&end={1}&id={2}&zoneId={3}&isRecurring={4}&recurringType={5}&weekdays={6}&sequenceIds={7}&Capacity={8}&Price={9}&isUnavailable={10}&Name={11}".format(formattedStartDate, formattedEndDate, id, zoneId, isRecurring, recurringType, weekDays, sequenceIds, Capacity, Price, isUnavailable, Name), function (data) {
        modalModule.title("Set Available Slot");
        modalModule.addBody(data);
        modalModule.initSendForm(self.onCreate, self, null);
        modalModule.initDeleteCallback(self.onDelete, self, null);
        modalModule.initAllDeleteCallback(self.onDelete, self, null);
        modalModule.initUpdateCallback(self.onUpdate, self, null);

        if (id > 0) {
          modalModule.title("Edit Available Slot")
          modalModule.showDelete();
          modalModule.showUpdate();
        }

        if (id > 0) {
          $(".btnSaveModalForm").hide();
        } else {
          $(".btnSaveModalForm").show();
          $(".btnRemoveBtnForm").hide();
          $(".btnRemoveAllBtnForm").hide();
          $(".btnUpdateBtnForm").hide();
        }
      });
    },
    /*controller when create response*/
    onCreate: function (response, self) {
      if (response.result == false) {
        modalModule.error(response.message);
        $(".modalAlert").text(response.message);
        $('#Start').val(moment($('#Start').val(), "MM/DD/YYYY HH:mm:ss").format("DD/MM/YYYY HH:mm:ss"));
        $('#End').val(moment($('#End').val(), "MM/DD/YYYY HH:mm:ss").format("DD/MM/YYYY HH:mm:ss"));
      }
      else {
        if (response.value != null) {
          var msg = confirm("This slot is overlapping with below mentioned slots. If you create this one then other slots will be deleted. Do you still want to proceed ahead? \n It will take approx 2-5 minutes to remove and create new slots \n" + response.value + "\n")
          if (msg == true) {
            var ids = response.Id;
            ids.split(',')
            var categoryIds = response.selectedcategories;
            categoryIds.split(',')
            var start = moment(response.start).format('DD/MM/YYYY HH:mm:ss');
            var end = moment(response.end).format('DD/MM/YYYY HH:mm:ss');
            var enddate = moment(response.enddate).format('DD/MM/YYYY HH:mm:ss');
            Get("admin/pickupslot", "RemoveEvents", "data={0}&start={1}&end={2}&enddate={3}&zoneId={4}&isrecurring={5}&recurringtype={6}&weekdays={7}&capacity={8}&price={9}&selectedcategories={10}".format(ids, start, end, enddate, response.zoneid, response.isrecurring, response.recurringtype, response.weekdays, response.capacity, response.price, categoryIds), function (response) {
              if (response.IsValid == false) {
                modalModule.error(response);
              }
              else
              {
                
                window.location.reload();
              }
            });
          }
          else {
            //$(".btnCloseModalForm").trigger("click");
            window.location.reload(); //Another possiblity
          }
        }
        setInterval(function () {
          location.reload();
        }, 3000);
        fullCalendarModule.loadEvents();
        modalModule.hide();
        window.location.reload();
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
    },
    onUpdate: function (response, self) {
      if (response.IsValid == false) {
        modalModule.error(response);
      } else {
        fullCalendarModule.loadEvents();
        modalModule.hide();
      }
    }
  }
});
