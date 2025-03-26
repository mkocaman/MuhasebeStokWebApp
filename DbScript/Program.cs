using System;
using System.Data.SqlClient;

namespace DbScript
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MS SQL Server veritabanı oluşturma scripti başlatılıyor...");

            string serverConnectionString = "Server=localhost,1433;User Id=sa;Password=Test1234_;TrustServerCertificate=True;";
            string databaseName = "MyDb";
            string dbConnectionString = $"Server=localhost,1433;Database={databaseName};User Id=sa;Password=Test1234_;TrustServerCertificate=True;";

            try
            {
                // Önce veritabanını oluştur
                CreateDatabase(serverConnectionString, databaseName);
                
                Console.WriteLine($"Veritabanı '{databaseName}' başarıyla oluşturuldu.");
                Console.WriteLine("İşlem tamamlandı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
            }
        }

        static void CreateDatabase(string connectionString, string databaseName)
        {
            // Veritabanı oluşturma
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                string checkDbQuery = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
                using (SqlCommand checkCommand = new SqlCommand(checkDbQuery, connection))
                {
                    int dbCount = (int)checkCommand.ExecuteScalar();
                    
                    if (dbCount > 0)
                    {
                        Console.WriteLine($"Veritabanı '{databaseName}' zaten mevcut, siliniyor...");
                        string dropDbQuery = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}]";
                        using (SqlCommand dropCommand = new SqlCommand(dropDbQuery, connection))
                        {
                            dropCommand.ExecuteNonQuery();
                        }
                    }
                }
                
                Console.WriteLine($"Veritabanı '{databaseName}' oluşturuluyor...");
                string createDbQuery = $"CREATE DATABASE [{databaseName}]";
                using (SqlCommand createCommand = new SqlCommand(createDbQuery, connection))
                {
                    createCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
