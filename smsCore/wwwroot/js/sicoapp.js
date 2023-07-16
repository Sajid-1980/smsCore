function setCookie(cname, cvalue, exdays) {
    const d = new Date();
    if (!exdays)
        exdays = 30;
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function getid(id) {
    return id.toString() + "   ";
}

function sno(i) {
    return +i + 1;
}
function sweetalert(title, message, icon) {
    window.Swal.fire({
        title: title,
        icon: icon,
        html: message,
        showCloseButton: true
    });

}
var ree = null;
var subs = null;
function Navigate(url) {
    if (subs) {
        if (subs.readyState != 4)
            subs.abort();
    }
    $('#page-content-holder').html('<div style="margin: auto;display: block;" class="spinner-grow" role="status"><span class="sr-only">Loading...</span></div>');
    subs = $.get(url, function (response, v, r) {
        ree = r;
        if (response == '') { window.location.href = '/Account/Login'; return false; }
        $('#page-content-holder').html(response);
    }).error(function (er, vr, cr) {
        $('#page-content-holder').html(`<h3 class="alert alert-danger">${er.statusText}</h3>`);
    });


    if ("undefined" !== typeof history.pushState) {
        history.pushState({ page: window.$(this).text() }, window.$(this).text(), url);
    } else {
        window.location.assign(url);
    }
    return false;
}



function block(id) {
    $(id).block({
        message: '<i class="fa fa-spin fa-spinner"></i>',
        overlayCSS: {
            backgroundColor: '#000',
            opacity: 0.3,
            cursor: 'wait',
            'box-shadow': '0 0 0 1px #ddd'
        },
        css: {
            border: 0,
            padding: 0,
            backgroundColor: 'none'
        }
    });
}


function sweetalertWithUrl(title, url,isdelete=false, successCallBack) {
    Swal.fire({
        title: title,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes',
        showLoaderOnConfirm: true,
        preConfirm: () => {
            return fetch(url, {method:isdelete?"post":"get"})
                .then(response => {
                    if (!response.ok) {
                        throw new Error(response.statusText);
                    }
                    return response.json();
                })
                .catch(error => {
                    Swal.showValidationMessage(
                        `Request failed: ${error}`
                    );
                });
        },
        allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {
        console.log('delete response', result);
        if (result.value) {
            if (result.value.message == 'success' || result.value===true) {
                Swal.fire({
                    title: isdelete?'Record has been deleted successfully': 'Record has been modified successfully.',
                    icon: 'info'
                });
                if (successCallBack)
                    successCallBack();
                if (isdelete)
                    if (typeof reloadGridData !== 'undefined' && typeof reloadGridData === 'function')
                    reloadGridData();
            } else {
                Swal.fire({
                    title: 'An error occured.',
                    message: result.value.message,
                    icon: 'error'
                });
            }
        }
    });
}

function Confirm(title, showDenyButton, showCancelButton, confirmButtonText, denyButtonText, confirmCallBack, denyCallBack) {
    Swal.fire({
        title: title,
        showDenyButton: showDenyButton,
        showCancelButton: showCancelButton,
        confirmButtonText: confirmButtonText,
        denyButtonText: denyButtonText,
    }).then((result) => {
        /* Read more about isConfirmed, isDenied below */
        if (result.isConfirmed) {
            if (confirmCallBack)
                confirmCallBack();
        } else if (result.isDenied) {
            if (denyCallBack)
                denyCallBack();
        }
    })
}
function ConfirmNavigate(url,title,  showDenyButton=true, showCancelButton=true, confirmButtonText="Ok", denyButtonText="Cancel") {
    Swal.fire({
        title: title,
        showDenyButton: showDenyButton,
        showCancelButton: showCancelButton,
        confirmButtonText: confirmButtonText,
        denyButtonText: denyButtonText,
    }).then((result) => {
        /* Read more about isConfirmed, isDenied below */
        if (result.isConfirmed) {
            Navigate(url)
        } else if (result.isDenied) {

        }
    })
}
//*** This code is copyright 2002-2016 by Gavin Kistner, !@phrogz.net
//*** It is covered under the license viewable at http://phrogz.net/JS/_ReuseLicense.txt
//*** Reuse or modification is free provided you abide by the terms of that license.
//*** (Including the first two lines above in your source code satisfies the conditions.)

// Include this code (with notice above ;) in your library; read below for how to use it.

Date.prototype.customFormat = function(formatString) {
    var YYYY, YY, MMMM, MMM, MM, M, DDDD, DDD, DD, D, hhhh, hhh, hh, h, mm, m, ss, s, ampm, AMPM, dMod, th;
    var dateObject = this;
    YY = ((YYYY = dateObject.getFullYear()) + "").slice(-2);
    MM = (M = dateObject.getMonth() + 1) < 10 ? ("0" + M) : M;
    MMM = (MMMM =
    [
        "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November",
        "December"
    ][M - 1]).substring(0, 3);
    DD = (D = dateObject.getDate()) < 10 ? ("0" + D) : D;
    DDD = (DDDD = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"][dateObject.getDay()])
        .substring(0, 3);
    th = (D >= 10 && D <= 20) ? "th" : ((dMod = D % 10) == 1) ? "st" : (dMod == 2) ? "nd" : (dMod == 3) ? "rd" : "th";
    formatString = formatString.replace("#YYYY#", YYYY).replace("#YY#", YY).replace("#MMMM#", MMMM)
        .replace("#MMM#", MMM).replace("#MM#", MM).replace("#M#", M).replace("#DDDD#", DDDD).replace("#DDD#", DDD)
        .replace("#DD#", DD).replace("#D#", D).replace("#th#", th);

    h = (hhh = dateObject.getHours());
    if (h == 0) h = 24;
    if (h > 12) h -= 12;
    hh = h < 10 ? ("0" + h) : h;
    hhhh = hhh < 10 ? ("0" + hhh) : hhh;
    AMPM = (ampm = hhh < 12 ? "am" : "pm").toUpperCase();
    mm = (m = dateObject.getMinutes()) < 10 ? ("0" + m) : m;
    ss = (s = dateObject.getSeconds()) < 10 ? ("0" + s) : s;
    return formatString.replace("#hhhh#", hhhh).replace("#hhh#", hhh).replace("#hh#", hh).replace("#h#", h)
        .replace("#mm#", mm).replace("#m#", m).replace("#ss#", ss).replace("#s#", s).replace("#ampm#", ampm)
        .replace("#AMPM#", AMPM);
};

/****************************************************************************

myDateObject.customFormat(formatString)
-------------------------------------------------
arguments:
	myDateObject - a JavaScript Date object
	formatString - a string containing one or more tokens to be replaced
	

returns:
	a new string based on formatString, with all valid tokens replaced with
	values from dateObject
	

description:
	Use customFormat(...) to format a Date object in anyway you like.
	Any token (from the list below) gets replaced with the corresponding value.
	

	token:     description:             example:
	#YYYY#     4-digit year             1999
	#YY#       2-digit year             99
	#MMMM#     full month name          February
	#MMM#      3-letter month name      Feb
	#MM#       2-digit month number     02
	#M#        month number             2
	#DDDD#     full weekday name        Wednesday
	#DDD#      3-letter weekday name    Wed
	#DD#       2-digit day number       09
	#D#        day number               9
	#th#       day ordinal suffix       nd
	#hhhh#     2 digit military hour    17
	#hhh#      military/24-based hour   17
	#hh#       2-digit hour             05
	#h#        hour                     5
	#mm#       2-digit minute           07
	#m#        minute                   7
	#ss#       2-digit second           09
	#s#        second                   9
	#ampm#     "am" or "pm"             pm
	#AMPM#     "AM" or "PM"             PM
	

notes:
	If you want the current date and time, use "new Date()".
	e.g.
	var curDate = new Date();
	curDate.customFormat("#DD#/#MM#/#YYYY#");
	

	If you want all-lowercase or all-uppercase versions of the weekday/month,
	use the toLowerCase() or toUpperCase() methods of the resulting string.
	e.g.
	curDate.customFormat("#DDDD#, #MMMM# #D#, #YYYY#").toLowerCase();

	If you use the same format in many places in your code, I suggest creating
	a wrapper function to this one, e.g.:
	Date.prototype.myDate=function(){
		return this.customFormat("#D#-#MMM#-#YYYY#").toLowerCase();
	}
	Date.prototype.myTime=function(){
		return this.customFormat("#h#:#mm##ampm#");
	}

	var now = new Date();
	alert(now.myDate());

	
examples:
	var now = new Date();
	var myString = now.customFormat("#YYYY#-#MM#-#DD#"); 
	alert(myString);
	-->1999-11-19
	
	var myString = now.customFormat("#DDD#, #MMM#-#D#-#YY#"); 
	alert(myString);
	-->Fri, Nov-19-99
	
	var myString = now.customFormat("#h#:#mm# #ampm#"); 
	alert(myString);
	-->4:34 pm

	var myString = now.customFormat("#hhh#:#mm#:#ss#"); 
	alert(myString);
	-->16:34:26

	var myString = now.customFormat("#DDDD#, #MMMM# #D##th#, #YYYY# @ #h#:#mm# #ampm#"); 
	alert(myString);
	-->Friday, November 19th, 1999 @ 4:34 pm
	
****************************************************************************/



function popup(url, title) {
    if (title == "undefined" || title == "") {
        title = "Sico Schools";
    }

    BootstrapDialog.show({
        title: title,
        message: $("#bootstrap-diallog").html(loaderHtml).load(url)
    });
}

$.fn.disable = function() {
    return this.each(function() {
        if (typeof this.disabled != "undefined") this.disabled = true;
    });
};

$.fn.enable = function() {
    return this.each(function() {
        if (typeof this.disabled != "undefined") this.disabled = false;
    });
};

function begin() {
    if ($(this).find(".nosubmit").length > 0) {
        alert('error found')

        $([document.documentElement, document.body]).animate({
                scrollTop: $(".nosubmit:first").offset().top - 50
            },
            500);
        return false;
    }
    $(this).find('button[type="submit"]').disable();
}

function isJSON(something) {
    if (typeof something != 'string')
        something = JSON.stringify(something);

    try {
        JSON.parse(something);
        return true;
    } catch (e) {
        return false;
    }
}

function completed(data) {
    var close = '<a class="close" style="color:red; top: 4px;position: absolute;right: 3px; font-size:1.3rem" onclick="clearMessage();"><i class="fas fa-times-circle"></i></a>';
    $(this).find('button[type="submit"]').enable();
    if (!isJSON(data.responseText)) {
        $(this).find("#message").html('<span class="float-left alert alert-danger w-100">' +
            data.responseText +
            close);

        if (data.responseText == "success" || data.responseText.startsWith('Success:')) {
            //alert('Information added to database successfully.')
            if (data.responseText == "success")
                $(this).find("#message").html('<span class="float-left alert alert-info w-100">' +
                    "Information added to database successfully." +
                    '<a class="close" style="top: 4px;position: absolute;right: 7px;" onclick="clearMessage();"><i class="fa fa-close"></i></a></span>');
            else
                if (data.responseText.startsWith('Success:'))
                    $(this).find("#message").html('<span class="float-left alert alert-info w-100">' +
                        data.responseText +
                        '<a class="close" style="top: 4px;position: absolute;right: 7px;" onclick="clearMessage();"><i class="fa fa-close"></i></a></span>');
            var form = $(this).find("#message").closest("form");
            if (form.attr("data-reset") == "true") {
                $(this).find("#message").closest("form")[0].reset();
                //$('.select2-hidden-accessible').select2('refresh');
                $(".select2-hidden-accessible").trigger("change.select2");
            }
        } else if (data.responseText == "delete") {
            //alert('Information is deleted from database successfully.')
            $(this).find("#message").html(
                '<span class="float-left alert alert-info w-100">Information is deleted from database successfully.' + close + '</span>');
            var form = $(this).find("#message").closest("form");
            if (form.attr("data-reset") == "true")
                $(this).find("#message").closest("form")[0].reset();
        } else if (data.responseText == "failed") {
            //alert(data.responseText);
            //alert('Something went wrong while trying to store data.')
            $(this).find("#message").html(
                '<span class="float-left alert alert-danger w-100">Something went wrong while trying to store data. ' + close + '</span>');
        }
        return false;
    }
    else {
        var result = data.responseJSON;
        if (result.status) {
            $(this).find("#message").html(
                '<span class="float-left alert alert-info w-100">' + result.message + close+'</span>');
        }
        else if (result.status == false) {
            $(this).find("#message").html(
                '<span class="float-left alert alert-danger w-100">' + result.message +close+ '</span>');

        }
        else
        $(this).find("#message").html('<span class="float-left alert alert-danger w-100">' +
            data.responseText +close+
            '</span> ');

    }
    //setTimeout(function () { $('#message').html(''); }, 5000)
    //window.location.reload()
}

function clearMessage() {
    $(window.event.target).closest("#message").html("");
}

function RebuildIndex() {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, do it!"
    }).then((result) => {
        if (result.value) {
            Swal.fire(
                "Completed!",
                "Index rebuild completed successfully.",
                "success"
            );
        }
    });
}


function DrawTable(tableId, colmodel, url, messageTop, title, searchonenter) {
    if ($.fn.dataTable.isDataTable(tableId)) {
        $(tableId).DataTable().destroy();
    }
    t = $(tableId).DataTable({
        processing: false,
        serverSide: true,
        info: true,
        ajax: {
            url: url,
            "data": function(d) {
                //we tell datatables to pass its object inside of a JSON string because MVC can't bind it properly.
                return { myDataTableParameter: JSON.stringify(d) };
            }
        },
        columns: colmodel,
        order: [[0, "desc"]],
        responsive: true,
        dom: "<'row no-gutters'<'col-sm-12 col-md-4'l><'col-sm-12 col-md-4 pt-1'B><'col-sm-12 col-md-4'f>>" +
            "<'row no-gutters'<'col-12'p>>" +
            "<'row no-gutters'<'col-sm-12'tr>>" +
            "<'row no-gutters'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        buttons: [
            {
                extend: "copy",
                text: '<i class="fa fa-copy"></i>',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "excel",
                text: '<i class="fa fa-file-excel-o"></i>',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "pdf",
                text: '<i class="fa fa-file-pdf-o"></i>',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "csv",
                text: '<i class="fa fa-file-text-o"></i>',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "print",
                text: '<i class="fa fa-print"></i>',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "colvis",
                text: '<i class="fa fa-columns"></i>',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            }
        ]
    });
    if (searchonenter) {
        $("" + tableId + " input").unbind();
        $("" + tableId + " input").bind("keyup",
            function(e) {
                if (e.keyCode == 13) {
                    t.search(this.value).draw();
                }
            });
    }
    //t.on('order.dt search.dt', function () {
    //    t.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
    //        cell.innerHTML = i + 1;
    //    });
    //}).draw();

}

