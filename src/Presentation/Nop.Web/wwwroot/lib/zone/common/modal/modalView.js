var modalViewModule = (function () {
  var _modalId = null;
  var _clickedThrottle = false;
  var _deleteCallback;
  var _btnRemoveForm = ".btnRemoveBtnForm";
  var _btnRemoveAllBtnForm = ".btnRemoveAllBtnForm";
  var _updateCallback;
  var _btnUpdateForm = ".btnUpdateBtnForm";

  return {
    init: function (modalId) {
      _modalId = modalId;
      console.log('init modalView module for ' + _modalId);

      $(".btnCloseModalForm, .modal-header .close").click(function () {
        //if (confirm("Are you sure, you want to leave the pop-up.")) {
        _clickedThrottle = false;
        $(_modalId).find('.btnSaveModalForm').unbind('click');
        $(_modalId).find(_btnRemoveForm).unbind('click');
        $(_modalId).find(_btnRemoveForm).hide();
        $(_modalId).find(_btnRemoveAllBtnForm).unbind('click');
        $(_modalId).find(_btnRemoveAllBtnForm).hide();
        //} else {
        //    return false;
        //}
      });
    },
    initSendForm: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find('.btnSaveModalForm').click(function () {
        if (_clickedThrottle == false) {
          self.sendForm(callback, that);
        }
        _clickedThrottle = true;
      });
    },
    //sendFormDelete call post method callback and validatecallback
    initDeleteCallback: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find(_btnRemoveForm).click(function () {

        if (confirm("Are you sure you want to delete this item?")) {
          if (_clickedThrottle == false) {
            self.sendFormDelete(callback, that);
            if (isNull(validateCallback)) {
              self.sendFormDelete(callback, that);
            } else {
              validateCallback(self.sendFormDelete, callback, that);
            }
          }
          _clickedThrottle = true;
          //self.action(id);
        }
      });

      _deleteCallback = callback;
    },
    initAllDeleteCallback: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find(_btnRemoveAllBtnForm).click(function () {

        if (confirm("Are you sure you want to delete this item?")) {
          if (_clickedThrottle == false) {
            self.sendFormAllDelete(callback, that);
            if (isNull(validateCallback)) {
              self.sendFormAllDelete(callback, that);
            } else {
              validateCallback(self.sendFormAllDelete, callback, that);
            }
          }
          _clickedThrottle = true;
          //self.action(id);
        }
      });

      _deleteCallback = callback;
    },
    initUpdateCallback: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find(_btnUpdateForm).click(function () {

        if (_clickedThrottle == false) {
          self.sendFormUpdate(callback, that);
          if (isNull(validateCallback)) {
            self.sendFormUpdate(callback, that);
          } else {
            validateCallback(self.sendFormDelete, callback, that);
          }
        }
        _clickedThrottle = true;
        //self.action(id);

      });

      _updateCallback = callback;
    },
    title: function (title) {
      $(_modalId).find('.modalHeader').html(title);
    },
    addBody: function (data) {
      $(_modalId).find('.modalBody').html(data);
    },
    clearBody: function () {
      $(_modalId).find('.modalBody').html('');
    },
    hideBody: function () {
      $(_modalId).find('.modalBody').hide();
    },
    hideSave: function () {
      $(_modalId).find('.btnSaveModalForm').hide();
    },
    showDelete: function () {
      $(_modalId).find(_btnRemoveForm).show();
      $(_modalId).find(_btnRemoveAllBtnForm).show();
    },
    hideDelete: function () {
      $(_modalId).find(_btnRemoveForm).hide();
      $(_modalId).find(_btnRemoveAllBtnForm).hide();
    },
    showUpdate: function () {
      $(_modalId).find(_btnUpdateForm).show();
    },
    hideUpdate: function () {
      $(_modalId).find(_btnUpdateForm).hide();
    },
    show: function () {
      $(_modalId).find('.modalBody').show();
      $(_modalId).find('.modalAlert').hide();
      $(_modalId).modal('show');
    },
    hide: function () {
      $(_modalId).find('.btnSaveModalForm').unbind('click');
      $(_modalId).find(_btnRemoveForm).unbind('click');
      $(_modalId).find(_btnRemoveForm).hide();
      $(_modalId).find(_btnRemoveAllBtnForm).unbind('click');
      $(_modalId).find(_btnRemoveAllBtnForm).hide();
      $(_modalId).find(_btnUpdateForm).unbind('click');
      $(_modalId).find(_btnUpdateForm).hide();
      $(_modalId).modal('hide');

    },
    sendForm: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: form.attr('action'),
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    //change method delete method with post method form serialize
    sendFormDelete: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: "/admin/slot/RemoveAvailableCalendarEvents",
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    sendFormAllDelete: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: "/admin/slot/RemoveAllAvailableCalendarEvents",
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    sendFormUpdate: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: "/admin/slot/UpdateAvailableCalendarEvents",
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    action: function (id) {
      _deleteCallback(id);
      _updateCallback(id);
      _clickedThrottle = false;
    },
    error: function (status) {
      $(_modalId).find('.modalAlert').html(status.Message);
      $(_modalId).find('.modalAlert').show();
    }
  };
});

