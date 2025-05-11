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

// Numerik input için sayısal formatlama
function formatNumericInput(input) {
    // Input değerini al ve temizle
    let value = input.value.replace(/[^\d.,]/g, '');
    
    // Türkçe lokalizasyona göre nokta ve virgül düzenlemesi
    // Virgülden sonra en fazla 2 rakam olmalı
    if (value.includes(',')) {
        const parts = value.split(',');
        if (parts[1].length > 2) {
            parts[1] = parts[1].substring(0, 2);
            value = parts.join(',');
        }
    }
    
    // İngilizce lokalizasyona göre nokta düzenlemesi
    if (value.includes('.')) {
        const parts = value.split('.');
        if (parts[1].length > 2) {
            parts[1] = parts[1].substring(0, 2);
            value = parts.join('.');
        }
    }
    
    // Eğer sayısal bir değer elde edilemezse boş bırak
    if (isNaN(parseFloat(value.replace(',', '.')))) {
        input.value = '';
    } else {
        input.value = value;
    }
}

// Input alanlarında sayısal giriş kontrolü için event listener
document.addEventListener('DOMContentLoaded', function() {
    // KDV alanı ve diğer sayısal alanlar için olay dinleyicileri
    const numericInputs = document.querySelectorAll('input[type="number"], input[name="KDVOrani"]');
    
    numericInputs.forEach(input => {
        input.addEventListener('input', function() {
            formatNumericInput(this);
        });
        
        input.addEventListener('blur', function() {
            // Boşsa veya değer yoksa varsayılan değer atama
            if (this.value === '' && this.name === 'KDVOrani') {
                this.value = '12';
            }
        });
    });
});

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
        
        // Menü tıklama olayı - Düzeltildi
        $('.dropdown > a.nav-link.menu-title').on('click', function(e) {
            // Alt menüsü olan menüler için
            if ($(this).siblings('.nav-submenu').length) {
                e.preventDefault();
                e.stopPropagation();
                
                // Eğer bu menü zaten açıksa, kapat
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                    $(this).siblings('.nav-submenu').slideUp(300);
                } else {
                    // Diğer tüm menüleri kapat
                    $('.dropdown > a.nav-link.menu-title').removeClass('active');
                    $('.nav-submenu').slideUp(300);
                    
                    // Bu menüyü aç
                    $(this).addClass('active');
                    $(this).siblings('.nav-submenu').slideDown(300);
                }
            }
        });
        
        // Sayfa yüklendiğinde aktif menüyü aç
        setTimeout(function() {
            // Aktif alt menü elemanı varsa, üst menüyü otomatik aç
            var $activeSubmenuItem = $('.nav-submenu a.active');
            if ($activeSubmenuItem.length) {
                $activeSubmenuItem.closest('.nav-submenu').show();
                $activeSubmenuItem.closest('.dropdown').find('> a.nav-link.menu-title').addClass('active');
            }
        }, 100);
        
        // Alt menü elemanlarına tıklandığında yayılımı durdur
        $('.nav-submenu a').on('click', function(e) {
            e.stopPropagation();
        });
        
        // Mobil menü için geri butonu
        $('.mobile-back').click(function() {
            $('.dropdown > a').removeClass('active');
            $('.nav-submenu').slideUp(300);
        });

        // Sayfa yüklendiğinde loader'ı kaldır
        $('.loader-wrapper').fadeOut('slow');

        // DataTables için kontrol
        if (typeof $.fn.dataTable !== 'undefined') {
            // DataTable Türkçe dil ayarları
            $.extend(true, $.fn.dataTable.defaults, {
                language: {
                    url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/tr.json'
                },
                responsive: true,
                pageLength: 10,
                lengthMenu: [[5, 10, 25, 50, -1], [5, 10, 25, 50, "Tümü"]],
                order: [],
                columnDefs: [
                    { targets: 'no-sort', orderable: false }
                ]
            });
            
            // Var olan DataTable'ları başlat
            $('.table-datatable').DataTable();
        }
        
        // Select2 için kontrol
        if (typeof $.fn.select2 !== 'undefined') {
            $('.select2').select2({
                theme: 'bootstrap-5',
                language: 'tr',
                placeholder: 'Seçiniz...',
                allowClear: true
            });
        }
        
        // Tooltips
        if (typeof bootstrap.Tooltip !== 'undefined') {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
        
        // Popover
        if (typeof bootstrap.Popover !== 'undefined') {
            const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
            popoverTriggerList.map(function (popoverTriggerEl) {
                return new bootstrap.Popover(popoverTriggerEl);
            });
        }
    });
});

