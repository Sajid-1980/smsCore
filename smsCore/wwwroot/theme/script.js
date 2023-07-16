/*
Author       : Dreamguys
Template Name: Preskool - Bootstrap Admin Template
Version      : 1.0
*/
//$(window).on('hashchange', function () {
//	console.log('location.hash: ' + location.hash);
//	console.log('globalCurrentHash: ' + globalCurrentHash);

//	if (location.hash == globalCurrentHash) {
//		console.log('Going fwd');
//	}
//	else {
//		console.log('Going Back'); 
//		//loadMenuSelection(location.hash);
//	}
//});

$(window).on('popstate', function (e) {
	console.log('location: ' + location);
	//alert('Back button was pressed.');
});

function schoolName() {
	return $('#globalschoolname').val();
}
function schoolAddress() {
	return $('#globalschooladdress').val();
}

var Sidemenu = function () {
	this.$menuItem = $('#sidebar-menu a');
};

$('body').on('click', 'a.ajax-load-menu', function (e) {
	e.preventDefault();
	e.stopPropagation();
	var url = window.$(this).attr('href');
	Navigate(url);
	$('#sidebar-menu .active').each(function () {
		$(this).removeClass('active');
	})
	let li = $(this).closest('li');
	li.addClass('active');

	return false;

});

