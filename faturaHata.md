# Fatura/Create Sayfası Hata Analizi

Bu belge, `Fatura/Create` sayfasında karşılaşılan sorunları ve olası nedenlerini analiz etmektedir.

## Tespit Edilen Sorunlar

1.  **İlk Kalem Ürün Adı Görünmüyor:** Sayfa ilk yüklendiğinde otomatik olarak eklenen fatura kaleminin "Ürün" sütununda ürün adı görüntülenmiyor. Ancak, "Kalem Ekle" butonu ile manuel olarak eklenen yeni kalemlerde ürün adları doğru bir şekilde görünüyor.
2.  **Satır Hesaplaması Çalışmıyor:** Fatura kalemine miktar ve birim fiyat girildiğinde, ilgili satırın "Tutar" alanı otomatik olarak hesaplanmıyor.

## Analiz ve Nedenler

### 1. İlk Kalem Ürün Adı Sorunu

*   **Neden:** Sorun, sayfa yüklendiğinde ilk kalemi ekleyen JavaScript (`addKalemRow` fonksiyonu) kodundaki zamanlama veya olay tetikleme mekanizmasından kaynaklanıyordu.
    *   İlk kalem eklendikten sonra, ilk ürün otomatik olarak seçiliyor ve ürün bilgilerini (`getUrunBilgileri` fonksiyonu ile) getirmek için bir `change` olayı tetikleniyordu (`.trigger('change')`).
    *   Ancak, bu `change` olayının tetiklendiği anda, ilgili olay dinleyicisinin (`event listener`) tam olarak hazır olmaması veya Select2 kütüphanesinin bu ilk tetiklemeyle etkileşime girmesi nedeniyle `getUrunBilgileri` fonksiyonu çağrılmıyordu.
    *   Bu durum, ilk satır için ürün adı (`urun-adi` gizli alanı) ve diğer ilgili alanların (birim, birim fiyat, KDV oranı) boş kalmasına yol açıyordu.
*   **Çözüm:** `addKalemRow` fonksiyonu güncellendi. Artık ilk ürün seçildikten sonra `getUrunBilgileri` fonksiyonu doğrudan çağrılıyor. Bu, `change` olayının tetiklenmesine güvenmek yerine ürün bilgilerinin kesin olarak alınmasını sağlıyor.

### 2. Satır Hesaplaması Sorunu

*   **Neden:** Satır toplamlarını (`Tutar`) hesaplayan `calculateRowTotals` fonksiyonu, miktar, birim fiyat, KDV veya indirim oranı alanlarındaki `input` olayları tarafından tetikleniyordu. Hesaplama mantığı (`miktar * birimFiyat`) temelde doğru olsa da, birkaç sorun vardı:
    *   **Olay Tetikleme/Fonksiyon Hatası:** `input` olaylarının `calculateRowTotals` fonksiyonunu her zaman doğru şekilde tetiklememesi veya fonksiyon içinde bir JavaScript hatası olması, `.tutar` alanının güncellenmesini engelliyor olabilirdi.
    *   **Toplam Hesaplama Mantığı (`calculateFaturaTotals`):** Daha önemli bir sorun, fatura genel toplamlarını hesaplayan `calculateFaturaTotals` fonksiyonunun, satırlardaki formatlanmış (`FormatDecimal` ile virgüllü hale getirilmiş) değerleri `parseFloat` ile okumasıydı. `parseFloat`, Türkçe yerelleştirmesindeki virgüllü ondalık ayracını yanlış yorumlayarak hatalı genel toplamlara neden oluyordu.
*   **Çözüm:**
    *   `calculateRowTotals` fonksiyonu, hesaplanan ham sayısal değerleri (örneğin, `tutar.toFixed(2)`) ilgili input alanlarına yazacak şekilde düzenlendi.
    *   `calculateFaturaTotals` fonksiyonu tamamen yeniden yazılarak, genel toplamları hesaplamak için formatlanmış ara değerleri okumak yerine, her satırdaki ham miktar, birim fiyat, KDV oranı ve indirim oranı input'larından değerleri doğrudan okuyup satır hesaplamalarını yeniden yapacak şekilde güncellendi. Bu, `parseFloat` sorununu ortadan kaldırır ve daha doğru bir genel toplam hesaplaması sağlar.

## Sonuç

Yapılan JavaScript güncellemeleri ile hem ilk fatura kaleminin ürün adının görünmemesi sorunu hem de satır tutarlarının ve genel toplamların doğru hesaplanmaması sorunu giderilmiştir.
