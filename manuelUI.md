# Muhasebe Stok Web Uygulaması - Manuel UI Test Kontrol Listesi

## Genel Kontroller
- [x] Uygulama başarıyla başlatılıyor
- [x] Dashboard sayfası görüntüleniyor
- [x] Menüler ve alt menüler doğru çalışıyor
- [x] Responsive tasarım farklı ekran boyutlarında düzgün görüntüleniyor

## Cari İşlemleri
- [ ] Yeni cari oluşturma
  - [ ] Zorunlu alanlar boş bırakıldığında hata mesajı veriyor
  - [ ] Tüm alanlar doldurulduğunda cari başarıyla oluşturuluyor
  - [ ] Vergi numarası, telefon ve e-posta formatlama kontrolleri doğru çalışıyor
- [ ] Cari listeleme
  - [ ] Cariler doğru şekilde filtreleniyor
  - [ ] Sayfalama işlemleri doğru çalışıyor
  - [ ] Sıralama işlemleri doğru çalışıyor
- [ ] Cari güncelleme
  - [ ] Var olan cari bilgileri doğru getiriliyor
  - [ ] Değişiklikler başarıyla kaydediliyor
- [ ] Cari silme
  - [ ] Silme onay ekranı görüntüleniyor
  - [ ] Silme işlemi başarıyla tamamlanıyor

## Ürün İşlemleri
- [ ] Yeni ürün oluşturma
  - [ ] Zorunlu alanlar boş bırakıldığında hata mesajı veriyor
  - [ ] Tüm alanlar doldurulduğunda ürün başarıyla oluşturuluyor
  - [ ] Barkod, stok kodu ve birim fiyat formatlama kontrolleri doğru çalışıyor
- [ ] Ürün listeleme
  - [ ] Ürünler doğru şekilde filtreleniyor
  - [ ] Sayfalama işlemleri doğru çalışıyor
  - [ ] Sıralama işlemleri doğru çalışıyor
- [ ] Ürün güncelleme
  - [ ] Var olan ürün bilgileri doğru getiriliyor
  - [ ] Değişiklikler başarıyla kaydediliyor
- [ ] Ürün silme
  - [ ] Silme onay ekranı görüntüleniyor
  - [ ] Silme işlemi başarıyla tamamlanıyor

## Fatura İşlemleri
- [ ] Yeni fatura oluşturma
  - [ ] Cari seçimi doğru çalışıyor
  - [ ] Ürün seçimi ve kalem ekleme doğru çalışıyor
  - [ ] Miktar, birim fiyat girildiğinde tutar hesaplamaları doğru yapılıyor
  - [ ] KDV hesaplamaları doğru yapılıyor
  - [ ] Para birimi (USD/UZS) seçimi ve kur hesaplamaları doğru çalışıyor:
     - [ ] USD seçildiğinde tutarlar USD olarak gösteriliyor, UZS değerleri otomatik hesaplanıyor
     - [ ] UZS seçildiğinde tutarlar UZS olarak gösteriliyor, USD değerleri otomatik hesaplanıyor
  - [ ] Toplam tutarlar doğru hesaplanıyor
  - [ ] Formatlama sorunları (6,72 $ gibi gösterilmesi gereken 672 $ değerlerinin) doğru gösteriliyor
  - [ ] Fatura başarıyla kaydediliyor
- [ ] Fatura listeleme
  - [ ] Faturalar doğru şekilde filtreleniyor
  - [ ] Tutarlar doğru formatta görüntüleniyor
  - [ ] Sayfalama işlemleri doğru çalışıyor
  - [ ] Sıralama işlemleri doğru çalışıyor
- [ ] Fatura detay görüntüleme
  - [ ] Fatura bilgileri doğru getiriliyor
  - [ ] Kalemler doğru listeleniyor
  - [ ] Tutarlar ve para birimleri doğru formatta görüntüleniyor
- [ ] Fatura güncelleme
  - [ ] Var olan fatura bilgileri doğru getiriliyor
  - [ ] Kalemler eklenip çıkarılabiliyor
  - [ ] Değişiklikler başarıyla kaydediliyor
