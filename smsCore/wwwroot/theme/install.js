$.fn.disable = function () {
    return this.each(function () {
        if (typeof this.disabled != "undefined") this.disabled = true;
    });
};

$.fn.enable = function () {
    return this.each(function () {
        if (typeof this.disabled != "undefined") this.disabled = false;
    });
};
function reload() {
    location.reload();
}


function begin() {
    if ($(this).find(".nosubmit").length > 0) {
        //alert('error found')

        $([document.documentElement, document.body]).animate({
            scrollTop: $(".nosubmit:first").offset().top - 50
        },
            500);
        return false;
    }
    $(this).find('button[type="submit"]').disable();
}

function completed(data) {
    $(this).find('button[type="submit"]').enable();

    if (data.responseText == "success") {
        //alert('Information added to database successfully.')
        $(this).find("#message").html('<span class="float-left alert alert-info w-100">' +
            "Information added to database successfully." +
            '<a class="close" style="top: 4px;position: absolute;right: 7px;" onclick="clearMessage();"><i class="fa fa-close">x</i></a></span>');
        var form = $(this).find("#message").closest("form");

        $(this).find("#message").closest("form")[0].reset();
        location.reload();

    } else if (data.responseText == "failed") {
        //alert(data.responseText);
        //alert('Something went wrong while trying to store data.')
        $(this).find("#message").html(
            '<span class="float-left alert alert-danger w-100">Something went wrong while trying to store data. <a class="close" style="top: 4px;position: absolute;right: 7px;" onclick="clearMessage();"><i class="fa fa-close">x</i></a></span>');
    } else {
        $(this).find("#message").html('<span class="float-left alert alert-danger w-100">' +
            data.responseText +
            '<a class="close" style="top: 4px;position: absolute;right: 7px;" onclick="clearMessage();"><i class="fa fa-close">x</i></a></span> ');

    }
    //setTimeout(function () { $('#message').html(''); }, 5000)
    //window.location.reload()
}

function clearMessage() {
    $(window.event.target).closest("#message").html("");
}
$('Document').ready(function () {

    $('#browsebtn').on('click',
        function () {
            var preview = $(this).data('preview');
            var value = $(this).data('txtvalue');
            var targetFileInputID = $(this).data('targetfileinputid');
            $('#' + targetFileInputID + '').trigger('click');

        });

    $('input[type=file]').on('change',
        function (data) {

            var preview = $(this).data('preview');
            if (this.files) {
                for (i = 0; i < this.files.length; i++) {
                    var reader = new FileReader();
                    var cFile = this.files[i];
                    reader.onload = (function (e) {
                        $('#' + preview + '').attr('src', e.target.result);
                    });
                    reader.readAsDataURL(this.files[i]);
                }
            }

        });

    $("#clearbtn").click(function () {
        $('img#ComLogoPreview').attr("src", "");
    });

    $('.datepicker').datepicker();
});
