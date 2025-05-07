/*! -----------------------------------------------------------------------------------

    Description: Bu dosya dashboard için grafikler ve arayüz işlevlerini içerir
    Author: Dashboard Team
    Version: 1.0

-----------------------------------------------------------------------------------*/

(function($) {
    "use strict";
    
    // Widget etkinleştirme
    $('.setting-option').each(function() {
        $(this).find('.setting-primary').on('click', function() {
            $(this).parent().parent('.setting-option').toggleClass('open-setting');
        });
    });
    
    // Gerçek zamanlı güncelleme için tarih ve saat
    setInterval(function(){
        var now = new Date();
        var hours = now.getHours();
        var minutes = now.getMinutes();
        var seconds = now.getSeconds();
        
        // Saatleri 01, 02, vb. formatına dönüştür
        hours = hours < 10 ? '0' + hours : hours;
        minutes = minutes < 10 ? '0' + minutes : minutes;
        seconds = seconds < 10 ? '0' + seconds : seconds;
        
        $('.live-time').text(hours + ':' + minutes + ':' + seconds);
    }, 1000);
    
    // Dashboard Timeline Grafiği
    var options1 = {
        chart: {
            height: 350,
            type: 'area',
            toolbar: {
                show: false
            },
            fontFamily: "'Montserrat', sans-serif",
            zoom: {
                enabled: false
            }
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: 3
        },
        xaxis: {
            type: 'datetime',
            categories: ["2023-09-19T00:00:00", "2023-09-19T01:30:00", "2023-09-19T02:30:00", "2023-09-19T03:30:00", "2023-09-19T04:30:00", "2023-09-19T05:30:00", "2023-09-19T06:30:00"]
        },
        tooltip: {
            x: {
                format: 'dd/MM/yy HH:mm'
            }
        },
        colors: ['#5baf61', '#e2614f'],
        fill: {
            type: 'gradient',
            gradient: {
                shadeIntensity: 1,
                opacityFrom: 0.7,
                opacityTo: 0.5,
                stops: [0, 90, 100]
            }
        },
        series: [{
            name: 'Gelir',
            data: [45, 52, 38, 45, 19, 33, 58]
        }, {
            name: 'Gider',
            data: [35, 41, 62, 42, 13, 18, 29]
        }],
        grid: {
            borderColor: '#f1f1f1',
            padding: {
                bottom: 5
            }
        }
    };
    
    var chart1 = '';
    if (document.getElementById("chart-timeline-dashbord")) {
        chart1 = new ApexCharts(document.getElementById("chart-timeline-dashbord"), options1);
        chart1.render();
        window.chart1 = chart1; // Global referans oluştur
    }
    
    // Dashboard Bar Grafiği
    var options2 = {
        chart: {
            height: 350,
            type: 'bar',
            toolbar: {
                show: false
            },
            fontFamily: "'Montserrat', sans-serif",
            zoom: {
                enabled: false
            }
        },
        plotOptions: {
            bar: {
                horizontal: false,
                columnWidth: '55%',
                endingShape: 'rounded',
                borderRadius: 4
            },
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            show: true,
            width: 2,
            colors: ['transparent']
        },
        xaxis: {
            categories: ['Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki'],
        },
        yaxis: {
            title: {
                text: 'TL',
                style: {
                    color: '#4f6479',
                    fontSize: '12px',
                    fontFamily: "'Montserrat', sans-serif",
                    fontWeight: 500
                }
            }
        },
        fill: {
            opacity: 1
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val + " TL"
                }
            }
        },
        colors: ['#5baf61', '#e2614f', '#4f6479'],
        series: [{
            name: 'Net Kazanç',
            data: [44, 55, 57, 56, 61, 58, 63, 60, 66]
        }, {
            name: 'Gelir',
            data: [76, 85, 101, 98, 87, 105, 91, 114, 94]
        }, {
            name: 'Serbest Nakit Akışı',
            data: [35, 41, 36, 26, 45, 48, 52, 53, 41]
        }],
        legend: {
            position: 'top',
            horizontalAlign: 'right',
            fontSize: '14px',
            fontFamily: "'Montserrat', sans-serif",
            markers: {
                width: 10,
                height: 10
            }
        },
        grid: {
            borderColor: '#f1f1f1',
            padding: {
                bottom: 5
            }
        }
    };
    
    var chart2 = '';
    if (document.getElementById("chart-dashbord")) {
        chart2 = new ApexCharts(document.getElementById("chart-dashbord"), options2);
        chart2.render();
        window.chart2 = chart2; // Global referans oluştur
    }
    
    // Kullanıcı Aktiviteleri Grafiği
    var options3 = {
        chart: {
            height: 350,
            type: 'line',
            toolbar: {
                show: false
            },
            fontFamily: "'Montserrat', sans-serif",
            zoom: {
                enabled: false
            }
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: 3
        },
        grid: {
            row: {
                colors: ['#f8f8f8', 'transparent'],
                opacity: 0.5
            },
            borderColor: '#f1f1f1',
            padding: {
                bottom: 5
            }
        },
        xaxis: {
            categories: ['Oca', 'Mar', 'May', 'Tem', 'Eyl', 'Kas'],
            tickAmount: 5,
            tickPlacement: 'between',
            axisTicks: {
                show: false
            },
            axisBorder: {
                show: false
            }
        },
        fill: {
            type: 'gradient',
            gradient: {
                shade: 'dark',
                gradientToColors: ['#5baf61'],
                shadeIntensity: 1,
                type: 'horizontal',
                opacityFrom: 1,
                opacityTo: 1,
                colorStops: [
                    {
                        offset: 0,
                        color: "#4f6479",
                        opacity: 1
                    },
                    {
                        offset: 100,
                        color: "#5baf61",
                        opacity: 1
                    },
                ]
            },
        },
        colors: ['#4f6479'],
        series: [{
            name: 'Aktif Kullanıcı',
            data: [12, 14, 15, 14, 13, 12]
        }],
        tooltip: {
            y: {
                formatter: function(val) {
                    return val + "k";
                }
            }
        },
        legend: {
            fontSize: '14px',
            fontFamily: "'Montserrat', sans-serif",
            markers: {
                width: 10,
                height: 10
            }
        }
    };
    
    var chart3 = '';
    if (document.getElementById("user-activation-dash-2")) {
        chart3 = new ApexCharts(document.getElementById("user-activation-dash-2"), options3);
        chart3.render();
        window.chart3 = chart3; // Global referans oluştur
    }
    
    // İşlemler Grafiği
    var options4 = {
        chart: {
            height: 350,
            type: 'area',
            toolbar: {
                show: false
            },
            fontFamily: "'Montserrat', sans-serif",
            zoom: {
                enabled: false
            }
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: 2
        },
        grid: {
            borderColor: '#f1f1f1',
            padding: {
                bottom: 5
            }
        },
        xaxis: {
            categories: ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara'],
            labels: {
                style: {
                    colors: '#a9a9a9',
                    fontSize: '12px',
                    fontFamily: "'Montserrat', sans-serif"
                }
            },
            axisBorder: {
                show: false
            },
            axisTicks: {
                show: false
            }
        },
        yaxis: {
            labels: {
                style: {
                    colors: '#a9a9a9',
                    fontSize: '12px',
                    fontFamily: "'Montserrat', sans-serif"
                }
            }
        },
        fill: {
            type: 'gradient',
            gradient: {
                shadeIntensity: 1,
                opacityFrom: 0.7,
                opacityTo: 0.5,
                stops: [0, 90, 100]
            }
        },
        colors: ['#e2614f'],
        series: [{
            name: 'İşlemler',
            data: [25, 30, 35, 28, 28, 33, 35, 33, 36, 34, 30, 38]
        }],
        tooltip: {
            y: {
                formatter: function(val) {
                    return val + " işlem"
                }
            }
        },
        legend: {
            fontSize: '14px',
            fontFamily: "'Montserrat', sans-serif",
            markers: {
                width: 10,
                height: 10
            }
        }
    };
    
    var chart4 = '';
    if (document.getElementById("chart-3dash")) {
        chart4 = new ApexCharts(document.getElementById("chart-3dash"), options4);
        chart4.render();
        window.chart4 = chart4; // Global referans oluştur
    }
    
    // Konfeti efekti
    function animateConfetti() {
        $('.confetti-piece').each(function(index) {
            var $this = $(this);
            var delay = index * 150; // Her parça için farklı gecikme
            setTimeout(function() {
                var randomX = (Math.random() * 100) + '%';
                var randomRotate = (Math.random() * 360) + 'deg';
                var randomScale = Math.random() * 0.8 + 0.2; // 0.2 - 1.0 arası
                
                $this.css({
                    'top': '-50px',
                    'left': randomX,
                    'transform': 'rotate(' + randomRotate + ') scale(' + randomScale + ')',
                    'opacity': 1
                }).animate({
                    'top': '120%',
                    'opacity': 0
                }, {
                    duration: 3000 + Math.random() * 2000,
                    easing: 'linear',
                    complete: function() {
                        // Animasyon tamamlandığında tekrar başlat
                        $(this).css('top', '-50px');
                        animateConfetti();
                    }
                });
            }, delay);
        });
    }
    
    // Sayfa yüklendiğinde konfeti animasyonunu başlat
    $(document).ready(function() {
        animateConfetti();
        
        // Widget ayarları
        $('.setting-option').on('click', function(e) {
            e.stopPropagation();
        });
        
        // Tema geçişi için
        var bodyElement = $('body');
        if (localStorage.getItem('dark-mode') === 'true') {
            bodyElement.addClass('dark-only');
            $('.mode i').removeClass("fa-moon-o").addClass("fa-lightbulb-o");
        }
        
        // Sidebar toggle
        $(".sidebar-toggle-icon").click(function() {
            $(".page-body-wrapper").toggleClass("sidebar-close");
        });
        
        // Card içinde tüm seçeneklerin işlevselliği
        $('.card-option li').on('click', function(e) {
            e.stopPropagation();
        });
        
        // Tema geçişlerinde grafikleri yeniden render et
        $('.mode').on('click', function() {
            setTimeout(function() {
                var bodyElement = $('body');
                var isDarkMode = bodyElement.hasClass('dark-only');
                
                // Karanlık/aydınlık tema için renk değişimleri
                var gridColor = isDarkMode ? '#363636' : '#f1f1f1';
                var textColor = isDarkMode ? '#eaedef' : '#a9a9a9';
                
                // Tüm grafikleri güncelle
                if (chart1 !== '') {
                    chart1.updateOptions({
                        grid: { borderColor: gridColor },
                        xaxis: { 
                            labels: { style: { colors: textColor } } 
                        },
                        yaxis: { 
                            labels: { style: { colors: textColor } },
                            title: { style: { color: textColor } }
                        }
                    });
                }
                
                if (chart2 !== '') {
                    chart2.updateOptions({
                        grid: { borderColor: gridColor },
                        xaxis: { 
                            labels: { style: { colors: textColor } } 
                        },
                        yaxis: { 
                            labels: { style: { colors: textColor } },
                            title: { style: { color: textColor } }
                        }
                    });
                }
                
                if (chart3 !== '') {
                    chart3.updateOptions({
                        grid: { 
                            borderColor: gridColor,
                            row: { colors: [isDarkMode ? '#2c2c43' : '#f8f8f8', 'transparent'] }
                        },
                        xaxis: { 
                            labels: { style: { colors: textColor } } 
                        },
                        yaxis: { 
                            labels: { style: { colors: textColor } }
                        }
                    });
                }
                
                if (chart4 !== '') {
                    chart4.updateOptions({
                        grid: { borderColor: gridColor },
                        xaxis: { 
                            labels: { style: { colors: textColor } } 
                        },
                        yaxis: { 
                            labels: { style: { colors: textColor } }
                        }
                    });
                }
            }, 300);
        });
        
        // Sayfa yüklendikten sonra tema kontrolü
        var isDarkMode = $('body').hasClass('dark-only');
        if (isDarkMode) {
            // Dark mode için grafik ayarları
            var gridColor = '#363636';
            var textColor = '#eaedef';
            
            setTimeout(function() {
                // Tüm grafikleri güncelle
                if (chart1) chart1.updateOptions({
                    grid: { borderColor: gridColor },
                    xaxis: { labels: { style: { colors: textColor } } },
                    yaxis: { 
                        labels: { style: { colors: textColor } },
                        title: { style: { color: textColor } }
                    }
                });
                
                if (chart2) chart2.updateOptions({
                    grid: { borderColor: gridColor },
                    xaxis: { labels: { style: { colors: textColor } } },
                    yaxis: { 
                        labels: { style: { colors: textColor } },
                        title: { style: { color: textColor } }
                    }
                });
                
                if (chart3) chart3.updateOptions({
                    grid: { 
                        borderColor: gridColor,
                        row: { colors: ['#2c2c43', 'transparent'] }
                    },
                    xaxis: { labels: { style: { colors: textColor } } },
                    yaxis: { labels: { style: { colors: textColor } } }
                });
                
                if (chart4) chart4.updateOptions({
                    grid: { borderColor: gridColor },
                    xaxis: { labels: { style: { colors: textColor } } },
                    yaxis: { labels: { style: { colors: textColor } } }
                });
            }, 500);
        }
    });
    
})(jQuery);