function begin() {

    $('#message').html('');
    block('#loginForm');

}
function loggedIn(response) {
    if (!response.responseJSON) {
        $('#message').html('<p class="alert alert-danger">An unknown network error occured.</p>');
        $('#loginForm').unblock();
        return false;
    }
    let json = response.responseJSON;
    if (json.status) {
        if (json.url)
            window.location.href = json.url;
        else window.location.href = "/";
    }
    else {
        if (!json) {
            $('#message').html('<p class="alert alert-danger">An unknown network error occured.</p>');
        }
        else {
            $('#message').html('<p class="alert alert-danger">' + json.message + '</p>');
        }
        $('#loginForm').unblock();
    }


}

function block(id) {
    $(id).block({
        message: '<i class="fa fa-spin fa-spinner"></i>',
        overlayCSS: {
            backgroundColor: '#fff',
            opacity: 0.8,
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