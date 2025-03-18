using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class ProductsRestore : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDeletedProducts();
            }
        }

        private void LoadDeletedProducts()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT 
                    UrunID, 
                    UrunAdi, 
                    StokMiktar 
                FROM Urunler
                WHERE SoftDelete = 1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvDeletedProducts.DataSource = dt;
                    gvDeletedProducts.DataBind();

                    lblNoRecords.Visible = dt.Rows.Count == 0;
                    if (dt.Rows.Count == 0)
                    {
                        lblNoRecords.Text = "Silinmiş ürün bulunamadı.";
                    }
                }
            }
        }

        protected void gvDeletedProducts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "RestoreProduct")
            {
                string productId = e.CommandArgument.ToString();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Urunler SET SoftDelete = 0 WHERE UrunID = @UrunID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UrunID", Guid.Parse(productId));
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Ürünler listesi yeniden yükleniyor
                LoadDeletedProducts();
            }
        }
    }
}
