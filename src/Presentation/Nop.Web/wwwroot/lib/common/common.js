/**
* Navigate to given url
*
*/
function Url(url) {
    window.location.href = url;
}

function Display(controller, action, id, outputDom, parent) {
	if (hideShow.Observe($(parent).attr('id'), "dealItemFreeTime-" + id)) {
		$.get("/{0}/{1}".format(controller, action), { id: id }, function (data) {
			if (data.success) {
			}
			else {
				$(outputDom + id).html(data);
			}
		});
	}
}

/**
* Post data from server
* @params key=value&key=value]
*
*/
function Post(controller, action, params, callback) {
	$.post("/{0}/{1}?{2}".format(controller, action, params), function (data) {
		if (data.success) {
		}
		else {
			callback(data);
		}
	});
}

/**
* Post data from server and block selector
* @params key=value&key=value]
*
*/
function BPost(controller, action, params, callback, selector, visibleUiBlock) {
	visibleUiBlock == false ? blockNone(selector) : blockDefault(selector);

	$.post("/{0}/{1}?{2}".format(controller, action, params), function (data) {
		if (data.success) { }
		else {
			$(selector).unblock();
			callback(data);
		}
	});
}

/**
* Get data from server
* @params key=value&key=value]
*
*/
function Get(controller, action, params, callback) {
	$.get("/{0}/{1}?{2}".format(controller, action, params), function (data) {
		if (data.success) { }
		else {
			callback(data);
		}
	});
}

/**
* Get data from server and block selector
* @params key=value&key=value]
*
*/
function BGet(controller, action, params, callback, selector, visibleUiBlock) {

	visibleUiBlock == false ? blockNone(selector) : blockDefault(selector);

	$.get("/{0}/{1}?{2}".format(controller, action, params), function (data) {
		if (data.success) { }
		else {
			$(selector).unblock();
			callback(data);
		}
	});
}

/**
* Display Modal Window
* @params id]
*
*/
function GetModal(id, message) {
	$("#" + id).modal('show');

	if (message) {
		$("#" + id + "-text").html(message);
	}
}

/**
* Move User to given Id
* @params id]
*
*/
function JumpTo(id) {
	$('html,body').animate({ scrollTop: $('#' + id).offset().top }, 'fast');
}

/**
* Move User to given Id
* @params id - dom Id
*         ref - reference
* 
*/
function Text(ref, id) {
	Post("WebsiteText", "Get", "reference=" + ref, function (data) {
		$(id).html(data);
	});
}

/**
* Block many request during post a submit
*
*/
function singlePost(formId, btnSubmitId) {
    $(function () {
        $(formId).submit(function () {
            if ($(formId).valid()) {
                $(btnSubmitId).attr('disabled', 'disabled');
            } else {
                $(btnSubmitId).removeAttr('disabled');
            }
        });
    });
}

/**
* Convert from UTC to local
*
*/
function ConvertUtcToLocal(dateObject) {
    var offset = new Date().getTimezoneOffset();
    var date = new Date(dateObject);

    date.setMinutes(date.getMinutes() + offset);

    return date;
}

/**
* Check if object is not null and undefined
*
*/
function isNull(obj) {
    return _.isNull(obj) || obj === undefined;
}

function isNullOrEmpy(str) {
    if (str) {
        return false;
    }

    return true;
}

/**
* Get date format
*
*/
function DateFormatPattern() {
    return "yyyy-mm-dd";
}

/**
* Get datetime format
*
*/
function DateTimeFormatPattern() {
    return "yyyy-mm-dd HH:MM";
}

function RefreshPopover() {
    $('[data-toggle="popover"]').popover({
        trigger: 'hover'
    });
}