// Para birimi formatını yerelleştirme
function formatCurrency(value) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(value);
}

// Tarihi yerelleştirme
function formatDate(dateString) {
    const options = { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    };
    
    return new Date(dateString).toLocaleDateString('tr-TR', options);
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

    // Bootstrap Tooltip'leri etkinleştir
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });
    
    // Toastr ayarları
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
    
    // Select2 elementleri
    if($.fn.select2) {
        $('.select2').select2({
            theme: 'bootstrap-5'
        });
    }
    
    // Feather iconları
    if(typeof feather !== 'undefined') {
        feather.replace();
    }
    
    // DataTables
    if($.fn.DataTable) {
        $('.datatable').DataTable({
            "language": {
                "url": "//cdn.datatables.net/plug-ins/1.10.25/i18n/Turkish.json"
            },
            responsive: true
        });
    }
    
    // Web bildirimleri izin kontrolü
    if ('Notification' in window) {
        if (Notification.permission !== 'granted' && Notification.permission !== 'denied') {
            Notification.requestPermission();
        }
    }
});

// Genel yükleme göstergesi fonksiyonları
function showLoader() {
    if ($('.loader-wrapper').length === 0) {
        $('body').append('<div class="loader-wrapper"><div class="loader-p"></div></div>');
    }
    $('.loader-wrapper').fadeIn();
}

function hideLoader() {
    $('.loader-wrapper').fadeOut();
}

// Form doğrulama fonksiyonu
function validateForm(formSelector) {
    var isValid = true;
    
    // Gerekli alanları kontrol et
    $(formSelector + ' [required]').each(function() {
        if ($(this).val().trim() === '') {
            $(this).addClass('is-invalid');
            isValid = false;
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
    // E-posta doğrulama
    $(formSelector + ' [type="email"]').each(function() {
        var email = $(this).val().trim();
        if (email !== '' && !isValidEmail(email)) {
            $(this).addClass('is-invalid');
            isValid = false;
        }
    });
    
    return isValid;
}

// E-posta doğrulama
function isValidEmail(email) {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

// Bildirim sesi çalma
function playNotificationSound() {
    try {
        // Ses dosyasını başa sar ve çal
        var sound = new Audio('/sounds/notification.mp3');
        sound.currentTime = 0;
        sound.play().catch(function(error) {
            console.log("Bildirim sesi çalma hatası:", error);
        });
    } catch (error) {
        console.error("Ses çalma hatası:", error);
    }
}

// Desktop bildirim gösterme
function showDesktopNotification(title, message, options = {}) {
    if (!('Notification' in window)) {
        console.log('Bu tarayıcı web bildirimlerini desteklemiyor');
        return;
    }
    
    // İzin kontrolü
    if (Notification.permission === "granted") {
        var defaultOptions = {
            body: message,
            icon: '/images/notification-icon.png',
            tag: 'notification',
            sound: true
        };
        
        var notificationOptions = {...defaultOptions, ...options};
        var notification = new Notification(title, notificationOptions);
        
        notification.onclick = function() {
            window.focus();
            if (options.url) {
                window.location.href = options.url;
            }
            notification.close();
        };
        
        // Ses çal
        if (notificationOptions.sound) {
            playNotificationSound();
        }
        
        // 10 saniye sonra bildirimi kapat
        setTimeout(function() {
            notification.close();
        }, 10000);
    } else if (Notification.permission !== 'denied') {
        Notification.requestPermission().then(function(permission) {
            if (permission === "granted") {
                showDesktopNotification(title, message, options);
            }
        });
    }
}

