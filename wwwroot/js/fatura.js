function setDefaultCurrencyForCustomer(cariId) {
    if (!cariId) return;
    
    $.ajax({
        url: '/Cari/GetCariDetails',
        type: 'GET',
        data: { cariId: cariId },
        success: function(data) {
            if (data && data.paraBirimiID) {
                // Cari'nin para birimini seç
                $('#dovizTuruSelect option').each(function() {
                    if ($(this).val() == data.paraBirimiID) {
                        $(this).prop('selected', true);
                        
                        // Para birimi seçildiğinde kur bilgisini güncelle
                        updateExchangeRate(data.paraBirimiID);
                    }
                });
            } else {
                // Varsayılan para birimi (TRY) seç
                $('#dovizTuruSelect option').each(function() {
                    var isAnaParaBirimi = $(this).data('anapara') === "true";
                    if (isAnaParaBirimi) {
                        $(this).prop('selected', true);
                        $('#DovizKuru').val(1);
                    }
                });
            }
        },
        error: function(xhr, status, error) {
            console.error("Cari bilgileri alınamadı: " + error);
            
            // Hata durumunda varsayılan para birimi (TRY) seç
            $('#dovizTuruSelect option').each(function() {
                var isAnaParaBirimi = $(this).data('anapara') === "true";
                if (isAnaParaBirimi) {
                    $(this).prop('selected', true);
                    $('#DovizKuru').val(1);
                }
            });
        }
    });
}

function updateExchangeRate(paraBirimiID) {
    if (!paraBirimiID) return;
    
    // Seçilen option'dan para birimi kodunu al
    var selectedOption = $('#dovizTuruSelect option:selected');
    var isAnaParaBirimi = selectedOption.data('anapara') === "true";
    var paraBirimiKod = selectedOption.data('kod');
    
    // Ana para birimi ise kur 1 olarak ayarla
    if (isAnaParaBirimi) {
        $('#DovizKuru').val(1);
        return;
    }
    
    // Diğer para birimleri için güncel kur bilgisini al
    $.ajax({
        url: '/ParaBirimi/GetLatestExchangeRate',
        type: 'GET',
        data: { paraBirimiID: paraBirimiID },
        success: function(data) {
            if (data && data.kur) {
                $('#DovizKuru').val(data.kur.toFixed(4));
            } else {
                $('#DovizKuru').val(1);
                console.warn("Kur bilgisi alınamadı, varsayılan değer 1 olarak ayarlandı.");
            }
        },
        error: function(xhr, status, error) {
            $('#DovizKuru').val(1);
            console.error("Kur bilgisi alınırken hata oluştu: " + error);
        }
    });
}

// Cari seçildiğinde
$(document).on('change', '#CariID', function() {
    var cariId = $(this).val();
    setDefaultCurrencyForCustomer(cariId);
});

// Para birimi değiştiğinde
$(document).on('change', '#dovizTuruSelect', function() {
    var paraBirimiID = $(this).val();
    updateExchangeRate(paraBirimiID);
});

