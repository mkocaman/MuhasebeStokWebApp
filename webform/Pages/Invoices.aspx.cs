using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Web.UI.WebControls;

namespace MuhasebeStokDB.Pages
{
    public partial class Invoices : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadInvoices();
            }
        }

        private void LoadInvoices()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Sadece "Çıkış" (satış) faturalarını getiriyoruz.
                string query = @"
                SELECT 
                    F.FaturaID, 
                    F.FaturaNumarasi, 
                    C.CariAdi, 
                    F.FaturaTarihi, 
                    F.GenelToplam,
                    F.DovizTuru
                FROM Faturalar F
                INNER JOIN Cariler C ON F.CariID = C.CariID
                INNER JOIN FaturaTuru FT ON F.FaturaTuruID = FT.FaturaTuruID
                WHERE F.SoftDelete = 0 AND FT.HareketTuru = N'Çıkış'
                ORDER BY F.FaturaNumarasi DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptInvoices.DataSource = dt;
                rptInvoices.DataBind();
            }
        }

        protected void rptInvoices_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string invoiceID = e.CommandArgument.ToString();

            if (e.CommandName == "EditInvoice")
            {
                Response.Redirect($"NewInvoice.aspx?InvoiceID={invoiceID}");
            }
            else if (e.CommandName == "DeleteInvoice")
            {
                DeleteInvoice(invoiceID);
                LoadInvoices(); // Repeater'ı yenile
            }
            else if (e.CommandName == "ViewInvoice")
            {
                Response.Redirect($"InvoiceDetails.aspx?InvoiceID={invoiceID}");
            }
        }

        private void DeleteInvoice(string invoiceID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE Faturalar SET SoftDelete = 1 WHERE FaturaID = @FaturaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FaturaID", invoiceID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// GenelToplam değerini tr-TR formatında (ör. 123.345,56) biçimlendirir
        /// ve veritabanından alınan döviz sembolünü tutarın sonuna ekler.
        /// </summary>
        protected string FormatTotal(object totalObj, object dovizTuruObj)
        {
            if (totalObj != null && Decimal.TryParse(totalObj.ToString(), out decimal total))
            {
                // Türk Lirası formatı için tr-TR kültürünü kullanıyoruz.
                CultureInfo tr = new CultureInfo("tr-TR");
                string formatted = total.ToString("N2", tr); // Örneğin: 123.345,56

                // Faturada saklanan döviz kodunu alıyoruz (varsayılan "TL").
                string currencyCode = dovizTuruObj?.ToString() ?? "TL";
                // Döviz sembolünü DB'den GetCurrencySymbol metodu ile alalım.
                string currencySymbol = GetCurrencySymbol(currencyCode);

                // Eğer DB'den sembol alınamazsa, yedek değerler kullanıyoruz.
                if (string.IsNullOrEmpty(currencySymbol))
                {
                    if (currencyCode.ToUpper() == "TL")
                        currencySymbol = "₺";
                    else if (currencyCode.ToUpper() == "USD")
                        currencySymbol = "$";
                    else if (currencyCode.ToUpper() == "EUR")
                        currencySymbol = "€";
                    else
                        currencySymbol = currencyCode;
                }

                // Tutarın sonuna döviz sembolünü ekleyelim.
                return formatted + " " + currencySymbol;
            }
            return "";
        }

        /// <summary>
        /// Verilen döviz kodu (örneğin, "TL", "USD", "EUR") için, Dovizler tablosundan döviz sembolünü getirir.
        /// Dovizler tablosunda döviz kodunun tutulduğu sütun "DovizKodu" olarak varsayılmıştır.
        /// </summary>
        private string GetCurrencySymbol(string currencyCode)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Dovizler tablosunda döviz kodu "DovizKodu" sütunu olarak tutuluyorsa; 
                // DövizSembol sütununu almak için sorgu:
                string query = "SELECT TOP 1 DovizSembol FROM Dovizler WHERE DovizKodu = @CurrencyCode";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CurrencyCode", currencyCode);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? result.ToString() : "";
            }
        }
    }
}