function init() {
	$('a.ajax-load-menu').on('click', function (e) {


	});
	$.fn.scrollTo = function (speed, top) {
		if (typeof (speed) === 'undefined')
			speed = 1000;

		console.log(top);
		$(this).animate({
			scrollTop: parseInt(top)
		}, speed);
	};
	var $this = Sidemenu;
	$(' #sidebar-menu a').on('click', function (e) {
		if ($('body').hasClass('top-menu') && $(this).closest('li').hasClass('submenu')) {
			return false;
		}
		if ($(this).parent().hasClass('submenu')) {
			e.preventDefault();
			//$('.sidebar-inner').scrollTo(1000, $(this).parent().offset().top);
		}
		if (!$(this).hasClass('subdrop')) {
			$('ul', $(this).parents('ul:first')).slideUp(350);
			$('a', $(this).parents('ul:first')).removeClass('subdrop');
			$(this).next('ul').slideDown(350);
			$(this).addClass('subdrop');
		} else if ($(this).hasClass('subdrop')) {
			$(this).removeClass('subdrop');
			$(this).next('ul').slideUp(350);
		}
	});

	$('#sidebar-menu ul li.submenu a.active').parents('li:last').children('a:first').addClass('active').trigger('click');


}
(function ($) {
	"use strict";

	//var hub = $.connection.smsHub;
	//$.connection.hub.start().done(function () {
	//	console.log('hub started');
	//	hub.server.subscribeFeeEntry();
	//});
	//var feeToast;
	//hub.client.notification = function (data) {

	//	var html = $($('#notification-template').html());

	//	html.find('.noti-details').html(data.message);
	//	html.find('.notification-time').html('');
	//	$('#notification-list').append(html);
	//	console.log(feeToast);
	//	console.log(data);
	//	if (feeToast) {

	//		if (data.message != '')
	//			feeToast.update({
	//				text: data.message
	//			});
	//		if (data.title & data.title != '')
	//			feeToast.update({
	//				heading: data.title
	//			});

	//		feeToast.update({
	//			icon: data.type
	//		});

	//		return false;
	//	}
	//	else {
	//		feeToast = $.toast({
	//			text: data.message, // Text that is to be shown in the toast
	//			heading: data.title, // Optional heading to be shown on the toast
	//			icon: data.type, // Type of toast icon
	//			showHideTransition: 'fade', // fade, slide or plain
	//			allowToastClose: true, // Boolean value true or false
	//			hideAfter: false, // false to make it sticky or number representing the miliseconds as time after which toast needs to be hidden
	//			stack: 5, // false if there should be only one toast at a time or a number representing the maximum number of toasts to be shown at a time
	//			position: 'bottom-right', // bottom-left or bottom-right or bottom-center or top-left or top-right or top-center or mid-center or an object representing the left, right, top, bottom values
	//			textAlign: 'left',  // Text alignment i.e. left, right or center
	//			loader: true,  // Whether to show loader or not. True by default
	//			loaderBg: '#9EC600',  // Background color of the toast loader
	//			//beforeShow: function () { }, // will be triggered before the toast is shown
	//			//afterShown: function () { }, // will be triggered after the toat has been shown
	//			//beforeHide: function () { }, // will be triggered before the toast gets hidden
	//			//afterHidden: function () { }  // will be triggered after the toast has been hidden
	//		});
	//	}

	//	let length = $('#notification-list li').length;
	//	if (length == 0) {
	//		$('#notification-badge').hide();
	//	}
	//	else {
	//		$('#notification-badge').html(length).show();
	//	}
	//}


	$('#sidebar').addClass('bg-dark');
	block('#sidebar');
	$.get('/dashboard/getmenu', function (response) {
		$('#leftmenuitem-container').html(response).animate(300, 'linear');
		// Sidebar Initiate
		init();
		$('#sidebar').removeClass('bg-dark');

	}).always(function () { $('#sidebar').unblock(); });
	$("#global-search-ddl").on("change",
		function () {
			$("#global-search-form").attr("action", $(this).val());
			//$('#global-search-keyword').att('placeholder','');
		});

	function myFunction() {
		document.getElementById("myDropdown").classList.toggle("show");
	}

	//var sTimeout = 2 * 60 * 1000;
	//setInterval("SessionWarning()", (5 * 60000));
	//function SessionWarning() {

	//	window.$.get('@Url.Action("ResetSession","Dashboard")');
	//}

	// Close the dropdown menu if the user clicks outside of it
	window.onclick = function (event) {
		if (!event.target.matches('.dropbtn')) {
			var dropdowns = document.getElementsByClassName("dropdown-content");
			var i;
			for (i = 0; i < dropdowns.length; i++) {
				var openDropdown = dropdowns[i];
				if (openDropdown.classList.contains('show')) {
					openDropdown.classList.remove('show');
				}
			}
		}
	}
	// Variables declarations

	var $wrapper = $('.main-wrapper');
	var $pageWrapper = $('.page-wrapper');
	//var $slimScrolls = $('.slimscroll');

	// Sidebar





	// Sidebar Initiate
	//init();

	// Mobile menu sidebar overlay

	$('body').append('<div class="sidebar-overlay"></div>');
	$(document).on('click', '#mobile_btn', function () {
		$wrapper.toggleClass('slide-nav');
		$('.sidebar-overlay').toggleClass('opened');
		$('html').addClass('menu-opened');
		return false;
	});

	// Sidebar overlay

	$(".sidebar-overlay").on("click", function () {
		$wrapper.removeClass('slide-nav');
		$(".sidebar-overlay").removeClass("opened");
		$('html').removeClass('menu-opened');
	});

	// Page Content Height

	if ($('.page-wrapper').length > 0) {
		var height = $(window).height();
		$(".page-wrapper").css("min-height", height);
	}

	// Page Content Height Resize

	$(window).resize(function () {
		if ($('.page-wrapper').length > 0) {
			var height = $(window).height();
			$(".page-wrapper").css("min-height", height);
		}
	});

	// Select 2

	if ($('.select').length > 0) {
		$('.select').select2({
			minimumResultsForSearch: -1,
			width: '100%'
		});
	}

	// Datetimepicker

	if ($('.datetimepicker').length > 0) {
		$('.datetimepicker').datetimepicker({
			format: 'DD-MM-YYYY',
			icons: {
				up: "fas fa-angle-up",
				down: "fas fa-angle-down",
				next: 'fas fa-angle-right',
				previous: 'fas fa-angle-left'
			}
		});
		$('.datetimepicker').on('dp.show', function () {
			$(this).closest('.table-responsive').removeClass('table-responsive').addClass('temp');
		}).on('dp.hide', function () {
			$(this).closest('.temp').addClass('table-responsive').removeClass('temp')
		});
	}

	// Tooltip

	if ($('[data-toggle="tooltip"]').length > 0) {
		$('[data-toggle="tooltip"]').tooltip();
	}

	// Datatable

	if ($('.datatable').length > 0) {
		$('.datatable').DataTable({
			"bFilter": false,
		});
	}

	// Check all email

	$(document).on('click', '#check_all', function () {
		$('.checkmail').click();
		return false;
	});
	if ($('.checkmail').length > 0) {
		$('.checkmail').each(function () {
			$(this).on('click', function () {
				if ($(this).closest('tr').hasClass('checked')) {
					$(this).closest('tr').removeClass('checked');
				} else {
					$(this).closest('tr').addClass('checked');
				}
			});
		});
	}

	// Mail important

	$(document).on('click', '.mail-important', function () {
		$(this).find('i.fa').toggleClass('fa-star').toggleClass('fa-star-o');
	});

	// Summernote

	if ($('.summernote').length > 0) {
		$('.summernote').summernote({
			height: 200,                 // set editor height
			minHeight: null,             // set minimum height of editor
			maxHeight: null,             // set maximum height of editor
			focus: false                 // set focus to editable area after initializing summernote
		});
	}


	// Sidebar Slimscroll





	$(function () {
		if ($('body').hasClass('top-menu'))
			slimScroll(true)
		else slimScroll(false);
	})
	// Small Sidebar

	$(document).on('click', '#toggle_btn', function () {
		if ($('body').hasClass('mini-sidebar')) {
			$('body').removeClass('mini-sidebar');
			$('.subdrop + ul').slideDown();
		} else {
			$('body').addClass('mini-sidebar');
			$('.subdrop + ul').slideUp();
		}
		setTimeout(function () {
			mA.redraw();
			mL.redraw();
		}, 300);
		return false;
	});
	$(document).on('mouseover', function (e) {
		e.stopPropagation();
		if ($('body').hasClass('mini-sidebar') && !$('body').hasClass('top-menu') && $('#toggle_btn').is(':visible')) {
			var targ = $(e.target).closest('.sidebar').length;
			if (targ) {
				$('body').addClass('expand-menu');
				$('.subdrop + ul').slideDown();
			} else {
				$('body').removeClass('expand-menu');
				$('.subdrop + ul').slideUp();
			}
			return false;
		}
	});

	// Circle Progress Bar
	function animateElements() {
		$('.circle-bar1').each(function () {
			var elementPos = $(this).offset().top;
			var topOfWindow = $(window).scrollTop();
			var percent = $(this).find('.circle-graph1').attr('data-percent');
			var animate = $(this).data('animate');
			if (elementPos < topOfWindow + $(window).height() - 30 && !animate) {
				$(this).data('animate', true);
				$(this).find('.circle-graph1').circleProgress({
					value: percent / 100,
					size: 400,
					thickness: 30,
					fill: {
						color: '#6e6bfa'
					}
				});
			}
		});
		$('.circle-bar2').each(function () {
			var elementPos = $(this).offset().top;
			var topOfWindow = $(window).scrollTop();
			var percent = $(this).find('.circle-graph2').attr('data-percent');
			var animate = $(this).data('animate');
			if (elementPos < topOfWindow + $(window).height() - 30 && !animate) {
				$(this).data('animate', true);
				$(this).find('.circle-graph2').circleProgress({
					value: percent / 100,
					size: 400,
					thickness: 30,
					fill: {
						color: '#6e6bfa'
					}
				});
			}
		});
		$('.circle-bar3').each(function () {
			var elementPos = $(this).offset().top;
			var topOfWindow = $(window).scrollTop();
			var percent = $(this).find('.circle-graph3').attr('data-percent');
			var animate = $(this).data('animate');
			if (elementPos < topOfWindow + $(window).height() - 30 && !animate) {
				$(this).data('animate', true);
				$(this).find('.circle-graph3').circleProgress({
					value: percent / 100,
					size: 400,
					thickness: 30,
					fill: {
						color: '#6e6bfa'
					}
				});
			}
		});
	}

	if ($('.circle-bar').length > 0) {
		animateElements();
	}
	$(window).scroll(animateElements);

	// Preloader

	$(window).on('load', function () {
		if ($('#loader').length > 0) {
			$('#loader').delay(350).fadeOut('slow');
			$('body').delay(350).css({ 'overflow': 'visible' });
		}
	})

})(jQuery);