function DrawGizzete(tableId, data, colmodel, messageTop, title, footerCallBack) {
    if ($.fn.dataTable.isDataTable(tableId)) {
        $(tableId).DataTable().destroy();
    }

    t = $(tableId).DataTable({
        data: data,
        columns: colmodel,
        order: [[0, "asc"]],
        responsive: true,
        dom:
            "<'row no-gutters py-1'<'col-sm-12 col-md-2 pl-2'l><'col-sm-12 col-md-7 pt-1 text-center'B><'col-sm-12 col-md-3 pr-2'f>>" +
                "<'row no-gutters'<'col-12'p>>" +
                "<'row no-gutters'<'col-sm-12'tr>>" +
                "<'row no-gutters py-1'<'col-sm-12 col-md-5 pl-1'i><'col-sm-12 col-md-7 pr-2'p>>",
        buttons: [
            {
                extend: "copy",
                text: '<i class="fa fa-copy"></i> copy',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "excel",
                text: '<i class="fa fa-file-excel-o"></i> export(xls)',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "pdf",
                text: '<i class="fa fa-file-pdf-o"></i> export (pdf)',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "csv",
                text: '<i class="fa fa-file-text-o"></i> export (csv,text)',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "print",
                text: '<i class="fa fa-print"></i> print',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            },
            {
                extend: "colvis",
                text: '<i class="fa fa-columns"></i> columns',
                className: "sico-export-btn",
                messageTop: messageTop,
                title: title
            }
        ],
        "footerCallback": footerCallBack
    });
    return t;


}

