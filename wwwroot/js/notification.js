// Bildirim Hub Bağlantısı
var notificationConnection;
var notificationSound;
var soundInitialized = false;

$(document).ready(function() {
    // Ses dosyasını yükle
    notificationSound = new Audio('/sounds/notification.mp3');
    
    // Yükleme sırasında hata olursa
    notificationSound.onerror = function() {
        console.error("Bildirim sesi yüklenirken hata oluştu.");
    };
    
    // Ses yüklendi mi kontrol et
    notificationSound.oncanplaythrough = function() {
        console.log("Bildirim sesi başarıyla yüklendi.");
        soundInitialized = true;
    };

    // SignalR Bağlantısı
    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    // Bağlantı başlatma
    startNotificationConnection();

    // Bildirim alındığında
    notificationConnection.on("ReceiveNotification", function (notification) {
        console.log("Bildirim alındı:", notification);
        
        // Bildirim tipine göre toastr mesajı göster
        switch (notification.type) {
            case "success":
                toastr.success(notification.message, notification.title);
                break;
            case "error":
                toastr.error(notification.message, notification.title);
                break;
            case "warning":
                toastr.warning(notification.message, notification.title);
                break;
            case "TodoReminder":
                toastr.warning(
                    notification.message + '<br><a href="' + notification.url + '" class="btn btn-sm btn-warning mt-2">Göreve Git</a>', 
                    notification.title, 
                    {
                        timeOut: 0,
                        extendedTimeOut: 0,
                        closeButton: true,
                        tapToDismiss: false
                    }
                );
                
                // Sadece TodoReminder tipindeki bildirimlerde ses çal
                playNotificationSound();
                
                // Masaüstü bildirim göster
                checkNotificationPermission().then(function(permission) {
                    if (permission === "granted") {
                        showDesktopNotification(notification.title, notification.message);
                    }
                });
                break;
            default:
                toastr.info(notification.message, notification.title);
                break;
        }
    });
    
    // Görev Hatırlatma Bildirimi
    notificationConnection.on("ReceiveTodoReminder", function (reminder) {
        console.log("Hatırlatma alındı:", reminder);
        
        // Desktop bildirim izni kontrol et
        checkNotificationPermission().then(function(permission) {
            if (permission === "granted") {
                // Masaüstü bildirimi göster
                showDesktopNotification(reminder.title, reminder.message);
            }
        });
        
        // Toastr bildirim göster
        toastr.warning(
            reminder.message + '<br><a href="' + reminder.url + '" class="btn btn-sm btn-warning mt-2">Göreve Git</a>', 
            reminder.title, 
            {
                timeOut: 0,
                extendedTimeOut: 0,
                closeButton: true,
                tapToDismiss: false
            }
        );
        
        // Ses çal - sadece hatırlatıcı bildirimlerde
        playNotificationSound();
    });
});

// SignalR Bağlantısını Başlat
function startNotificationConnection() {
    notificationConnection.start()
        .then(function() {
            console.log("SignalR NotificationHub'a bağlandı");
        })
        .catch(function(err) {
            console.error("SignalR bağlantı hatası: ", err.toString());
            // 5 saniye sonra tekrar bağlanmayı dene
            setTimeout(startNotificationConnection, 5000);
        });
}

// Bildirim sesi çalma - sadece hatırlatıcılar için
function playNotificationSound() {
    try {
        // Ses başlatılmış mı kontrol et
        if (!soundInitialized) {
            console.warn("Ses dosyası henüz yüklenmemiş.");
            return;
        }
        
        // Ses dosyasını başa sar ve çal
        if (notificationSound && notificationSound.readyState >= 2) { // HAVE_CURRENT_DATA or higher
            notificationSound.currentTime = 0;
            var playPromise = notificationSound.play();
            
            if (playPromise !== undefined) {
                playPromise.then(function() {
                    console.log("Bildirim sesi başarıyla çalındı");
                }).catch(function(error) {
                    console.error("Bildirim sesi çalınamadı:", error);
                    
                    // Kullanıcı etkileşimi sonrasında çalmayı dene (tarayıcı politikası)
                    document.addEventListener('click', function clickHandler() {
                        notificationSound.play().catch(function(e) {
                            console.error("Kullanıcı etkileşimi sonrası ses çalınamadı:", e);
                        });
                        document.removeEventListener('click', clickHandler);
                    }, { once: true });
                });
            }
        } else {
            console.warn("Ses dosyası henüz yüklenmemiş veya hazır değil.");
        }
    } catch (error) {
        console.error("Ses çalma hatası:", error);
    }
}

// Desktop bildirim izni kontrolü
function checkNotificationPermission() {
    return new Promise(function(resolve, reject) {
        if (!('Notification' in window)) {
            console.log('Bu tarayıcı web bildirimlerini desteklemiyor');
            resolve("denied");
            return;
        }
        
        if (Notification.permission === "granted") {
            resolve("granted");
        } else if (Notification.permission !== "denied") {
            Notification.requestPermission().then(function(permission) {
                resolve(permission);
            });
        } else {
            resolve(Notification.permission);
        }
    });
}

// Masaüstü bildirimi gösterme
function showDesktopNotification(title, message) {
    if (Notification.permission === "granted") {
        var notification = new Notification(title, {
            body: message,
            icon: '/images/notification-icon.png',
            tag: 'todo-reminder',
            requireInteraction: true // Kullanıcı etkileşim yapana kadar bildirim kalıcı olur
        });
        
        notification.onclick = function() {
            window.focus();
            notification.close();
        };
        
        // 30 saniye sonra bildirimi kapat
        setTimeout(function() {
            notification.close();
        }, 30000);
    }
} 