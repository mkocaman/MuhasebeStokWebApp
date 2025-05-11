# Muhasebe Stok Web Uygulaması - Manuel UI Test Kontrol Listesi

## Genel Kontroller
- [x] Uygulama başarıyla başlatılıyor
- [x] Dashboard sayfası görüntüleniyor
- [x] Menüler ve alt menüler doğru çalışıyor
- [x] Responsive tasarım farklı ekran boyutlarında düzgün görüntüleniyor
- [x] Kullanıcı girişi ve çıkışı doğru çalışıyor
- [ ] Kullanıcı yetkilendirme kontrolleri doğru çalışıyor
- [ ] Dil değiştirme seçeneği doğru çalışıyor

## Cari İşlemleri
- [x] Yeni cari oluşturma
  - [ ] Zorunlu alanlar boş bırakıldığında hata mesajı veriyor
  - [x] Tüm alanlar doldurulduğunda cari başarıyla oluşturuluyor
  - [ ] Vergi numarası, telefon ve e-posta formatlama kontrolleri doğru çalışıyor
- [ ] Cari listeleme
  - [ ] Cariler doğru şekilde filtreleniyor
  - [x] Sayfalama işlemleri doğru çalışıyor
  - [x] Sıralama işlemleri doğru çalışıyor
- [x] Cari güncelleme
  - [x] Var olan cari bilgileri doğru getiriliyor
  - [x] Değişiklikler başarıyla kaydediliyor
- [x] Cari silme
  - [x] Silme onay ekranı görüntüleniyor
  - [x] Silme işlemi başarıyla tamamlanıyor
- [x] Cari gruplandırma
  - [x] Müşteri/Tedarikçi/Pasif grupları doğru çalışıyor
  - [x] Gruplama filtreleri doğru listeleniyor
- [ ] Cari bakiye kontrolleri
  - [ ] Bakiye bilgileri doğru görüntüleniyor
  - [ ] Bakiye ekstre raporları doğru oluşturuluyor

## Ürün İşlemleri
- [x] Yeni ürün oluşturma
  - [x] Zorunlu alanlar boş bırakıldığında hata mesajı veriyor
  - [x] Tüm alanlar doldurulduğunda ürün başarıyla oluşturuluyor
  - [ ] Barkod, stok kodu ve birim fiyat formatlama kontrolleri doğru çalışıyor
- [x] Ürün listeleme
  - [x] Ürünler doğru şekilde filtreleniyor
  - [x] Sayfalama işlemleri doğru çalışıyor
  - [x] Sıralama işlemleri doğru çalışıyor
- [x] Ürün güncelleme
  - [x] Var olan ürün bilgileri doğru getiriliyor
  - [x] Değişiklikler başarıyla kaydediliyor
- [x] Ürün silme
  - [x] Silme onay ekranı görüntüleniyor
  - [x] Silme işlemi başarıyla tamamlanıyor
- [x] Ürün kategorileri yönetimi
  - [x] Yeni kategori oluşturulabiliyor
  - [x] Kategoriler düzenlenebiliyor
  - [x] Kategoriler silinebiliyor
- [ ] Ürün varyantları 
  - [ ] Varyant ekleme doğru çalışıyor
  - [ ] Varyant seçimi doğru çalışıyor
  - [ ] Fiyat farklılıkları doğru hesaplanıyor 

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
- [ ] Stok sayım işlemleri
  - [ ] Sayım formları doğru çalışıyor
  - [ ] Stok kontrolleri doğru yapılabiliyor

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
- [ ] İrsaliye faturalaştırma
  - [ ] İrsaliye faturaya dönüştürülebiliyor
  - [ ] Faturalaştırılan irsaliyeler doğru işaretleniyor
  - [ ] Faturalanan irsaliyeler tekrar faturalanamıyor

## Kasa İşlemleri
- [ ] Kasa giriş çıkış işlemleri
  - [ ] Kasa giriş çıkış formları doğru çalışıyor
  - [ ] İşlemler başarıyla kaydediliyor
- [ ] Kasa hareketleri listeleme
  - [ ] Kasa hareketleri doğru listeleniyor
  - [ ] Filtreler doğru çalışıyor