function DrawTableLocal(tableId, colmodel, url, messageTop, title, footerCallBack) {
    if ($.fn.dataTable.isDataTable(tableId)) {
        $(tableId).DataTable().destroy();
        //t.clear();
        //t.destroy();
    }
    $.get(url,
        function(data) {
            t = $(tableId).DataTable({
                data: JSON.parse(data),
                columns: colmodel,
                order: [[0, "asc"]],
                responsive: true,
                dom:
                    "<'row no-gutters py-1'<'col-sm-12 col-md-2 pl-2'l><'col-sm-12 col-md-7 pt-1 text-center'B><'col-sm-12 col-md-3 pr-2'f>>" +
                        "<'row no-gutters'<'col-12'p>>" +
                        "<'row no-gutters'<'col-sm-12'tr>>" +
                        "<'row no-gutters py-1'<'col-sm-12 col-md-5 pl-1'i><'col-sm-12 col-md-7 pr-2'p>>",
                buttons: [
                    {
                        extend: "copy",
                        text: '<i class="fa fa-copy"></i> copy',
                        className: "sico-export-btn",
                        messageTop: messageTop,
                        title: title
                    },
                    {
                        extend: "excel",
                        text: '<i class="fa fa-file-excel-o"></i> export(xls)',
                        className: "sico-export-btn",
                        messageTop: messageTop,
                        title: title
                    },
                    {
                        extend: "pdf",
                        text: '<i class="fa fa-file-pdf-o"></i> export (pdf)',
                        className: "sico-export-btn",
                        messageTop: messageTop,
                        title: title
                    },
                    {
                        extend: "csv",
                        text: '<i class="fa fa-file-text-o"></i> export (csv,text)',
                        className: "sico-export-btn",
                        messageTop: messageTop,
                        title: title
                    },
                    {
                        extend: "print",
                        text: '<i class="fa fa-print"></i> print',
                        className: "sico-export-btn",
                        messageTop: messageTop,
                        title: title
                    },
                    {
                        extend: "colvis",
                        text: '<i class="fa fa-columns"></i> columns',
                        className: "sico-export-btn",
                        messageTop: messageTop,
                        title: title
                    }
                ],
                "footerCallback": footerCallBack
            });
            return t;
        });
}

