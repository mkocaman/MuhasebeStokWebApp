using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace MaOaKApp.Pages
{
    public partial class CustomerStatement : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // QueryString üzerinden CariID alınıyor.
                string cariID = Request.QueryString["CariID"];
                if (!string.IsNullOrEmpty(cariID))
                {
                    LoadCustomerDetails(cariID);
                    LoadCustomerStatement(cariID);
                }
                else
                {
                    // CariID eksikse, kullanıcıyı yönlendirebilir veya hata mesajı gösterebilirsiniz.
                    Response.Write("Müşteri bilgileri alınamadı.");
                }
            }
        }

        /// <summary>
        /// CariID parametresi ile ilgili müşterinin adını Cariler tablosundan alır
        /// ve lblCustomerName kontrolüne yazar.
        /// </summary>
        private void LoadCustomerDetails(string cariID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CariAdi FROM Cariler WHERE CariID = @CariID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CariID", cariID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                if (result != null)
                {
                    lblCustomerName.Text = "Cari Hesap Ekstresi - " + result.ToString();
                }
            }
        }

        /// <summary>
        /// CariHareketler tablosundan, ilgili CariID’ye ait işlemleri alır ve repeater’a bağlar.
        /// </summary>
        private void LoadCustomerStatement(string cariID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        IslemTarihi, 
                        Tutar, 
                        IslemTuru, 
                        Aciklama
                    FROM CariHareketler 
                    WHERE CariID = @CariID AND SoftDelete = 0
                    ORDER BY IslemTarihi DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CariID", cariID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptStatement.DataSource = dt;
                rptStatement.DataBind();
            }
        }
    }
}