var modalViewModuleForUnavailableCalendar = (function () {
  var _modalId = null;
  var _clickedThrottle = false;
  var _deleteCallback;
  var _btnRemoveForm = ".btnRemoveBtnForm";
  var _btnRemoveAllBtnForm = ".btnRemoveAllBtnForm";
  var _updateCallback;
  var _btnUpdateForm = ".btnUpdateBtnForm";

  return {
    init: function (modalId) {
      _modalId = modalId;
      console.log('init modalView module for ' + _modalId);

      $(".btnCloseModalForm, .modal-header .close").click(function () {
        /*if (confirm("Are you sure, you want to leave the pop-up.")) {*/
        _clickedThrottle = false;
        $(_modalId).find('.btnSaveModalForm').unbind('click');
        $(_modalId).find(_btnRemoveForm).unbind('click');
        $(_modalId).find(_btnRemoveForm).hide();
        $(_modalId).find(_btnRemoveAllBtnForm).unbind('click');
        $(_modalId).find(_btnRemoveAllBtnForm).hide();
        //} else {
        //    return false;
        //}
      });
    },
    initSendForm: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find('.btnSaveModalForm').click(function () {
        if (_clickedThrottle == false) {
          self.sendForm(callback, that);
        }
        _clickedThrottle = true;
      });
    },
    //sendFormDelete call post method callback and validatecallback	
    initDeleteCallback: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find(_btnRemoveForm).click(function () {

        if (confirm("Are you sure you want to delete this item?")) {
          if (_clickedThrottle == false) {
            self.sendFormDelete(callback, that);
            if (isNull(validateCallback)) {
              self.sendFormDelete(callback, that);
            } else {
              validateCallback(self.sendFormDelete, callback, that);
            }
          }
          _clickedThrottle = true;
          //self.action(id);	
        }
      });
      _deleteCallback = callback;
    },
    initAllDeleteCallback: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find(_btnRemoveAllBtnForm).click(function () {
        if (confirm("Are you sure you want to delete this item?")) {
          if (_clickedThrottle == false) {
            self.sendFormAllDelete(callback, that);
            if (isNull(validateCallback)) {
              self.sendFormAllDelete(callback, that);
            } else {
              validateCallback(self.sendFormAllDelete, callback, that);
            }
          }
          _clickedThrottle = true;
          //self.action(id);	
        }
      });
      _deleteCallback = callback;
    },
    initUpdateCallback: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find(_btnUpdateForm).click(function () {

        if (_clickedThrottle == false) {
          self.sendFormUpdate(callback, that);
          if (isNull(validateCallback)) {
            self.sendFormUpdate(callback, that);
          } else {
            validateCallback(self.sendFormDelete, callback, that);
          }
        }
        _clickedThrottle = true;
        //self.action(id);

      });

      _updateCallback = callback;
    },
    title: function (title) {
      $(_modalId).find('.modalHeader').html(title);
    },
    addBody: function (data) {
      $(_modalId).find('.modalBody').html(data);
    },
    clearBody: function () {
      $(_modalId).find('.modalBody').html('');
    },
    hideBody: function () {
      $(_modalId).find('.modalBody').hide();
    },
    hideSave: function () {
      $(_modalId).find('.btnSaveModalForm').hide();
    },
    showDelete: function () {
      $(_modalId).find(_btnRemoveForm).show();
      $(_modalId).find(_btnRemoveAllBtnForm).show();
    },
    hideDelete: function () {
      $(_modalId).find(_btnRemoveForm).hide();
      $(_modalId).find(_btnRemoveAllBtnForm).hide();
    },
    showUpdate: function () {
      $(_modalId).find(_btnUpdateForm).show();
    },
    hideUpdate: function () {
      $(_modalId).find(_btnUpdateForm).hide();
    },
    show: function () {
      $(_modalId).find('.modalBody').show();
      $(_modalId).find('.modalAlert').hide();
      $(_modalId).modal('show');
    },
    hide: function () {
      $(_modalId).find('.btnSaveModalForm').unbind('click');
      $(_modalId).find(_btnRemoveForm).unbind('click');
      $(_modalId).find(_btnRemoveForm).hide();
      $(_modalId).find(_btnRemoveAllBtnForm).unbind('click');
      $(_modalId).find(_btnRemoveAllBtnForm).hide();
      $(_modalId).find(_btnUpdateForm).unbind('click');
      $(_modalId).find(_btnUpdateForm).hide();
      $(_modalId).modal('hide');

    },
    sendForm: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: form.attr('action'),
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    //change method delete method with post method form serialize	
    sendFormDelete: function (callback, that) {
      var form = $(_modalId).find('form');
      event.preventDefault();
      form.validate();
      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: "/admin/slot/RemoveUnavailableCalendarEvents",
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    //change method delete method with post method form serialize
    sendFormAllDelete: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: "/admin/slot/RemoveAllUnavailableCalendarEvents",
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    sendFormUpdate: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: "/admin/slot/UpdateUnavailableCalendarEvents",
          data: form.serialize(),
          success: function (response) {
            if (response.result == false) {
              $(_modalId).find('.modalAlert').html(response.message);
              $(_modalId).find('.modalAlert').show();
              $('#EndDate').val(moment($('#EndDate').val(), "DD/MM/YYYY").format("MM/DD/YYYY"));
            }
            else {
              _clickedThrottle = false;
              callback(response, that);
            }
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    action: function (id) {
      _deleteCallback(id);
      _updateCallback(id);
      _clickedThrottle = false;
    },
    error: function (status) {
      $(_modalId).find('.modalAlert').html(status.Message);
      $(_modalId).find('.modalAlert').show();
    }
  };
});