var $slimScrolls = $('.sidebar-inner2');
function slimScroll(destory) {
	if ($slimScrolls.length > 0) {
		if (destory) {
			$('.slimscroll').slimScroll({ destory: true });
			$(".slimScrollBar,.slimScrollRail").remove();
			$(".slimscroll").contents().unwrap();
			$('.slimscroll').slimscroll("destroy");
			$('.slimScrollDiv').attr('style', '').css('height', 'auto');
			setTimeout(function () {
				$('.slimScrollDiv').attr('style', '').css('height', 'auto');
			}, 300);
		}
		else {
			$slimScrolls.slimScroll({
				height: 'auto',
				width: '100%',
				position: 'right',
				size: '12px',
				color: '#6c757d',
				allowPageScroll: false,
				wheelStep: 10,
				touchScrollStep: 100
			});
			var wHeight = $('body').hasClass('top-menu') ? 0 : $(window).height() - 60;
			$slimScrolls.height(wHeight);
			$('.sidebar .slimScrollDiv').height(wHeight);
			console.log('slim scroll height setting', wHeight);
			$(window).resize(function () {
				var template = $('body').hasClass('top-menu');
				{
					var rHeight = template ? 0 : $(window).height() - 60;
					$slimScrolls.height(rHeight);
					$('.sidebar .slimScrollDiv').height(rHeight);
				}
			});
		}
	}
}

function changeLayout() {
	var ontop = $('body').hasClass('top-menu');
	if (ontop) {
		//slimScroll(false);
		$('.sidebar-inner').css('overflow-y', 'scroll');
		$('body').removeClass('top-menu mini-sidebar');
	}
	else {
		$('body').addClass('top-menu mini-sidebar');
		//slimScroll(true);
		$('.sidebar-inner').css('overflow-y', 'initial');
	}
}