function DrawTableLocalWithoutButton(tableId, colmodel, url, messageTop, title, initCompleteEvent, footerCallBack) {
    if ($.fn.dataTable.isDataTable(tableId)) {
        $(tableId).DataTable().destroy();
    }
    $.get(url,
        function(data) {
            $(tableId).DataTable({
                destroy: true,
                data: JSON.parse(data),
                columns: colmodel,
                order: [[0, "desc"]],
                responsive: true,
                dom: "<'row no-gutters py-1'<'col-sm-12 col-md-4 pl-2'l><'col-sm-12 col-md-6 pr-2'f>>" +
                    "<'row no-gutters'<'col-sm-12'tr>>" +
                    "<'row no-gutters py-1'<'col-sm-12 col-md-5 pl-1'i><'col-sm-12 col-md-7 pr-2'p>>",
                "initComplete": initCompleteEvent,
                "footerCallback": footerCallBack
            });
            //return t;
        });
}

$.fn.clock = function() {

    var clockID;

    function UpdateClock() {
        var tDate = new Date(new Date().getTime());
        var in_hours = tDate.getHours();
        var in_minutes = tDate.getMinutes();
        var in_seconds = tDate.getSeconds();

        if (in_minutes < 10)
            in_minutes = "0" + in_minutes;
        if (in_seconds < 10)
            in_seconds = "0" + in_seconds;
        if (in_hours < 10)
            in_hours = "0" + in_hours;

        this.innerHTML = "" + in_hours + ":" + in_minutes + ":" + in_seconds;

    }

    this.start = function StartClock() {
        clockID = setInterval(UpdateClock, 500);
    };

    this.stop = function KillClock() {
        clearTimeout(clockID);
    };


};