- [ ] Kasa açılış ve kapanış işlemleri
  - [ ] Günlük kasa açılışı doğru yapılabiliyor
  - [ ] Günlük kasa kapanışı doğru yapılabiliyor
  - [ ] Kasa farkları tespit edilebiliyor
- [ ] Kasa raporları
  - [ ] Günlük kasa raporu doğru oluşturuluyor
  - [ ] Tarih aralığı filtreleri doğru çalışıyor

## Banka İşlemleri
- [ ] Yeni banka hesabı oluşturma
  - [ ] Zorunlu alanlar boş bırakıldığında hata mesajı veriyor
  - [ ] Tüm alanlar doldurulduğunda banka hesabı başarıyla oluşturuluyor
  - [ ] IBAN ve hesap numarası formatlama kontrolleri doğru çalışıyor
- [ ] Banka hesabı listeleme
  - [ ] Banka hesapları doğru şekilde filtreleniyor
  - [ ] Sayfalama işlemleri doğru çalışıyor
  - [ ] Sıralama işlemleri doğru çalışıyor
- [ ] Banka hesabı güncelleme
  - [ ] Var olan banka hesabı bilgileri doğru getiriliyor
  - [ ] Değişiklikler başarıyla kaydediliyor
- [ ] Banka hesabı silme
  - [ ] Silme onay ekranı görüntüleniyor
  - [ ] Silme işlemi başarıyla tamamlanıyor
- [ ] Çek/Senet işlemleri
  - [ ] Çek/Senet giriş işlemleri doğru yapılabiliyor
  - [ ] Çek/Senet tahsilat/ödeme işlemleri doğru yapılabiliyor 
  - [ ] Vadeli işlemler doğru takip edilebiliyor
- [ ] Kredi kartı işlemleri
  - [ ] POS entegrasyonu doğru çalışıyor
  - [ ] Kredi kartı komisyon oranları doğru hesaplanıyor
  - [ ] Taksitli işlemler doğru kaydediliyor
- [ ] Banka hesabı giriş çıkış işlemleri
  - [ ] Para yatırma formu doğru çalışıyor
  - [ ] Para çekme formu doğru çalışıyor
  - [ ] İşlemler başarıyla kaydediliyor

## Banka Hareketleri
- [ ] Banka hareketleri listeleme
  - [ ] Banka hareketleri doğru listeleniyor
  - [ ] Filtreler doğru çalışıyor
  - [ ] Sayfalama işlemleri doğru çalışıyor
- [ ] Banka hareket detayları
  - [ ] Hareket detayları doğru görüntüleniyor
  - [ ] İlişkili belgeler doğru görüntüleniyor
- [ ] Döviz işlemleri
  - [ ] Döviz kurları doğru güncelleniyor
  - [ ] Döviz hesaplı işlemler doğru kaydediliyor
- [ ] Dekont yazdırma
  - [ ] Dekont formları doğru görüntüleniyor
  - [ ] Dekont çıktıları doğru alınabiliyor

## Cari Hareketleri
- [ ] Cari hareket oluşturma
  - [ ] Tahsilat işlemleri doğru çalışıyor
  - [ ] Ödeme işlemleri doğru çalışıyor
  - [ ] Para birimi seçenekleri doğru çalışıyor
  - [ ] Tutarlar doğru formatta gösteriliyor
- [ ] Cari hareketleri listeleme
  - [ ] Cari hareketleri doğru listeleniyor
  - [ ] Tarih aralığı filtreleri doğru çalışıyor
  - [ ] Hareket tipi filtreleri doğru çalışıyor
  - [ ] Sayfalama işlemleri doğru çalışıyor
- [ ] Cari hareket güncelleme
  - [ ] Var olan cari hareket bilgileri doğru getiriliyor
  - [ ] Değişiklikler başarıyla kaydediliyor
- [ ] Cari hareket silme
  - [ ] Silme onay ekranı görüntüleniyor
  - [ ] Silme işlemi başarıyla tamamlanıyor
- [ ] Mahsup işlemleri
  - [ ] Borç/alacak mahsup işlemleri doğru yapılabiliyor
  - [ ] Mahsup fişleri doğru oluşturuluyor