- [ ] Fatura silme
  - [ ] Silme onay ekranı görüntüleniyor
  - [ ] Silme işlemi başarıyla tamamlanıyor
- [ ] Fatura ödeme işlemleri
  - [ ] Ödeme eklenebiliyor
  - [ ] Ödeme miktarı doğru kaydediliyor
  - [ ] Fatura ödeme durumu doğru güncelleniyor

## Stok İşlemleri
- [ ] Stok giriş işlemleri
  - [ ] Stok giriş formları doğru çalışıyor
  - [ ] Stok girişi başarıyla kaydediliyor
- [ ] Stok çıkış işlemleri
  - [ ] Stok çıkış formları doğru çalışıyor
  - [ ] Stok çıkışı başarıyla kaydediliyor
- [ ] Stok hareketleri listeleme
  - [ ] Stok hareketleri doğru listeleniyor
  - [ ] Filtreler doğru çalışıyor

## İrsaliye İşlemleri
- [ ] Yeni irsaliye oluşturma
  - [ ] Cari seçimi doğru çalışıyor
  - [ ] Ürün seçimi ve kalem ekleme doğru çalışıyor
  - [ ] İrsaliye başarıyla kaydediliyor
- [ ] İrsaliye listeleme
  - [ ] İrsaliyeler doğru şekilde filtreleniyor
  - [ ] Sayfalama işlemleri doğru çalışıyor
- [ ] İrsaliye detay görüntüleme
  - [ ] İrsaliye bilgileri doğru getiriliyor
  - [ ] Kalemler doğru listeleniyor
- [ ] İrsaliye güncelleme
  - [ ] Var olan irsaliye bilgileri doğru getiriliyor
  - [ ] Değişiklikler başarıyla kaydediliyor
- [ ] İrsaliye silme
  - [ ] Silme onay ekranı görüntüleniyor
  - [ ] Silme işlemi başarıyla tamamlanıyor

## Kasa İşlemleri
- [ ] Kasa giriş çıkış işlemleri
  - [ ] Kasa giriş çıkış formları doğru çalışıyor
  - [ ] İşlemler başarıyla kaydediliyor
- [ ] Kasa hareketleri listeleme
  - [ ] Kasa hareketleri doğru listeleniyor
  - [ ] Filtreler doğru çalışıyor

## Raporlama
- [ ] Cari raporları
  - [ ] Cari ekstre raporları doğru oluşturuluyor
  - [ ] Cari bakiye raporları doğru oluşturuluyor
- [ ] Stok raporları
  - [ ] Stok bakiye raporları doğru oluşturuluyor
  - [ ] Stok hareket raporları doğru oluşturuluyor
- [ ] Satış raporları
  - [ ] Satış raporları doğru oluşturuluyor
  - [ ] Filtreler doğru çalışıyor
- [ ] Özet raporlar
  - [ ] Dashboard raporları doğru oluşturuluyor
  - [ ] Grafik ve istatistikler doğru görüntüleniyor

## Formatlama Testleri
- [ ] USD ve UZS değerlerinin formatlaması
  - [ ] 6,72 $ yerine 672 $ doğru şekilde gösteriliyor
  - [ ] 56.000,00 UZS yerine 5.600.000,00 so'm doğru şekilde gösteriliyor
  - [ ] Kalem detay ekranlarında birim fiyatlar doğru formatta gösteriliyor
  - [ ] Toplam tutarlar doğru formatta gösteriliyor
  - [ ] Döviz hesaplamaları doğru yapılıyor

## Hata İşleme
- [ ] Ağ bağlantısı olmadığında uygun mesaj gösteriliyor
- [ ] Sunucu hataları kullanıcıya anlaşılır şekilde gösteriliyor
- [ ] Form doğrulama hataları doğru gösteriliyor
- [ ] Oturum kontrolü doğru çalışıyor

## Performans Testleri
- [ ] Sayfa yükleme süreleri makul
- [ ] Büyük veri setleri ile çalışırken performans kabul edilebilir
- [ ] Çok sayıda kalem içeren faturalar sorunsuz kaydedilebiliyor 