// Products.aspx.cs
using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebGrease.Activities;



namespace MaOaKApp.Pages
{
    public partial class Products : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState["Filter"] = ""; // Filtre başlangıçta boş
                LoadProducts();
            }
        }



        private void LoadProducts()
        {
            string filter = ViewState["Filter"]?.ToString() ?? "";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT 
                Urunler.UrunID, 
                Urunler.UrunAdi,
                Urunler.StokMiktar,
                (SELECT TOP 1 Fiyat FROM UrunFiyatlari 
                 WHERE UrunFiyatlari.UrunID = Urunler.UrunID AND FiyatTipiID = 1 ORDER BY GecerliTarih DESC) AS ListeFiyati,
                (SELECT TOP 1 Fiyat FROM UrunFiyatlari 
                 WHERE UrunFiyatlari.UrunID = Urunler.UrunID AND FiyatTipiID = 2 ORDER BY GecerliTarih DESC) AS MaliyetFiyati,
                (SELECT TOP 1 Fiyat FROM UrunFiyatlari 
                 WHERE UrunFiyatlari.UrunID = Urunler.UrunID AND FiyatTipiID = 3 ORDER BY GecerliTarih DESC) AS SatisFiyati
            FROM Urunler
            WHERE Urunler.SoftDelete = 0
            AND (@Filter = '' OR Urunler.UrunAdi LIKE '%' + @Filter + '%')";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Filter", filter);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvProducts.DataSource = dt;
                    gvProducts.DataBind();

                    // Eğer sonuç yoksa mesaj göster
                    lblNoRecords.Visible = dt.Rows.Count == 0;
                    if (dt.Rows.Count == 0)
                    {
                        lblNoRecords.Text = "Aradığınız ürün bulunamadı.";
                    }
                }
            }
        }


        protected void btnSaveProduct_Click(object sender, EventArgs e)
        {
            string productName = txtProductName.Text.Trim();
            decimal listeFiyati = Convert.ToDecimal(txtListeFiyati.Text);
            decimal maliyetFiyati = Convert.ToDecimal(txtMaliyetFiyati.Text);
            decimal satisFiyati = Convert.ToDecimal(txtSatisFiyati.Text);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    string insertProductQuery = @"
                        DECLARE @NewProductID uniqueidentifier;
                        SET @NewProductID = NEWID();
                        INSERT INTO Urunler (UrunID, UrunAdi, SoftDelete) 
                        VALUES (@NewProductID, @UrunAdi, 0);
                        SELECT @NewProductID;";

                    SqlCommand productCmd = new SqlCommand(insertProductQuery, con, transaction);
                    productCmd.Parameters.AddWithValue("@UrunAdi", productName);
                    Guid productId = (Guid)productCmd.ExecuteScalar();

                    // Insert prices
                    InsertPrice(con, transaction, productId, listeFiyati, 1); // Liste Fiyatı
                    InsertPrice(con, transaction, productId, maliyetFiyati, 2); // Maliyet Fiyatı
                    InsertPrice(con, transaction, productId, satisFiyati, 3); // Satış Fiyatı

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
                finally
                {
                    con.Close();
                }
            }
            Response.Redirect(Request.RawUrl);
            LoadProducts();
        }

        private void InsertPrice(SqlConnection con, SqlTransaction transaction, Guid productId, decimal price, int priceTypeId)
        {
            string query = @"
        INSERT INTO UrunFiyatlari ( UrunID, Fiyat, FiyatTipiID, GecerliTarih) 
        VALUES ( @UrunID, @Fiyat, @FiyatTipiID, GETDATE())";

            using (SqlCommand cmd = new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.AddWithValue("@UrunID", productId);
                cmd.Parameters.AddWithValue("@Fiyat", price);
                cmd.Parameters.AddWithValue("@FiyatTipiID", priceTypeId);
                cmd.ExecuteNonQuery();
            }
        }


        private void LoadProductForEdit(string productId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT UrunAdi,
                   (SELECT TOP 1 Fiyat FROM UrunFiyatlari WHERE UrunID = @UrunID AND FiyatTipiID = 1) AS ListeFiyati,
                   (SELECT TOP 1 Fiyat FROM UrunFiyatlari WHERE UrunID = @UrunID AND FiyatTipiID = 2) AS MaliyetFiyati,
                   (SELECT TOP 1 Fiyat FROM UrunFiyatlari WHERE UrunID = @UrunID AND FiyatTipiID = 3) AS SatisFiyati
            FROM Urunler WHERE UrunID = @UrunID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UrunID", Guid.Parse(productId));
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtEditProductName.Text = reader["UrunAdi"].ToString();
                        txtEditListeFiyati.Text = reader["ListeFiyati"].ToString();
                        txtEditMaliyetFiyati.Text = reader["MaliyetFiyati"].ToString();
                        txtEditSatisFiyati.Text = reader["SatisFiyati"].ToString();
                    }
                }
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditModal", "$('#editProductModal').modal('show');", true);
        }



        protected void btnUpdateProduct_Click(object sender, EventArgs e)
        {
            string productId = ViewState["SelectedProductId"]?.ToString();

            // Ürün ID'nin doğruluğunu kontrol et
            if (string.IsNullOrEmpty(productId) || !Guid.TryParse(productId, out Guid validGuid))
            {
                lblError.Text = "Geçerli bir ürün seçilmedi.";
                return;
            }

            try
            {
                string productName = txtEditProductName.Text.Trim();
                decimal listeFiyati = Convert.ToDecimal(txtEditListeFiyati.Text);
                decimal maliyetFiyati = Convert.ToDecimal(txtEditMaliyetFiyati.Text);
                decimal satisFiyati = Convert.ToDecimal(txtEditSatisFiyati.Text);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Ürün güncelleme sorgusu
                            string updateProductQuery = @"
                        UPDATE Urunler 
                        SET UrunAdi = @UrunAdi 
                        WHERE UrunID = @UrunID";

                            using (SqlCommand productCmd = new SqlCommand(updateProductQuery, con, transaction))
                            {
                                productCmd.Parameters.AddWithValue("@UrunAdi", productName);
                                productCmd.Parameters.AddWithValue("@UrunID", validGuid);
                                productCmd.ExecuteNonQuery();
                            }

                            // Fiyatları güncelleme (yeni fiyatlar ekleniyor)
                            InsertPrice(con, transaction, validGuid, listeFiyati, 1); // Liste Fiyatı
                            InsertPrice(con, transaction, validGuid, maliyetFiyati, 2); // Maliyet Fiyatı
                            InsertPrice(con, transaction, validGuid, satisFiyati, 3); // Satış Fiyatı

                            // Transaction'ı tamamla
                            transaction.Commit();

                            // Modal kapatma ve sayfayı yenileme
                            ScriptManager.RegisterStartupScript(this, GetType(), "HideEditModal", "$('#editProductModal').modal('hide');", true);
                        }
                        catch (Exception ex)
                        {
                            // Transaction geri alma
                            transaction.Rollback();
                            lblError.Text = "Hata: " + ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Beklenmeyen bir hata oluştu: " + ex.Message;
            }

            // Ürünleri yeniden yükle
            LoadProducts();
            Response.Redirect(Request.RawUrl);
        }


        protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string selectedProductId = e.CommandArgument.ToString();
            ViewState["SelectedProductId"] = selectedProductId;

            if (e.CommandName == "EditProduct")
            {
                LoadProductForEdit(selectedProductId);
            }
            else if (e.CommandName == "DeleteProduct")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowDeleteModal", "$('#deleteConfirmModal').modal('show');", true);
            }
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            string selectedProductId = ViewState["SelectedProductId"]?.ToString();
            if (Guid.TryParse(selectedProductId, out Guid validGuid))
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Urunler SET SoftDelete = 1 WHERE UrunID = @UrunID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UrunID", validGuid);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "HideDeleteModal", "$('#deleteConfirmModal').modal('hide');", true);
                Response.Redirect(Request.RawUrl);
                LoadProducts();
            }
        }
        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ViewState["Filter"] = txtSearch.Text.Trim(); // Update filter value
            gvProducts.PageIndex = 0; // Reset to first page
            LoadProducts();
        }

        protected void gvProducts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvProducts.PageIndex = e.NewPageIndex;
            LoadProducts();
        }

    }
}
