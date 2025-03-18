// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Site JavaScript

// Modal işlemleri için yardımcı fonksiyonlar
function showModal(modalId) {
    const modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
}

function hideModal(modalId) {
    const modalElement = document.getElementById(modalId);
    const modal = bootstrap.Modal.getInstance(modalElement);
    if (modal) {
        modal.hide();
    }
}

// Form input alanları için otomatik is-filled sınıfı ekleme
document.addEventListener('DOMContentLoaded', function() {
    const inputs = document.querySelectorAll('.input-group.input-group-outline input, .input-group.input-group-outline textarea');
    
    inputs.forEach(input => {
        if (input.value !== '') {
            input.parentNode.classList.add('is-filled');
        }
        
        input.addEventListener('focus', function() {
            this.parentNode.classList.add('is-focused');
        });
        
        input.addEventListener('blur', function() {
            this.parentNode.classList.remove('is-focused');
            if (this.value !== '') {
                this.parentNode.classList.add('is-filled');
            } else {
                this.parentNode.classList.remove('is-filled');
            }
        });
    });
    
    // jQuery ile menü davranışını düzenleme
    $(document).ready(function() {
        // Menü açma/kapama
        $('.dropdown > a').on('click', function(e) {
            if ($(this).siblings('.nav-submenu').length > 0) {
                e.preventDefault();
                e.stopPropagation();
                
                var $submenu = $(this).siblings('.nav-submenu');
                
                // Eğer zaten açıksa kapat, değilse aç
                if ($submenu.is(':visible')) {
                    $submenu.slideUp();
                    $(this).removeClass('active');
                } else {
                    // Diğer açık menüleri kapat
                    $('.nav-submenu').not($submenu).slideUp();
                    $('.dropdown > a').not(this).removeClass('active');
                    
                    // Tıklanan menüyü aç
                    $submenu.slideDown();
                    $(this).addClass('active');
                }
            }
        });
        
        // Alt menü elemanlarının doğru çalışması için
        $('.nav-submenu a').on('click', function(e) {
            e.stopPropagation();
        });
        
        // Dışarıya tıklandığında açık menüleri kapat
        $(document).on('click', function(e) {
            if (!$(e.target).closest('.dropdown').length) {
                $('.nav-submenu').slideUp();
                $('.dropdown > a').removeClass('active');
            }
        });
        
        // Aktif menü öğesini işaretle
        var currentUrl = window.location.pathname;
        
        // Tam eşleşme için
        $('.nav-menu a[href="' + currentUrl + '"]').addClass('active');
        
        // Kısmi eşleşme için
        $('.nav-menu a').each(function() {
            var menuUrl = $(this).attr('href');
            if (menuUrl && currentUrl.startsWith(menuUrl) && menuUrl !== '/') {
                $(this).addClass('active');
            }
        });
        
        // Eğer alt menü elemanı aktifse, üst menüyü de aktif yap ve alt menüyü aç
        var $activeSubmenuItem = $('.nav-submenu a').filter(function() {
            var href = $(this).attr('href');
            return href === currentUrl || (href && currentUrl.startsWith(href) && href !== '/');
        });
        
        if ($activeSubmenuItem.length) {
            $activeSubmenuItem.addClass('active');
            $activeSubmenuItem.closest('.nav-submenu').show();
            $activeSubmenuItem.closest('.dropdown').find('> a').addClass('active');
        }
    });
});

// Para formatı için yardımcı fonksiyon
function formatCurrency(value) {
    if (value == null) return "-";
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
}

// Tarih formatı için yardımcı fonksiyon
function formatDate(dateString) {
    if (!dateString) return "-";
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('tr-TR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }).format(date);
}

// Sayfa yüklendiğinde sidebar aktif menü öğesini işaretleme
document.addEventListener('DOMContentLoaded', function() {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav-link');
    
    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath.includes(href) && href !== '/') {
            link.classList.add('active', 'bg-gradient-primary');
        } else if (href === '/' && currentPath === '/') {
            link.classList.add('active', 'bg-gradient-primary');
        }
    });
});

// Smooth scrollbar
var win = navigator.platform.indexOf('Win') > -1;
if (win && document.querySelector('#sidenav-scrollbar')) {
    var options = {
        damping: '0.5'
    }
    Scrollbar.init(document.querySelector('#sidenav-scrollbar'), options);
}

// Mobil cihazlarda sidebar toggle
document.addEventListener('DOMContentLoaded', function() {
    const iconNavbarSidenav = document.getElementById('iconNavbarSidenav');
    const iconSidenav = document.getElementById('iconSidenav');
    const sidenav = document.getElementById('sidenav-main');
    
    if (iconNavbarSidenav) {
        iconNavbarSidenav.addEventListener('click', function() {
            sidenav.classList.toggle('show');
        });
    }
    
    if (iconSidenav) {
        iconSidenav.addEventListener('click', function() {
            sidenav.classList.remove('show');
        });
    }
});