function clockUpdate(elementId) {
    var date = new Date();
    $(elementId).css({ 'color': "#fff", 'text-shadow': "0 0 6px #ff0" });

    function addZero(x) {
        if (x < 10) {
            return x = "0" + x;
        } else {
            return x;
        }
    }

    function twelveHour(x) {
        if (x > 12) {
            return x = x - 12;
        } else if (x == 0) {
            return x = 12;
        } else {
            return x;
        }
    }

    var h = addZero(twelveHour(date.getHours()));
    var m = addZero(date.getMinutes());
    var s = addZero(date.getSeconds());
    var text = "<span class='date'>" +
        date.toDateString() +
        "</span><span class='sico-hour'>" +
        h +
        "</span><span class='sico-seprator hour-min'>:</span> <span class='sico-min'>" +
        m +
        "</span><span class='sico-seprator min-sec'>:</span> <span class='sico-sec'>" +
        s +
        "</span>";
    $(elementId).html(text);
    //console.log($(elementId).html());
}

function clock(elementId) {
    //clockUpdate(elementId);
    setInterval(clockUpdate, 1000, elementId);
}


function MakeReport(tableId, colmodel, url, messageTop, title, colGroup) {
    //if ($.fn.dataTable.isDataTable(tableId)) {
    //    t.clear();
    //    t.destroy();
    //}
    $.get(url,
        function(data) {
            t = $(tableId).DataTable({
                data: JSON.parse(data),
                columns: colmodel,
                columnDefs: [
                    {
                        visible: true,
                        targets: 0
                    }
                ],
                order: [[0, "asc"]],
                rowGroup: {
                    startRender: function(rows, group) {
                        return group + " (" + rows.count() + " rows)";
                    },
                    dataSrc: colGroup
                },
                destroy: true,
                responsive: false,
                dom:
                    "<'row no-gutters py-1'<'col-sm-12 col-md-2 pl-2'l><'col-sm-12 col-md-8 pt-1'><'col-sm-12 col-md-2'B>>" +
                        "<'row no-gutters'<'col-12'p>>" +
                        "<'row no-gutters'<'col-sm-12'tr>>" +
                        "<'row no-gutters py-1'<'col-sm-12 col-md-5 pl-1'i><'col-sm-12 col-md-7 pr-2'p>>",
                stateSave: false,
                buttons: [
                    {
                        extend: "collection",
                        text: "... MORE",
                        className: "sico-export-btn",
                        fade: 0,
                        buttons: [
                            {
                                extend: "copy",
                                text: '<i class="fa fa-copy"></i> copy',
                                messageTop: messageTop,
                                title: title
                            },
                            {
                                extend: "excel",
                                text: '<i class="fa fa-file-excel-o"></i> export(xls)',
                                messageTop: messageTop,
                                title: title
                            },
                            {
                                extend: "pdf",
                                text: '<i class="fa fa-file-pdf-o"></i> export (pdf)',
                                messageTop: messageTop,
                                title: title
                            },
                            {
                                extend: "csv",
                                text: '<i class="fa fa-file-text-o"></i> export (csv,text)',
                                messageTop: messageTop,
                                title: title
                            },
                            {
                                extend: "print",
                                text: '<i class="fa fa-print"></i> print',
                                messageTop: messageTop,
                                title: title
                            },
                            //{ extend: 'colvis', text: '<i class="fa fa-columns"></i> columns', messageTop: messageTop, title: title }
                        ],
                    },
                    {
                        text: '<i class="fa fa-chart-line"></i> FILTER',
                        className: "sico-export-btn filterColumn",

                    },
                    {
                        text: '<i class="fa fa-chart-line"></i> CHART',
                        className: "sico-export-btn",
                    },
                    //{
                    //    extend: 'searchPanes',
                    //    text: '<i class="fa fa-chart-line"></i> search',
                    //    className: 'sico-export-btn',
                    //},
                ],
                initComplete: function() {
                    $(".filterColumn").on("click",
                        function() {
                            $(".hide").toggle();
                        });
                    this.api().columns().every(function() {
                        var column = this;
                        var select = $('<select><option value="">--select--</option></select>')
                            .appendTo($(column.footer()).empty())
                            .on("change",
                                function() {
                                    var val = $.fn.dataTable.util.escapeRegex(
                                        $(this).val()
                                    );

                                    column
                                        .search(val ? "^" + val + "$" : "", true, false)
                                        .draw();
                                });

                        column.data().unique().sort().each(function(d, j) {
                            select.append('<option value="' + d + '">' + d + "</option>");
                        });
                    });
                },

            });
            t.columns().every(function() {
                var that = this;

                $("input", this.footer()).on("keyup change",
                    function() {
                        if (that.search() !== this.value) {
                            that
                                .search(this.value)
                                .draw();
                        }
                    });
            });
            t.on("rowgroup-datasrc",
                function(e, dt, val) {
                    t.order.fixed({ pre: [[val, "asc"]] }).draw();
                });

            return t;
        });
}