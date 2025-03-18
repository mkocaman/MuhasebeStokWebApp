/*=====================
  Sidebar Menu js
==========================*/
$(document).ready(function() {
    // Menü açma/kapama işlevi
    $('.toggle-sidebar').on('click', function() {
        $('.page-body-wrapper').toggleClass('sidebar-close');
        $(this).toggleClass('active');
    });

    // Alt menüleri açma/kapama
    $('.sidebar-main .nav-submenu').hide();
    $('.sidebar-main .dropdown').click(function() {
        $('.sidebar-main .nav-submenu').slideUp();
        if (!$(this).hasClass('active')) {
            $('.sidebar-main .dropdown').removeClass('active');
            $(this).addClass('active');
            $(this).find('.nav-submenu').slideDown();
        } else {
            $(this).removeClass('active');
            $(this).find('.nav-submenu').slideUp();
        }
    });

    // Mobil menü işlevi
    $('.mobile-sidebar .dropdown').click(function(e) {
        $('.mobile-sidebar .nav-submenu').slideUp();
        $(this).find('.nav-submenu').slideToggle();
    });

    // Aktif menü öğesini işaretleme
    var current = window.location.pathname;
    $('.sidebar-main .nav-menu li a').each(function() {
        var $this = $(this);
        if ($this.attr('href') === current) {
            $this.addClass('active');
            $this.parents('.dropdown').addClass('active');
            $this.parents('.nav-submenu').slideDown();
        }
    });

    // Responsive menü
    $('.responsive-btn').on('click', function() {
        $('.responsive-btn').toggleClass('active');
        $('.page-body-wrapper').toggleClass('show-menu');
    });

    // Menü öğesi tıklama olayı
    $('.nav-menu > li > a').click(function(e) {
        if ($(this).parent().hasClass('dropdown') && !$('body').hasClass('horizontal-menu')) {
            e.preventDefault();
        }
    });

    // Yatay menü için özel işlevler
    if ($('body').hasClass('horizontal-menu')) {
        $('.sidebar-main .nav-submenu').hide();
        $('.sidebar-main .dropdown').hover(function() {
            $(this).find('.nav-submenu').stop(true, true).slideDown();
        }, function() {
            $(this).find('.nav-submenu').stop(true, true).slideUp();
        });
    }
});

    $(".toggle-nav").click(function () {
        $('.nav-menu').css("left", "0px");
    });
    $(".mobile-back").click(function () {
        $('.nav-menu').css("left", "-410px");
    });
   

    $(".page-wrapper").attr("class", "page-wrapper "+localStorage.getItem("page-wrapper"));
    $(".page-body-wrapper").attr("class", "page-body-wrapper "+localStorage.getItem("page-body-wrapper"));


    if (localStorage.getItem("page-wrapper") === null) {
        $(".page-wrapper").addClass("compact-wrapper");
    }   

  // left sidebar and horizotal menu
    if($('#pageWrapper').hasClass('compact-wrapper')){
          jQuery('.submenu-title').append('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
          jQuery('.submenu-title').click(function () {
              jQuery('.submenu-title').removeClass('active');
              jQuery('.submenu-title').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
              jQuery('.submenu-content').slideUp('normal');
              if (jQuery(this).next().is(':hidden') == true) {
                  jQuery(this).addClass('active');
                  jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                  jQuery(this).next().slideDown('normal');
              } else {
                  jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
              }
          });
          jQuery('.submenu-content').hide();

          jQuery('.menu-title').append('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
          jQuery('.menu-title').click(function () {
              jQuery('.menu-title').removeClass('active');
              jQuery('.menu-title').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
              jQuery('.menu-content').slideUp('normal');
              if (jQuery(this).next().is(':hidden') == true) {
                  jQuery(this).addClass('active');
                  jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                  jQuery(this).next().slideDown('normal');
              } else {
                  jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
              }
          });
          jQuery('.menu-content').hide();
    } else if ($('#pageWrapper').hasClass('horizontal-wrapper')) {
        var contentwidth = jQuery(window).width();
        if ((contentwidth) < '992') {
            $('#pageWrapper').removeClass('horizontal-wrapper').addClass('compact-wrapper');
            $('.page-body-wrapper').removeClass('horizontal-menu').addClass('sidebar-icon');
            jQuery('.submenu-title').append('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
          jQuery('.submenu-title').click(function () {
              jQuery('.submenu-title').removeClass('active');
              jQuery('.submenu-title').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
              jQuery('.submenu-content').slideUp('normal');
              if (jQuery(this).next().is(':hidden') == true) {
                  jQuery(this).addClass('active');
                  jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                  jQuery(this).next().slideDown('normal');
              } else {
                  jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
              }
          });
          jQuery('.submenu-content').hide();

          jQuery('.menu-title').append('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
          jQuery('.menu-title').click(function () {
              jQuery('.menu-title').removeClass('active');
              jQuery('.menu-title').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
              jQuery('.menu-content').slideUp('normal');
              if (jQuery(this).next().is(':hidden') == true) {
                  jQuery(this).addClass('active');
                  jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                  jQuery(this).next().slideDown('normal');
              } else {
                  jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
              }
          });
          jQuery('.menu-content').hide();
        }

    }

// toggle sidebar
$nav = $('.main-nav');
$header = $('.page-main-header');
$toggle_nav_top = $('#sidebar-toggle');
$toggle_nav_top.click(function() {
  $this = $(this);
  $nav = $('.main-nav');
  $nav.toggleClass('close_icon');
  $header.toggleClass('close_icon');
});

$( window ).resize(function() {
  $nav = $('.main-nav');
  $header = $('.page-main-header');
  $toggle_nav_top = $('#sidebar-toggle');
  $toggle_nav_top.click(function() {
    $this = $(this);
    $nav = $('.main-nav');
    $nav.toggleClass('close_icon');
    $header.toggleClass('close_icon');
  });
});

$body_part_side = $('.body-part');
$body_part_side.click(function(){
  $toggle_nav_top.attr('checked', false);
  $nav.addClass('close_icon');
  $header.addClass('close_icon');
});

// document sidebar
$('.mobile-sidebar').click(function(){
  $('.document').toggleClass('close')
});

// $(".mobile-sidebar").click(function(){
//   $("p").toggleClass("main");
// });

//    responsive sidebar
var $window = $(window);
var widthwindow = $window.width();
(function($) {
"use strict";
if(widthwindow+17 <= 993) {
    $toggle_nav_top.attr('checked', false);
    $nav.addClass("close_icon");
    $header.addClass("close_icon");
}
})(jQuery);
$( window ).resize(function() {
var widthwindaw = $window.width();
if(widthwindaw+17 <= 991){
    $toggle_nav_top.attr('checked', false);
    $nav.addClass("close_icon");
    $header.addClass("close_icon");
}else{
    $toggle_nav_top.attr('checked', true);
    $nav.removeClass("close_icon");
    $header.removeClass("close_icon");
}
});

// horizontal arrowss
var view = $("#mainnav");
var move = "500px";
var leftsideLimit = -500

// var Windowwidth = jQuery(window).width();
// get wrapper width
var getMenuWrapperSize = function () {
    return $('.sidebar-wrapper').innerWidth();
}
var menuWrapperSize = getMenuWrapperSize();

if ((menuWrapperSize) >= '1660') {
    var sliderLimit = -3000
    
} else if ((menuWrapperSize) >= '1440') {
    var sliderLimit = -3600
} else {
    var sliderLimit = -4200
}

$("#left-arrow").addClass("disabled");
$("#right-arrow").click(function () {
    var currentPosition = parseInt(view.css("marginLeft"));
    if (currentPosition >= sliderLimit) {
        $("#left-arrow").removeClass("disabled");
        view.stop(false, true).animate({
            marginLeft: "-=" + move
        }, {
            duration: 400
        })
        if (currentPosition == sliderLimit) {
            $(this).addClass("disabled");
            console.log("sliderLimit", sliderLimit);
        }
    }
});

$("#left-arrow").click(function () {
    var currentPosition = parseInt(view.css("marginLeft"));
    if (currentPosition < 0) {
        view.stop(false, true).animate({
            marginLeft: "+=" + move
        }, {
            duration: 400
        })
        $("#right-arrow").removeClass("disabled");
        $("#left-arrow").removeClass("disabled");
        if (currentPosition >= leftsideLimit) {
            $(this).addClass("disabled");
        }
    }

});

// page active
    $( ".main-navbar" ).find( "a" ).removeClass("active");
    $( ".main-navbar" ).find( "li" ).removeClass("active");

    var current = window.location.pathname
    $(".main-navbar ul>li a").filter(function() {

        var link = $(this).attr("href");
        if(link){
            if (current.indexOf(link) != -1) {
                $(this).parents().children('a').addClass('active');
                $(this).parents().parents().children('ul').css('display', 'block');
                $(this).addClass('active');
                $(this).parent().parent().parent().children('a').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                $(this).parent().parent().parent().parent().parent().children('a').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                return false;
            }
        }
    });
    $('.custom-scrollbar').animate({
        scrollTop: $('a.nav-link.menu-title.active').offset().top - 500
    }, 1000);