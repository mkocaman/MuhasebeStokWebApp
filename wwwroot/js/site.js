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

// Global modal işlevleri
function openModal(url, modalId) {
    $.ajax({
        type: "GET",
        url: url,
        success: function (response) {
            $(modalId).find('.modal-content').html(response);
            $(modalId).modal('show');
        },
        error: function (err) {
            showAlert('error', 'Hata!', 'Modal yüklenirken bir hata oluştu: ' + err.statusText);
        }
    });
}

function submitForm(form, successCallback, errorCallback) {
    var formData = new FormData(form);
    
    $.ajax({
        url: form.action,
        type: form.method,
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                if (typeof successCallback === 'function') {
                    successCallback(response);
                } else {
                    showAlert('success', 'Başarılı!', response.message || 'İşlem başarıyla tamamlandı.');
                    $('.modal').modal('hide');
                    if (typeof refreshDataTable === 'function') {
                        refreshDataTable();
                    } else {
                        window.location.reload();
                    }
                }
            } else {
                if (typeof errorCallback === 'function') {
                    errorCallback(response);
                } else {
                    showAlert('error', 'Hata!', response.message || 'İşlem sırasında bir hata oluştu.');
                }
            }
        },
        error: function (xhr) {
            if (typeof errorCallback === 'function') {
                errorCallback(xhr);
            } else {
                showAlert('error', 'Hata!', 'İşlem sırasında bir hata oluştu: ' + xhr.statusText);
            }
        }
    });
}

function showAlert(type, title, message) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: type,
            title: title,
            text: message,
            confirmButtonText: 'Tamam'
        });
    } else {
        alert(title + ': ' + message);
    }
}

// DataTable yenileme işlevi
function refreshDataTable() {
    if ($.fn.DataTable.isDataTable('.table-datatable')) {
        $('.table-datatable').DataTable().ajax.reload();
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
        // Sidebar toggle
        $('#sidebar-toggle').click(function() {
            $('.page-body-wrapper').toggleClass('sidebar-close');
        });
        
        // Tam ekran
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
        
        // Tam ekran butonu için
        $('[onclick="javascript:toggleFullScreen()"]').on('click', function() {
            toggleFullScreen();
        });
        
        // Menü tıklama olayı
        $('.dropdown > a').on('click', function(e) {
            // Alt menüsü olan menüler için
            if ($(this).siblings('.nav-submenu').length) {
                e.preventDefault();
                e.stopPropagation();
                
                // Eğer bu menü zaten açıksa, kapat
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                } else {
                    // Diğer tüm menüleri kapat
                    $('.dropdown > a').removeClass('active');
                    
                    // Bu menüyü aç
                    $(this).addClass('active');
                }
            }
        });
        
        // Alt menü elemanlarına tıklandığında yayılımı durdur
        $('.nav-submenu a').on('click', function(e) {
            e.stopPropagation();
        });
        
        // Sayfa dışına tıklandığında menüleri kapat
        $(document).on('click', function(e) {
            if (!$(e.target).closest('.dropdown').length) {
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
        
        // Eğer alt menü elemanı aktifse, üst menüyü de aktif yap
        var $activeSubmenuItem = $('.nav-submenu a').filter(function() {
            var href = $(this).attr('href');
            return href === currentUrl || (href && currentUrl.startsWith(href) && href !== '/');
        });
        
        if ($activeSubmenuItem.length) {
            $activeSubmenuItem.addClass('active');
            $activeSubmenuItem.closest('.dropdown').find('> a').addClass('active');
        }

        // Mobil menü için geri butonu
        $('.mobile-back').click(function() {
            $('.dropdown > a').removeClass('active');
        });

        // Sayfa yüklendiğinde loader'ı kaldır
        $('.loader-wrapper').fadeOut('slow');

        // DataTables için kontrol
        if (typeof $.fn.dataTable !== 'undefined') {
            // DataTables ayarları
            $.extend(true, $.fn.dataTable.defaults, {
                "language": {
                    "url": "/lib/datatables/i18n/Turkish.json"
                },
                "responsive": true,
                "pagingType": "full_numbers",
                "scrollY": "calc(100vh - 350px)", // Ekran yüksekliğine göre dinamik tablo yüksekliği
                "scrollCollapse": true,  // Scroll alanını içeriğe göre ayarlar
                "dom": "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                       "<'row'<'col-sm-12'tr>>" +
                       "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Tümü"]]
            });
            
            console.log("DataTables başarıyla yüklendi ve konfigüre edildi.");
        } else {
            console.error("DataTables kütüphanesi yüklenemedi! Lütfen JavaScript konsolunu kontrol edin.");
        }

        // Select2 initialization
        if (typeof $.fn.select2 !== 'undefined') {
            $('.select2').select2();
        }
        
        // Auto-hide alerts after 5 seconds
        setTimeout(function() {
            $('.alert').fadeOut('slow');
        }, 5000);
        
        // Initialize all tooltips
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl)
        });
        
        // Initialize all popovers
        var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
        var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
            return new bootstrap.Popover(popoverTriggerEl)
        });
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

// Tema değiştirme fonksiyonları
function initTheme() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);
    updateThemeIcon(savedTheme);
}

function toggleTheme() {
    const currentTheme = document.documentElement.getAttribute('data-theme');
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    
    document.documentElement.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
    updateThemeIcon(newTheme);
}

function updateThemeIcon(theme) {
    const icon = document.querySelector('.theme-toggle i');
    if (icon) {
        icon.className = theme === 'light' ? 'fas fa-moon' : 'fas fa-sun';
    }
}

// Sayfa yüklendiğinde tema ayarını uygula
document.addEventListener('DOMContentLoaded', function() {
    initTheme();
    
    // Tema değiştirme butonunu ekle
    const themeToggle = document.createElement('button');
    themeToggle.className = 'theme-toggle';
    themeToggle.innerHTML = '<i class="fas fa-moon"></i>';
    themeToggle.onclick = toggleTheme;
    document.body.appendChild(themeToggle);
});

// Push Bildirimleri
if ('Notification' in window) {
    Notification.requestPermission().then(function(permission) {
        if (permission === 'granted') {
            console.log('Push bildirimleri için izin verildi');
        }
    });
}

// SignalR Bağlantısı
if (typeof signalR !== 'undefined') {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", function (title, message, type) {
        // Toastr bildirimi göster
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "newestOnTop": true,
            "progressBar": true,
            "positionClass": "toast-top-right",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };

        switch (type) {
            case "success":
                toastr.success(message, title);
                break;
            case "info":
                toastr.info(message, title);
                break;
            case "warning":
                toastr.warning(message, title);
                break;
            case "error":
                toastr.error(message, title);
                break;
            default:
                toastr.info(message, title);
        }
    });

    // Bağlantıyı başlat
    connection.start()
        .then(function () {
            console.log("SignalR bağlantısı başarılı.");
        })
        .catch(function (err) {
            console.error("SignalR bağlantı hatası: " + err.toString());
            return console.error(err.toString());
        });
}

// Document ready işlemleri
$(document).ready(function () {
    // Sayfa yüklendiğinde otomatik alert kapatma
    setTimeout(function () {
        $('.alert').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 4000);

    // DataTable ortak ayarları
    if ($.fn.DataTable && $('.table-datatable').length) {
        $('.table-datatable').each(function() {
            if (!$.fn.DataTable.isDataTable(this)) {
                $(this).DataTable({
                    "language": {
                        "url": "/lib/datatables/i18n/Turkish.json"
                    },
                    "responsive": true,
                    "lengthChange": true,
                    "autoWidth": false
                });
            }
        });
    }
});

