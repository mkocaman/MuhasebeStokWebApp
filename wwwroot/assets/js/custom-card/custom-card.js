var customcard = {
    init: function() {
        // Card kapatma işlevi
        $(".card-option .close-card").on('click', function() {
            var $this = $(this);
            var port = $($this).parents('.card');
            port.css('display', 'none');
        });

        // Reload card işlevi - kart yenileme animasyonu
        $(".card-option .reload-card").on('click', function() {
            var $this = $(this);
            var port = $($this).parents('.card');
            port.addClass('card-load');
            port.append('<div class="card-loader"><div class="loader-p"></div></div>');
            
            setTimeout(function() {
                port.children('.card-loader').remove();
                port.removeClass('card-load');
            }, 2000);
        });

        // Ayarlar menüsünü toggle etme işlevi
        $(".card-option .setting-primary").on('click', function() {
            var $this = $(this);
            var port = $($this).closest('.card-option');
            port.find('.setting-option').toggleClass('open-setting');
            
            var settingElement = port.find('.setting-option');
            if (settingElement.hasClass('open-setting')) {
                settingElement.css({
                    'width': '230px',
                    'opacity': '1',
                    'visibility': 'visible'
                });
            } else {
                settingElement.css({
                    'width': '0',
                    'opacity': '0',
                    'visibility': 'hidden'
                });
            }
        });
        
        // Card içindeki butonların işlevleri
        $(".setting-list").find('li').on('click', function(e) {
            if (!$(this).find('.setting-option').hasClass('open-setting')) {
                e.stopPropagation();
            }
        });
        
        // Tam ekran modunu etkinleştirme
        $(".card-option .full-card").on('click', function() {
            var $this = $(this);
            var port = $($this).parents('.card');
            port.toggleClass('full-card');
            $(this).toggleClass('icon-maximize');
            $(this).toggleClass('icon-minimize');
            
            if (port.hasClass('full-card')) {
                $('body').css('overflow', 'hidden');
            } else {
                $('body').css('overflow', 'auto');
            }
        });
        
        // Card küçültme/büyütme işlevi
        $(".card-option .minimize-card").on('click', function() {
            var $this = $(this);
            var port = $($this).parents('.card');
            var card_body = port.children('.card-body').first();
            var card_footer = port.children('.card-footer');
            
            if (!port.hasClass('card-load')) {
                if (card_body.is(':visible')) {
                    card_body.slideUp();
                    if (card_footer.length) {
                        card_footer.slideUp();
                    }
                    $(this).removeClass('fa-minus').addClass('fa-plus');
                } else {
                    card_body.slideDown();
                    if (card_footer.length) {
                        card_footer.slideDown();
                    }
                    $(this).removeClass('fa-plus').addClass('fa-minus');
                }
            }
        });
        
        // Dark/Light tema geçişi
        $(".mode").on('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            var $body = $('body');
            // Her zaman mevcut durumun tersine geçiş yap
            if ($body.hasClass('dark-only')) {
                // Dark moddan Light moda geçiş
                $body.removeClass('dark-only');
                localStorage.setItem('theme-mode', 'light');
                $('.mode i').removeClass('fa-lightbulb-o').addClass('fa-moon-o');
            } else {
                // Light moddan Dark moda geçiş
                $body.addClass('dark-only');
                localStorage.setItem('theme-mode', 'dark');
                $('.mode i').removeClass('fa-moon-o').addClass('fa-lightbulb-o');
            }
            
            // Grafikleri güncellemek için özel bir olay tetikle
            $body.trigger('themeChanged', [$body.hasClass('dark-only') ? 'dark' : 'light']);
        });
        
        // Sayfa yüklendiğinde localStorage'dan tema tercihini al ve uygula
        $(document).ready(function() {
            // Tema varsayılan olarak light mod olsun
            localStorage.setItem('theme-mode', 'light');
            $('body').removeClass('dark-only');
            $('.mode i').removeClass('fa-lightbulb-o').addClass('fa-moon-o');
            
            // Mode butonunu daha görünür yap
            $('.mode').css({
                'cursor': 'pointer',
                'z-index': '1050',
                'position': 'relative'
            });
            
            $('.mode i').css({
                'font-size': '20px',
                'transition': 'all 0.3s ease'
            });
            
            // Feather ikonları başlat
            if (typeof feather !== 'undefined') {
                feather.replace();
            }
        });

        // HTML kodunu gösterme işlevi
        $(".view-html").on('click', function() {
            var html_source = $(this).parents(".card");
            var html_chield = html_source.find(".card-body");
            html_chield.toggleClass("show-source");
            $(this).toggleClass("fa-eye");
        });

        // Clipboard.js entegrasyonu - kod kopyalama işlevi
        if (typeof ClipboardJS !== 'undefined') {
            var clipboard = new ClipboardJS('.btn-clipboard');
            clipboard.on('success', function(e) {
                e.clearSelection();
                alert('Kod kopyalandı!');
            });
            clipboard.on('error', function(e) {
                alert('Kod kopyalanamadı!');
            });
        }

        // Layout için fonksiyonlar
        function toggleFullScreen() {
            if ((document.fullScreenElement && document.fullScreenElement !== null) || (!document.mozFullScreen && !document.webkitIsFullScreen)) {
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

        // Tam ekran toggle için olay dinleyici
        $('.toggle-full-screen').on('click', function() {
            toggleFullScreen();
        });

        // Sidebar toggle etme işlevi
        $('#sidebar-toggle').on('click', function() {
            $('body').toggleClass('sidebar-close');
        });
    }
};

// jQuery gereksinimi için
(function($) {
    "use strict";
    customcard.init();

    // Sayfa yüklendiğinde fonksiyonlar
    $(document).ready(function() {
        // Widget ayarları için olay dinleyiciler
        $('.setting-list .setting-option li').on('click', function(e) {
            e.stopPropagation();
        });

        // Widget için eksik işlevleri tamamla
        $('.card-header-right .close-card').on('click', function() {
            var $this = $(this);
            var port = $($this).parents('.card');
            port.animate({
                'opacity': '0',
                '-webkit-transform': 'scale3d(.3, .3, .3)',
                'transform': 'scale3d(.3, .3, .3)'
            });
            setTimeout(function() {
                port.remove();
            }, 800);
        });

        // Tüm widget işlevlerini aktifleştir
        $('.card-option li').on('click', function(e) {
            e.stopPropagation();
        });

        // Harici RTL destek
        $(".rtl .card-header-left .close-card").on('click', function() {
            var $this = $(this);
            var port = $($this).parents('.card');
            port.animate({
                'opacity': '0',
                '-webkit-transform': 'scale3d(.3, .3, .3)',
                'transform': 'scale3d(.3, .3, .3)'
            });
            setTimeout(function() {
                port.remove();
            }, 800);
        });

        // Dışarı tıklandığında ayarlar menüsünü kapat
        $(document).on('click', function(e) {
            if (!$(e.target).closest('.card-option').length) {
                $('.setting-option').removeClass('open-setting').css({
                    'width': '0',
                    'opacity': '0',
                    'visibility': 'hidden'
                });
            }
        });
        
        // İkonları güncelle (Feather)
        setTimeout(function() {
            if (typeof feather !== 'undefined') {
                feather.replace();
            }
        }, 300);
        
        // Loader için işlevler
        setTimeout(function() {
            $('.loader-wrapper').fadeOut('slow');
        }, 1000);

        // Konfeti animasyonu
        if ($('.confetti').length > 0) {
            $('.confetti-piece').each(function(index) {
                var delay = index * 50;
                var $this = $(this);
                setTimeout(function() {
                    $this.css('animation', 'makeItRain 5s ease-out infinite');
                }, delay);
            });
        }
    });
})(jQuery);