- [ ] Açık hesap takibi
  - [ ] Açık hesaplar doğru listeleniyor
  - [ ] Vade tarihleri doğru takip ediliyor

## Todo Modülü
- [ ] Görev oluşturma
  - [ ] Yeni görev başlığı ve açıklaması girilebiliyor
  - [ ] Öncelik seviyesi seçilebiliyor
  - [ ] Son tarih belirlenebiliyor
  - [ ] Sorumlu kişi atanabiliyor
  - [ ] Görev başarıyla kaydediliyor
- [ ] Görev listeleme
  - [ ] Görevler doğru şekilde listeleniyor
  - [ ] Tamamlanan/tamamlanmayan görev filtreleri çalışıyor
  - [ ] Öncelik filtreleri doğru çalışıyor
  - [ ] Görevler son tarihe göre sıralanabiliyor
- [ ] Görev güncelleme
  - [ ] Görev detayları doğru getiriliyor
  - [ ] Görev tamamlandı olarak işaretlenebiliyor
  - [ ] Görev bilgileri güncellenebiliyor
- [ ] Görev silme
  - [ ] Silme onay ekranı görüntüleniyor
  - [ ] Silme işlemi başarıyla tamamlanıyor
- [ ] Görev bildirimleri
  - [ ] Yaklaşan son tarihli görevler için bildirimler görüntüleniyor
  - [ ] Bildirimler doğru zamanda tetikleniyor
- [ ] Ekip görevleri
  - [ ] Görevler birden fazla kişiye atanabiliyor
  - [ ] Ekip üyeleri görevleri görebiliyor
  - [ ] Ekip içi yorum ve bildirimler doğru çalışıyor
- [ ] Görev etiketleri
  - [ ] Etiketler oluşturulabiliyor ve düzenlenebiliyor
  - [ ] Görevler etiketlere göre filtrelenebiliyor
  - [ ] Etiket bazlı raporlar alınabiliyor
- [ ] Takvim görünümü
  - [ ] Görevler takvimde doğru görüntüleniyor
  - [ ] Tarihe göre görevler filtrelenebiliyor
- [ ] Görev yorumları
  - [ ] Görevlere yorum eklenebiliyor
  - [ ] Yorumlar görüntülenebiliyor ve silinebiliyor

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
- [ ] Finansal raporlar
  - [ ] Gelir-gider raporları doğru oluşturuluyor
  - [ ] Nakit akış raporları doğru oluşturuluyor
- [ ] İşlem raporları
  - [ ] Kullanıcı işlem logları doğru görüntüleniyor
  - [ ] Tarih bazlı log filtreleme doğru çalışıyor

## Formatlama Testleri
- [ ] USD ve UZS değerlerinin formatlaması
  - [x] 6,72 $ yerine 672 $ doğru şekilde gösteriliyor
  - [x] 56.000,00 UZS yerine 5.600.000,00 so'm doğru şekilde gösteriliyor
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

## Veri Yedekleme ve Geri Yükleme
- [ ] Manuel yedekleme
  - [ ] Veritabanı yedeklemesi başarıyla gerçekleştiriliyor
  - [ ] Yedekleme dosyaları doğru oluşturuluyor
- [ ] Otomatik yedekleme
  - [ ] Zamanlı yedekleme doğru çalışıyor
  - [ ] Yedekleme bildirimleri doğru gönderiliyor
- [ ] Geri yükleme
  - [ ] Yedekten geri yükleme başarıyla gerçekleştiriliyor
  - [ ] Veri bütünlüğü kontrolleri doğru çalışıyor

## Sistem Ayarları
- [ ] Genel ayarlar
  - [ ] Firma bilgileri düzenlenebiliyor
  - [ ] Para birimi ayarları doğru çalışıyor
  - [ ] Vergi oranları düzenlenebiliyor
- [ ] Kullanıcı yönetimi
  - [ ] Yeni kullanıcı ekleme doğru çalışıyor
  - [ ] Kullanıcı yetkilendirme doğru çalışıyor
  - [ ] Kullanıcı şifre sıfırlama doğru çalışıyor
- [ ] Otomatik e-posta bildirimleri
  - [ ] Bildirim ayarları düzenlenebiliyor
  - [ ] Bildirimler doğru gönderiliyor 