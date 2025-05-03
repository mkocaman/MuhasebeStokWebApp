#!/bin/bash

# Bu script, Entity Framework Core migration dosyalarını birleştirmek için kullanılır
# MuhasebeStokWebApp projesi için özelleştirilmiştir

# Renkli konsolda çıktı fonksiyonları
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}Entity Framework Core Migration Birleştirme Scripti${NC}"
echo -e "${YELLOW}MuhasebeStokWebApp projesi için özelleştirilmiştir${NC}"
echo ""

# Proje dizinini kontrol et
if [ ! -d "Migrations" ]; then
    echo -e "${RED}Hata: Bu script projenin kök dizininde çalıştırılmalıdır!${NC}"
    exit 1
fi

# Mevcut migration'ları yedekle
echo -e "${YELLOW}Mevcut migration dosyaları yedekleniyor...${NC}"
mkdir -p MigrationsBackup
cp -r Migrations/* MigrationsBackup/
echo -e "${GREEN}Migrations klasörü MigrationsBackup klasörüne yedeklendi${NC}"

# Veritabanı işlemleri için uyarı
echo -e "${RED}UYARI: Bu işlem devam etmeden önce, veritabanınızı ve __EFMigrationsHistory tablosunu yedeklemeniz önerilir!${NC}"
echo -e "${YELLOW}Devam etmek istiyor musunuz? (E/H)${NC}"
read -p "> " response
if [[ "$response" != "E" && "$response" != "e" ]]; then
    echo -e "${BLUE}İşlem iptal edildi.${NC}"
    exit 0
fi

# Mevcut migration'ları temizle, ancak snapshot dosyasını koru
echo -e "${YELLOW}Migration dosyaları temizleniyor...${NC}"
mkdir -p TempSnapshot
cp Migrations/ApplicationDbContextModelSnapshot.cs TempSnapshot/
rm -rf Migrations/*.cs
cp TempSnapshot/ApplicationDbContextModelSnapshot.cs Migrations/
echo -e "${GREEN}Migration dosyaları temizlendi, snapshot dosyası korundu${NC}"

# Yeni migration'ları oluştur
echo -e "${YELLOW}Yeni konsolide migration'lar oluşturuluyor...${NC}"

# Her bir ana kategoride yeni bir migration oluştur
echo -e "${BLUE}1. Temel Veritabanı Yapısı Migration'ı oluşturuluyor${NC}"
dotnet ef migrations add ConsolidatedInitialMigration --context ApplicationDbContext

echo -e "${BLUE}2. Cari Modülü İyileştirmeleri Migration'ı oluşturuluyor${NC}"
dotnet ef migrations add ConsolidatedCariModuleImprovement --context ApplicationDbContext

echo -e "${BLUE}3. Fatura ve İrsaliye İyileştirmeleri Migration'ı oluşturuluyor${NC}"
dotnet ef migrations add ConsolidatedFaturaIrsaliyeImprovements --context ApplicationDbContext

echo -e "${BLUE}4. Stok Modülü İyileştirmeleri Migration'ı oluşturuluyor${NC}"
dotnet ef migrations add ConsolidatedStokModuleImprovements --context ApplicationDbContext

echo -e "${BLUE}5. Para Birimi ve Kur İyileştirmeleri Migration'ı oluşturuluyor${NC}"
dotnet ef migrations add ConsolidatedParaBirimiImprovements --context ApplicationDbContext

echo -e "${BLUE}6. Format ve Veri Tipi Düzeltmeleri Migration'ı oluşturuluyor${NC}"
dotnet ef migrations add ConsolidatedDataFormatFixes --context ApplicationDbContext

echo -e "${BLUE}7. Diğer İyileştirmeler Migration'ı oluşturuluyor${NC}"
dotnet ef migrations add ConsolidatedOtherImprovements --context ApplicationDbContext

echo -e "${GREEN}Yeni migration'lar oluşturuldu${NC}"

# SQL script oluştur
echo -e "${YELLOW}Yeni migration'lar için SQL script oluşturuluyor...${NC}"
dotnet ef migrations script --output consolidated_migrations.sql --context ApplicationDbContext
echo -e "${GREEN}SQL script oluşturuldu: consolidated_migrations.sql${NC}"

# Tamamlandı
echo -e "${BLUE}Migration birleştirme işlemi tamamlandı.${NC}"
echo -e "${YELLOW}UYARI: Bu işlem sonrasında veritabanını güncelleme işlemi SQL script veya 'dotnet ef database update' komutu ile yapılmalıdır.${NC}"
echo -e "${GREEN}MigrationsBackup klasöründe mevcut migration'ların yedekleri bulunmaktadır.${NC}"
echo -e "${GREEN}Dileğiniz zaman bunları geri yükleyebilirsiniz.${NC}" 