var modalViewModulePickup = (function () {
  var _modalId = null;
  var _clickedThrottle = false;
  var _deleteCallback;
  var _btnRemoveForm = ".btnRemoveBtnForm";
  var _btnRemoveAllBtnForm = ".btnRemoveAllBtnForm";
  var _updateCallback;
  var _btnUpdateForm = ".btnUpdateBtnForm";

  return {
    init: function (modalId) {
      _modalId = modalId;
      console.log('init modalView module for ' + _modalId);

      $(".btnCloseModalForm, .modal-header .close").click(function () {
        /* if (confirm("Are you sure, you want to leave the pop-up.")) {*/
        _clickedThrottle = false;
        $(_modalId).find('.btnSaveModalForm').unbind('click');
        $(_modalId).find(_btnRemoveForm).unbind('click');
        $(_modalId).find(_btnRemoveForm).hide();
        $(_modalId).find(_btnRemoveAllBtnForm).unbind('click');
        $(_modalId).find(_btnRemoveAllBtnForm).hide();
        //} else {
        //  return false;
        //}
      });
    },
    initSendForm: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find('.btnSaveModalForm').click(function () {
        if (_clickedThrottle == false) {
          self.sendForm(callback, that);
        }
        _clickedThrottle = true;
      });
    },
    //sendFormDelete call post method callback and validatecallback
    initDeleteCallback: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find(_btnRemoveForm).click(function () {

        if (confirm("Are you sure you want to delete this item?")) {
          if (_clickedThrottle == false) {
            self.sendFormDelete(callback, that);
            if (isNull(validateCallback)) {
              self.sendFormDelete(callback, that);
            } else {
              validateCallback(self.sendFormDelete, callback, that);
            }
          }
          _clickedThrottle = true;
          //self.action(id);
        }
      });

      _deleteCallback = callback;
    },
    initAllDeleteCallback: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find(_btnRemoveAllBtnForm).click(function () {

        if (confirm("Are you sure you want to delete this item?")) {
          if (_clickedThrottle == false) {
            self.sendFormAllDelete(callback, that);
            if (isNull(validateCallback)) {
              self.sendFormAllDelete(callback, that);
            } else {
              validateCallback(self.sendFormAllDelete, callback, that);
            }
          }
          _clickedThrottle = true;
          //self.action(id);
        }
      });

      _deleteCallback = callback;
    },
    initUpdateCallback: function (callback, that, validateCallback) {
      var self = this;
      $(_modalId).find(_btnUpdateForm).click(function () {

        if (_clickedThrottle == false) {
          self.sendFormUpdate(callback, that);
          if (isNull(validateCallback)) {
            self.sendFormUpdate(callback, that);
          } else {
            validateCallback(self.sendFormDelete, callback, that);
          }
        }
        _clickedThrottle = true;
        //self.action(id);

      });

      _updateCallback = callback;
    },
    title: function (title) {
      $(_modalId).find('.modalHeader').html(title);
    },
    addBody: function (data) {
      $(_modalId).find('.modalBody').html(data);
    },
    clearBody: function () {
      $(_modalId).find('.modalBody').html('');
    },
    hideBody: function () {
      $(_modalId).find('.modalBody').hide();
    },
    hideSave: function () {
      $(_modalId).find('.btnSaveModalForm').hide();
    },
    showDelete: function () {
      $(_modalId).find(_btnRemoveForm).show();
      $(_modalId).find(_btnRemoveAllBtnForm).show();
    },
    hideDelete: function () {
      $(_modalId).find(_btnRemoveForm).hide();
      $(_modalId).find(_btnRemoveAllBtnForm).hide();
    },
    showUpdate: function () {
      $(_modalId).find(_btnUpdateForm).show();
    },
    hideUpdate: function () {
      $(_modalId).find(_btnUpdateForm).hide();
    },
    show: function () {
      $(_modalId).find('.modalBody').show();
      $(_modalId).find('.modalAlert').hide();
      $(_modalId).modal('show');
    },
    hide: function () {
      $(_modalId).find('.btnSaveModalForm').unbind('click');
      $(_modalId).find(_btnRemoveForm).unbind('click');
      $(_modalId).find(_btnRemoveForm).hide();
      $(_modalId).find(_btnRemoveAllBtnForm).unbind('click');
      $(_modalId).find(_btnRemoveAllBtnForm).hide();
      $(_modalId).find(_btnUpdateForm).unbind('click');
      $(_modalId).find(_btnUpdateForm).hide();
      $(_modalId).modal('hide');

    },
    sendForm: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: form.attr('action'),
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    //change method delete method with post method form serialize
    sendFormDelete: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: "/admin/pickupslot/RemoveAvailableCalendarEvents",
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    sendFormAllDelete: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: "/admin/pickupslot/RemoveAllAvailableCalendarEvents",
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    sendFormUpdate: function (callback, that) {
      var form = $(_modalId).find('form');

      event.preventDefault();
      form.validate();

      if (form.valid()) {
        $.ajax({
          type: "POST",
          url: "/admin/pickupslot/UpdateAvailableCalendarEvents",
          data: form.serialize(),
          success: function (response) {
            _clickedThrottle = false;
            callback(response, that);
          },
          error: function () {
            _clickedThrottle = false;
          }
        });
      } else {
        _clickedThrottle = false;
      }
    },
    action: function (id) {
      _deleteCallback(id);
      _updateCallback(id);
      _clickedThrottle = false;
    },
    error: function (status) {
      $(_modalId).find('.modalAlert').html(status.Message);
      $(_modalId).find('.modalAlert').show();
    }
  };
});



