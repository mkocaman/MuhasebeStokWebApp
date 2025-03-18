(function($) {
    "use strict";
    $(".mobile-toggle").click(function(){
        $(".nav-menus").toggleClass("open");
    });
    $(".mobile-toggle-left").click(function(){
        $(".left-header").toggleClass("open");
    });
    $(".mobile-search").click(function(){
       $(".form-control-plaintext").toggleClass("open");
   });
    $(".bookmark-search").click(function(){
        $(".form-control-search").toggleClass("open");
    })
    $(".filter-toggle").click(function(){
        $(".product-sidebar").toggleClass("open");
    });
    $(".toggle-data").click(function(){
        $(".product-wrapper").toggleClass("sidebaron");
    });
    $(".form-control-search").keyup(function(e){
        if(e.target.value) {
            $(".page-wrapper.horizontal-wrapper").addClass("offcanvas-bookmark");
        } else {
            $(".page-wrapper.horizontal-wrapper").removeClass("offcanvas-bookmark");
        }
    });
})(jQuery);

$('.loader-wrapper').fadeOut('slow', function() {
    $(this).remove();
});

$(window).on('scroll', function() {
    if ($(this).scrollTop() > 600) {
        $('.tap-top').fadeIn();
    } else {
        $('.tap-top').fadeOut();
    }
});


$('.media-size-email svg').on('click', function (e) {
    $(this).toggleClass("like");
});



$('.tap-top').click( function() {
    $("html, body").animate({
        scrollTop: 0
    }, 600);
    return false;
});

function toggleFullScreen() {
    if ((document.fullScreenElement && document.fullScreenElement !== null) ||
        (!document.mozFullScreen && !document.webkitIsFullScreen)) {
        if (document.documentElement.requestFullScreen) {
            document.documentElement.requestFullScreen();
        } else if (document.documentElement.mozRequestFullScreen) {
            document.documentElement.mozRequestFullScreen();
        } else if (document.documentElement.webkitRequestFullScreen) {
            document.documentElement.webkitRequestFullScreen(Element.ALLOW_KEYBOARD_INPUT);
        }
    } else {
        if (document.cancelFullScreen) {
            document.cancelFullScreen();
        } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
        } else if (document.webkitCancelFullScreen) {
            document.webkitCancelFullScreen();
        }
    }
}
(function($, window, document, undefined) {
    "use strict";
    var $ripple = $(".js-ripple");
    $ripple.on("click.ui.ripple", function(e) {
        var $this = $(this);
        var $offset = $this.parent().offset();
        var $circle = $this.find(".c-ripple__circle");
        var x = e.pageX - $offset.left;
        var y = e.pageY - $offset.top;
        $circle.css({
            top: y + "px",
            left: x + "px"
        });
        $this.addClass("is-active");
    });
    $ripple.on(
        "animationend webkitAnimationEnd oanimationend MSAnimationEnd",
        function(e) {
            $(this).removeClass("is-active");
        });
})(jQuery, window, document);


// active link
$(".chat-menu-icons .toogle-bar").click(function(){
    $(".chat-menu").toggleClass("show");
});


// Language
var tnum = 'en';
$(document).ready(function () {
    if (localStorage.getItem("primary") != null) {
        var primary_val = localStorage.getItem("primary");
        $("#ColorPicker1").val(primary_val);
        var secondary_val = localStorage.getItem("secondary");
        $("#ColorPicker2").val(secondary_val);
    }
    $(document).click(function (e) {
        $('.translate_wrapper, .more_lang').removeClass('active');
    });
    $('.translate_wrapper .current_lang').click(function (e) {
        e.stopPropagation();
        $(this).parent().toggleClass('active');

        setTimeout(function () {
            $('.more_lang').toggleClass('active');
        }, 5);
    });

    /*TRANSLATE*/
    translate(tnum);

    $('.more_lang .lang').click(function () {
        $(this).addClass('selected').siblings().removeClass('selected');
        $('.more_lang').removeClass('active');

        var i = $(this).find('i').attr('class');
        var lang = $(this).attr('data-value');
        var tnum = lang;
        translate(tnum);

        $('.current_lang .lang-txt').text(lang);
        $('.current_lang i').attr('class', i);
    });
});

function translate(tnum) {
    $('.lan-1').text(trans[0][tnum]);
    $('.lan-2').text(trans[1][tnum]);
    $('.lan-3').text(trans[2][tnum]);
    $('.lan-4').text(trans[3][tnum]);
    $('.lan-5').text(trans[4][tnum]);
    $('.lan-6').text(trans[5][tnum]);
    $('.lan-7').text(trans[6][tnum]);
    $('.lan-8').text(trans[7][tnum]);
    $('.lan-9').text(trans[8][tnum]);
}