$(document).ready(function() {
    // Ürün seçildiğinde birim değerini otomatik dolduracak
    $('#urun').change(function() {
        var selectedOption = $(this).find('option:selected');
        var birimDegeri = selectedOption.data('birim');
        $('#birim').val(birimDegeri);
        
        // Birim fiyatı AJAX ile alınabilir veya manuel girilir
        fetchUrunBilgileri($(this).val());
    });
    
    // Ürün ve fiyat bilgisini getir
    function fetchUrunBilgileri(urunId) {
        if(!urunId) return;
        
        $.ajax({
            url: '/Urun/GetUrunFiyat',
            type: 'GET',
            data: { urunId: urunId },
            success: function(data) {
                if(data && data.birimFiyat) {
                    $('#birimFiyat').val(data.birimFiyat.toFixed(2));
                }
            },
            error: function(xhr, status, error) {
                console.error("Ürün bilgileri alınamadı: " + error);
            }
        });
    }
    
    // Ürün ekleme düğmesi
    $('#btnUrunEkle').click(function() {
        ekleUrun();
    });
    
    // Ürün ekleme fonksiyonu
    function ekleUrun() {
        var urunSelect = $('#urun');
        var urunId = urunSelect.val();
        var urunAdi = urunSelect.find('option:selected').text();
        var miktar = parseFloat($('#miktar').val());
        var birim = $('#birim').val();
        var birimFiyat = parseFloat($('#birimFiyat').val());
        var kdvOrani = parseFloat($('#kdvOrani').val());
        var indirimOrani = parseFloat($('#indirimOrani').val());
        
        if(!urunId || isNaN(miktar) || isNaN(birimFiyat)) {
            alert('Lütfen gerekli alanları doldurunuz.');
            return;
        }
        
        // Hesaplamalar
        var araToplam = miktar * birimFiyat;
        var indirimTutari = (araToplam * indirimOrani) / 100;
        var indirimliTutar = araToplam - indirimTutari;
        var kdvTutari = (indirimliTutar * kdvOrani) / 100;
        var genelToplam = indirimliTutar + kdvTutari;
        
        // Tabloya ekleme
        var index = $('#urunlerTablosu tbody tr').length;
        var newRow = `<tr data-urun-id="${urunId}">
            <td>${urunAdi}</td>
            <td>${miktar.toFixed(2)}</td>
            <td>${birim}</td>
            <td>${birimFiyat.toFixed(2)}</td>
            <td>${kdvOrani.toFixed(0)}</td>
            <td>${indirimOrani.toFixed(0)}</td>
            <td>${araToplam.toFixed(2)}</td>
            <td>${kdvTutari.toFixed(2)}</td>
            <td>${genelToplam.toFixed(2)}</td>
            <td>
                <button type="button" class="btn btn-danger btn-sm btnSilUrun">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
            <input type="hidden" name="FaturaKalemleri[${index}].UrunID" value="${urunId}" />
            <input type="hidden" name="FaturaKalemleri[${index}].Miktar" value="${miktar}" />
            <input type="hidden" name="FaturaKalemleri[${index}].BirimFiyat" value="${birimFiyat}" />
            <input type="hidden" name="FaturaKalemleri[${index}].KdvOrani" value="${kdvOrani}" />
            <input type="hidden" name="FaturaKalemleri[${index}].IndirimOrani" value="${indirimOrani}" />
        </tr>`;
        
        $('#urunlerTablosu tbody').append(newRow);
        
        // Formu sıfırla
        urunSelect.val('');
        $('#miktar').val('1');
        $('#birim').val('');
        $('#birimFiyat').val('');
        $('#kdvOrani').val('18');
        $('#indirimOrani').val('0');
        
        // Toplamları güncelle
        toplamlarıGuncelle();
    }
    
    // Ürün silme işlemi
    $(document).on('click', '.btnSilUrun', function() {
        $(this).closest('tr').remove();
        toplamlarıGuncelle();
    });
    
    // Toplamları hesapla
    function toplamlarıGuncelle() {
        var araToplam = 0;
        var kdvToplam = 0;
        var genelToplam = 0;
        
        $('#urunlerTablosu tbody tr').each(function() {
            araToplam += parseFloat($(this).find('td:eq(6)').text());
            kdvToplam += parseFloat($(this).find('td:eq(7)').text());
            genelToplam += parseFloat($(this).find('td:eq(8)').text());
        });
        
        $('#araToplam').text(araToplam.toFixed(2));
        $('#kdvToplam').text(kdvToplam.toFixed(2));
        $('#genelToplam').text(genelToplam.toFixed(2));
        
        // Gizli alanlara değerleri aktar
        $('#hiddenAraToplam').val(araToplam);
        $('#hiddenKdvToplam').val(kdvToplam);
        $('#hiddenGenelToplam').val(genelToplam);
    }
    
    // Güncel kur bilgisini getir
    $('#btnGetKur').click(function() {
        var dovizTuru = $('#dovizTuru').val();
        if(dovizTuru === 'TRY') {
            $('#dovizKuru').val(1);
            return;
        }
        
        $.ajax({
            url: '/ParaBirimi/GetLatestExchangeRate',
            type: 'GET',
            data: { kod: dovizTuru },
            success: function(data) {
                if(data && data.kur) {
                    $('#dovizKuru').val(data.kur.toFixed(4));
                }
            },
            error: function(xhr, status, error) {
                console.error("Kur bilgisi alınamadı: " + error);
                alert("Kur bilgisi alınamadı.");
            }
        });
    });
    
    // Cari değiştiğinde sözleşmeleri güncelle
    $('#cariSelect').change(function() {
        var cariId = $(this).val();
        if(!cariId) return;
        
        $.ajax({
            url: '/Sozlesme/GetSozlesmelerByCariId',
            type: 'GET',
            data: { cariId: cariId },
            success: function(data) {
                // Sözleşme dropdown'ını güncelle
                var sozlesmeDrop = $('#SozlesmeID');
                sozlesmeDrop.empty();
                sozlesmeDrop.append('<option value="">-- Sözleşme Seçin --</option>');
                
                if(data && data.length > 0) {
                    $.each(data, function(index, item) {
                        sozlesmeDrop.append(`<option value="${item.value}">${item.text}</option>`);
                    });
                }
            }
        });
    });
    
    // Form gönderilmeden önce JSON veriyi oluştur
    $('#faturaForm').submit(function(e) {
        var kalemler = [];
        
        if($('#urunlerTablosu tbody tr').length === 0) {
            alert("Lütfen en az bir ürün ekleyin.");
            e.preventDefault();
            return false;
        }
        
        $('#urunlerTablosu tbody tr').each(function(index) {
            var tr = $(this);
            var urunId = tr.data('urun-id');
            
            var kalem = {
                UrunID: urunId,
                Miktar: parseFloat(tr.find('input[name$=".Miktar"]').val()),
                BirimFiyat: parseFloat(tr.find('input[name$=".BirimFiyat"]').val()),
                KdvOrani: parseFloat(tr.find('input[name$=".KdvOrani"]').val()),
                IndirimOrani: parseFloat(tr.find('input[name$=".IndirimOrani"]').val())
            };
            
            kalemler.push(kalem);
        });
        
        $('#detaylarJson').val(JSON.stringify(kalemler));
    });
}); 