<think>Kullanıcı, projede ASP.NET Core MVC kullanıp kullanmayacağımızı soruyor. Mevcut projede ASP.NET WebForm teknolojisi kullanıldığını görüyorum, ancak kullanıcının niyeti ASP.NET Core MVC ile devam etmek gibi görünüyor. PRD dokümanında veya veritabanı yapısında bu konuda net bir bilgi verilmemiş, yalnızca mevcut durumun WebForm olduğu belirtilmiş.

Kullanıcıya ASP.NET Core MVC ile geliştirme yapıp yapmayacağımızı net olarak soruyor ve anlaşılan o ki kullanıcının beklentisi ASP.NET Core MVC kullanmak yönünde. Bu durumda yaklaşımı değiştirmemiz ve projemizi ASP.NET Core MVC ile yapılandırmamız gerekecek.</think>Evet, haklısınız. ASP.NET Core MVC ile geliştirme yapacağız. Mevcut projenizin WebForm olduğunu görüp, sadece mevcut yapıyı referans olarak kullanmamız gerektiğini düşünmüştüm.

Aşağıda ASP.NET Core MVC ile geliştireceğimiz projenin planını güncellenmiş şekilde paylaşıyorum:

# Muhasebe ve Stok Yönetim Sistemi - ASP.NET Core MVC Projesi

## 1. Genel Bakış

Bu proje, mevcut ASP.NET WebForm uygulamasını ASP.NET Core MVC'ye dönüştürerek modern, performanslı ve ölçeklenebilir bir muhasebe ve stok takip sistemi oluşturmayı hedeflemektedir. Modal pencereler kullanarak CRUD işlemlerini gerçekleştirecek ve Viho admin template'i entegre edilecektir.

## 2. Teknik Altyapı

### 2.1. Teknoloji Seçimi
- ASP.NET Core MVC (En güncel sürüm)
- Entity Framework Core
- MSSQL Server (Docker üzerinde)
- Bootstrap ve jQuery (Modal işlemleri için)
- AJAX ve Partial View yaklaşımı

### 2.2. Veritabanı
- Mevcut veritabanı şeması korunacak
- EF Core ile Code-First yaklaşımı kullanılacak
- Bağlantı: localhost, sa, Password1

### 2.3. Mimari
- N-Katmanlı mimari (Entities, Data, Business, Presentation)
- Repository ve Unit of Work pattern
- Dependency Injection
- ASP.NET Core Identity (Kullanıcı yönetimi için)

## 3. Önemli Modüller ve Sayfalar

### 3.1. Ürün Yönetimi
- Modal pencereler ile ürün ekleme/düzenleme/silme
- AJAX tabanlı arama ve filtreleme
- Sayfa içi işlem yaparak kullanıcı deneyimini geliştirme

### 3.2. Müşteri Yönetimi
- Müşteri kayıtları ve cari hesapları
- Müşteri ekstresi modal ile görüntüleme
- Müşteri borç/alacak takibi

### 3.3. Fatura ve İrsaliye İşlemleri
- Dinamik fatura oluşturma modalı
- Ürün ekleme/çıkarma
- İrsaliye bağlantısı
- Ödeme takibi

### 3.4. Kasa ve Banka İşlemleri
- Para giriş/çıkış kayıtları
- Hesaplar arası transfer
- Cari hesap ilişkileri

### 3.5. Stok Hareketleri
- Stok giriş/çıkış takibi
- Depo yönetimi
- Kritik stok seviyesi uyarıları

### 3.6. Raporlama
- Dashboard görünümü
- Detaylı raporlar
- Tarih aralığına göre filtreleme

## 4. Geliştirme Süreci

1. Mevcut WebForm projesinin analizi ve model çıkarma
2. ASP.NET Core MVC proje yapısı oluşturma
3. Entity ve DbContext sınıflarının oluşturulması
4. Repository ve Service katmanlarının geliştirilmesi
5. Controller ve View yapılarının oluşturulması
6. Viho temasının entegrasyonu
7. Modal pencere yapısının kurulması
8. CRUD işlemlerinin geliştirilmesi
9. Raporlama modülünün oluşturulması
10. Test ve optimizasyon

## 5. Modal Yapısı ve Sayfa İçi İşlemler

- Her sayfa bir ana listeyi gösterecek
- Ekleme/düzenleme/silme işlemleri için modal pencereler
- AJAX ile sayfa yenilenmeden işlem yapılması
- Partial View'lar ile modal içeriğinin dinamik oluşturulması
- toastr.js ile bildirimler

## 6. Frontend Yapısı

- Viho admin teması kullanılacak
- Responsive tasarım
- jQuery ve Bootstrap entegrasyonu
- DataTables veya benzer kütüphanelerle gelişmiş tablo özellikleri
- Chart.js ile grafiksel raporlar

Bu plan doğrultusunda ASP.NET Core MVC kullanarak, modern ve etkili bir muhasebe-stok uygulaması geliştireceğiz. Mevcut WebForm projesini referans alacak, ancak tamamen yeni bir mimari ile ASP.NET Core MVC'ye uygun şekilde uygulamayı sıfırdan inşa edeceğiz.