var trans = [
    {
        en: 'General',
        pt: 'Geral',
        es: 'General',
        fr: 'Generale',
        de: 'Generel',
        cn: 'General',
        ae: 'General'
    }, 
    {
        en: 'Dashboards, widgets & layout',
        pt: 'Paineis, widgets e layout',
        es: 'Paneles, widgets y diseño',
        fr: "Tableaux de bord, widgets et mise en page",
        de: 'Dashboards, widgets en lay-out',
        cn: 'Dashboards, widgets and layout',
        ae: 'Dashboards, widgets and layout'
    }, 
    {
        en: 'Dashboards',
        pt: 'Paineis',
        es: 'Paneles',
        fr: 'Tableaux',
        de: 'Dashboards',
        cn: 'Dashboards',
        ae: 'Dashboards'
    }, 
    {
        en: 'Default',
        pt: 'Padrao',
        es: 'Predeterminado',
        fr: 'Defaut',
        de: 'Standaard',
        cn: 'Default',
        ae: 'Default'
    }, 
    {
        en: 'Ecommerce',
        pt: 'Comercio eletronico',
        es: 'Comercio electronico',
        fr: 'Commerce electronique',
        de: 'E-commerce',
        cn: 'Ecommerce',
        ae: 'Ecommerce'
    }, 
    {
        en: 'Widgets',
        pt: 'Ferramenta',
        es: 'Widgets',
        fr: 'Widgets',
        de: 'Widgets',
        cn: 'Widgets',
        ae: 'Widgets'
    }, 
    {
        en: 'Page layout',
        pt: 'Layout da pagina',
        es: 'Diseño de pagina',
        fr: 'Mise en page',
        de: 'Mise en page',
        cn: 'Page layout',
        ae: 'Page layout'
    }, 
    {
        en: 'Applications',
        pt: 'Formularios',
        es: 'Aplicaciones',
        fr: 'Applications',
        de: 'Toepassingen',
        cn: 'Applications',
        ae: 'Applications'
    }, 
    {
        en: 'Ready to use Apps',
        pt: 'Pronto para usar aplicativos',
        es: 'Aplicaciones listas para usar',
        fr: 'Applications pretes a utiliser',
        de: 'Klaar om apps te gebruiken',
        cn: 'Ready to use Apps',
        ae: 'Ready to use Apps'
    }
];

/*=====================
02. Background Image js
  ==========================*/
$(".bg-center").parent().addClass('b-center');
$(".bg-img-cover").parent().addClass('bg-size');
$('.bg-img-cover').each(function () {
    var el = $(this),
        src = el.attr('src'),
        parent = el.parent();
    parent.css({
        'background-image': 'url(' + src + ')',
        'background-size': 'cover',
        'background-position': 'center',
        'display': 'block'
    });
    el.hide();
});



/*----------------------------------------
passward show hide
----------------------------------------*/
$('.show-hide').show();
$('.show-hide span').addClass('show');

$('.show-hide span').click(function () {
if ($(this).hasClass('show')) {
    $('input[name="login[password]"]').attr('type', 'text');
    $(this).removeClass('show');
} else {
    $('input[name="login[password]"]').attr('type', 'password');
    $(this).addClass('show');
}
});
$('form button[type="submit"]').on('click', function () {
$('.show-hide span').addClass('show');
$('.show-hide').parent().find('input[name="login[password]"]').attr('type', 'password');
});



//landing header //
$(".toggle-menu").click(function(){
    $('.landing-menu').toggleClass('open');
});   
$(".menu-back").click(function(){
    $('.landing-menu').toggleClass('open');
});    

$('.product-size ul li ').on('click', function(e) {
    $(".product-size ul li ").removeClass("active");
    $(this).addClass("active");
});

$('.email-sidebar .email-aside-toggle ').on('click', function(e) {
    $(".email-sidebar .email-left-aside ").toggleClass("open");   
});


$('.job-sidebar .job-toggle ').on('click', function(e) {
    $(".job-sidebar .job-left-aside ").toggleClass("open");   
});


$(".mode").on("click", function () {
        $('.mode i').toggleClass("fa-moon-o").toggleClass("fa-lightbulb-o");
        // $('.mode-sun').toggleClass("show")
        $('body').toggleClass("dark-only");
        var color = $(this).attr("data-attr");
        localStorage.setItem('body', 'dark-only');
    });