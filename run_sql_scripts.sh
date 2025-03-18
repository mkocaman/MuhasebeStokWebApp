#!/bin/bash

# SQL Server bağlantı bilgileri
SERVER="localhost"
DATABASE="MuhasebeStokDB"
USER="sa"
PASSWORD="Password1"

# SQL scriptleri çalıştır
echo "SistemAyarlari tablosuna varsayılan kayıt ekleniyor..."
/opt/mssql-tools/bin/sqlcmd -S $SERVER -d $DATABASE -U $USER -P $PASSWORD -i insert_sistem_ayarlari.sql

echo "DovizKurlari tablosuna varsayılan kayıtlar ekleniyor..."
/opt/mssql-tools/bin/sqlcmd -S $SERVER -d $DATABASE -U $USER -P $PASSWORD -i insert_doviz_kurlari.sql

echo "SQL scriptleri başarıyla çalıştırıldı." 