using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MuhasebeStokDB.Pages
{
    public partial class NewDeliveryNote : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        public string ProductOptionsJson { get; private set; }
        public string UnitOptionsJson { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCustomers();
                LoadDeliveryNoteTypes();
                CacheProducts();
                SetProductOptionsJson();
                SetUnitOptionsJson();
                RegisterProductsScript();
                RegisterUnitsScript();
                ViewState["DeliveryNoteDetails"] = new List<DeliveryNoteDetail>();
                BindDeliveryNoteDetails();
                ClientScript.RegisterStartupScript(this.GetType(), "ProductOptions", $"var productOptions = {ProductOptionsJson};", true);
                ClientScript.RegisterStartupScript(this.GetType(), "UnitOptions", $"var unitOptions = {UnitOptionsJson};", true);

                // Mevcut irsaliye düzenleme işlemi için irsaliye bilgilerini yükle
                if (!string.IsNullOrEmpty(Request.QueryString["DeliveryNoteID"]))
                {
                    LoadDeliveryNoteDetails(Request.QueryString["DeliveryNoteID"]);
                }
                else
                {
                    // Yeni irsaliye oluşturuluyorsa irsaliye numarasını ayarla
                    if (Session["IrsaliyeNumarasi"] == null)
                    {
                        lblIrsaliyeNumarasi.Text = string.Empty;
                    }
                    else
                    {
                        lblIrsaliyeNumarasi.Text = Session["IrsaliyeNumarasi"].ToString();
                    }
                }

                // SweetAlert tetiklenmesini kontrol et
                if (Session["ShowSuccessMessage"] != null && (bool)Session["ShowSuccessMessage"])
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "SuccessMessage", "Swal.fire('Başarılı!', 'İrsaliye başarıyla kaydedildi.', 'success');", true);
                    Session["ShowSuccessMessage"] = false;
                }
            }
        }

        private void LoadDeliveryNoteDetails(string deliveryNoteID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Irsaliyeler WHERE IrsaliyeID = @IrsaliyeID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@IrsaliyeID", deliveryNoteID);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lblIrsaliyeNumarasi.Text = reader["IrsaliyeNumarasi"].ToString();
                    ddlCari.SelectedValue = reader["CariID"].ToString();
                    ddlIrsaliyeTuru.SelectedValue = reader["IrsaliyeTuruID"].ToString();
                    txtIrsaliyeTarihi.Text = Convert.ToDateTime(reader["IrsaliyeTarihi"]).ToString("yyyy-MM-dd");
                    txtAciklama.Text = reader["Aciklama"].ToString();
                    chkResmi.Checked = Convert.ToBoolean(reader["Resmi"]);
                }
            }

            // İrsaliye detaylarını yükle
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                        SELECT DND.*, U.UrunAdi 
                        FROM IrsaliyeDetaylari DND
                        INNER JOIN Urunler U ON DND.UrunID = U.UrunID
                        WHERE DND.IrsaliyeID = @IrsaliyeID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@IrsaliyeID", deliveryNoteID);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<DeliveryNoteDetail> details = new List<DeliveryNoteDetail>();
                while (reader.Read())
                {
                    details.Add(new DeliveryNoteDetail
                    {
                        ProductID = Guid.Parse(reader["UrunID"].ToString()),
                        ProductName = reader["UrunAdi"].ToString(),
                        Quantity = Convert.ToInt32(reader["Miktar"]),
                        UnitID = Guid.Parse(reader["BirimID"].ToString()),
                        UnitPrice = Convert.ToDecimal(reader["BirimFiyat"]),
                        LineTotal = Convert.ToDecimal(reader["SatirToplam"])
                    });
                }
                ViewState["DeliveryNoteDetails"] = details;
                BindDeliveryNoteDetails();
            }
        }

        protected void ddlIrsaliyeTuru_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlIrsaliyeTuru.SelectedValue))
            {
                lblIrsaliyeNumarasi.Text = GetDeliveryNoteNumber();
                upIrsaliyeNumarasi.Update(); // UpdatePanel'i güncelle
            }

            // İrsaliye türüne göre gerekli işlemler yapılır
            // Örneğin, ürün ve birim listeleri güncellenebilir

            // Ürün ve birim seçeneklerini güncelle
            SetProductOptionsJson();
            SetUnitOptionsJson();

            // JavaScript kodlarını tekrar kaydet
            RegisterProductsScript();
            RegisterUnitsScript();

            // İrsaliye detaylarını tekrar bağla
            BindDeliveryNoteDetails();
        }

        private string GetDeliveryNoteNumber()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    IF NOT EXISTS (SELECT 1 FROM IrsaliyeNumaralari WHERE Yil = YEAR(GETDATE()) AND IrsaliyeTuruID = @IrsaliyeTuruID)
                    BEGIN
                        INSERT INTO IrsaliyeNumaralari (Yil, SonNumara, Rezervasyon, IrsaliyeTuruID)
                        VALUES (YEAR(GETDATE()), 0, 0, @IrsaliyeTuruID);
                    END
                    ELSE
                    BEGIN
                        UPDATE IrsaliyeNumaralari
                        SET Rezervasyon = Rezervasyon + 1
                        WHERE Yil = YEAR(GETDATE()) AND IrsaliyeTuruID = @IrsaliyeTuruID;
                    END;

                    SELECT CONCAT(
                        CASE @IrsaliyeTuruID
                            WHEN 1 THEN 'RE-SIR-' -- Satış İrsaliyesi
                            WHEN 2 THEN 'RE-AIR-' -- Alış İrsaliyesi
                            WHEN 4 THEN 'RE-AIIR-' -- Alış İade İrsaliyesi
                            WHEN 3 THEN 'RE-SIIR-' -- Satış İade İrsaliyesi
                            ELSE 'RE-IR-'           -- Genel İrsaliye
                        END,
                        YEAR(GETDATE()),
                        FORMAT(Rezervasyon, '000000')
                    ) AS IrsaliyeNumarasi
                    FROM IrsaliyeNumaralari
                    WHERE Yil = YEAR(GETDATE()) AND IrsaliyeTuruID = @IrsaliyeTuruID;";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IrsaliyeTuruID", ddlIrsaliyeTuru.SelectedValue);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString() ?? "HATA";
            }
        }

        private void SetProductOptionsJson()
        {
            var products = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT UrunID, UrunAdi FROM Urunler WHERE Aktif = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new
                        {
                            ProductID = reader["UrunID"].ToString(),
                            ProductName = reader["UrunAdi"].ToString()
                        });
                    }
                }
            }

            ProductOptionsJson = new JavaScriptSerializer().Serialize(products);
        }

        private void SetUnitOptionsJson()
        {
            var units = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT BirimID, BirimAdi FROM Birimler WHERE Aktif = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        units.Add(new
                        {
                            UnitID = reader["BirimID"].ToString(),
                            UnitName = reader["BirimAdi"].ToString()
                        });
                    }
                }
            }

            UnitOptionsJson = new JavaScriptSerializer().Serialize(units);
        }

        private void RegisterProductsScript()
        {
            string script = $"<script>var productOptions = {ProductOptionsJson};</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "ProductOptions", script, false);
        }

        private void RegisterUnitsScript()
        {
            string script = $"<script>var unitOptions = {UnitOptionsJson};</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "UnitOptions", script, false);
        }

        private void LoadCustomers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CariID, CariAdi FROM Cariler WHERE Aktif = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                ddlCari.DataSource = cmd.ExecuteReader();
                ddlCari.DataTextField = "CariAdi";
                ddlCari.DataValueField = "CariID";
                ddlCari.DataBind();
            }
            ddlCari.Items.Insert(0, new ListItem("Seçiniz", ""));
        }

        private void LoadDeliveryNoteTypes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM IrsaliyeTuru";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        ddlIrsaliyeTuru.DataSource = dt;
                        ddlIrsaliyeTuru.DataTextField = "IrsaliyeTuru";
                        ddlIrsaliyeTuru.DataValueField = "IrsaliyeTuruID";
                        ddlIrsaliyeTuru.DataBind();
                    }
                }
            }
            ddlIrsaliyeTuru.Items.Insert(0, new ListItem("Seçiniz", ""));
        }

        private void CacheProducts()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT UrunID, UrunAdi FROM Urunler WHERE Aktif = 1";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                ViewState["UrunlerCache"] = dt;
            }
        }

        private void BindDeliveryNoteDetails()
        {
            // İrsaliye detaylarını bağlama işlemi
            // Bu kısımda repeater veya benzeri bir kontrol kullanılmadığı için bu metodu güncelliyoruz
            // Eğer repeater kullanmak isterseniz, bu kısmı repeater ile güncelleyebilirsiniz
        }

        private void CalculateTotals()
        {
            decimal genelToplam = 0;

            foreach (var detail in DeliveryNoteDetails)
            {
                genelToplam += detail.Quantity * detail.UnitPrice;
            }

            lblGenelToplam.Text = genelToplam.ToString("N2");
        }

        private List<DeliveryNoteDetail> DeliveryNoteDetails
        {
            get
            {
                if (ViewState["DeliveryNoteDetails"] == null)
                    ViewState["DeliveryNoteDetails"] = new List<DeliveryNoteDetail>();
                return (List<DeliveryNoteDetail>)ViewState["DeliveryNoteDetails"];
            }
            set
            {
                ViewState["DeliveryNoteDetails"] = value;
            }
        }

        protected void BtnSaveDeliveryNote_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ddlCari.SelectedValue) || string.IsNullOrEmpty(ddlIrsaliyeTuru.SelectedValue))
                {
                    lblMessage.Text = "Cari ve İrsaliye Türü seçilmeden irsaliye kaydedilemez.";
                    lblMessage.CssClass = "text-danger";
                    return;
                }

                var detailsJson = hfDeliveryNoteDetails.Value;
                var details = new JavaScriptSerializer().Deserialize<List<DeliveryNoteDetail>>(detailsJson);

                if (details == null || details.Count == 0)
                {
                    lblMessage.Text = "İrsaliye içeriği boş! Lütfen en az bir ürün ekleyin.";
                    lblMessage.CssClass = "text-danger";
                    return;
                }

                details.RemoveAll(d => d.Quantity <= 0 || d.UnitPrice <= 0);

                if (details.Count == 0)
                {
                    lblMessage.Text = "Geçerli miktar ve birim fiyat girilmeden irsaliye kaydedilemez.";
                    lblMessage.CssClass = "text-danger";
                    return;
                }

                // İrsaliye toplamlarını hesapla
                decimal genelToplam = 0;

                foreach (var detail in details)
                {
                    genelToplam += detail.Quantity * detail.UnitPrice;
                }

                // Kullanıcı ID'sini kontrol et ve ekle
                var olusturanKullaniciID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";

                // İrsaliye kaydetme
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertDeliveryNoteQuery = @"
                                    INSERT INTO Irsaliyeler (IrsaliyeNumarasi, CariID, IrsaliyeTuruID, IrsaliyeTarihi, GenelToplam, Aciklama, OlusturanKullaniciID, Resmi)
                                    OUTPUT INSERTED.IrsaliyeID
                                    VALUES (@IrsaliyeNumarasi, @CariID, @IrsaliyeTuruID, @IrsaliyeTarihi, @GenelToplam, @Aciklama, @OlusturanKullaniciID, @Resmi);";

                            SqlCommand cmd = new SqlCommand(insertDeliveryNoteQuery, conn, transaction);
                            cmd.Parameters.AddWithValue("@IrsaliyeNumarasi", lblIrsaliyeNumarasi.Text);
                            cmd.Parameters.AddWithValue("@CariID", ddlCari.SelectedValue);
                            cmd.Parameters.AddWithValue("@IrsaliyeTuruID", ddlIrsaliyeTuru.SelectedValue);
                            cmd.Parameters.AddWithValue("@IrsaliyeTarihi", DateTime.Parse(txtIrsaliyeTarihi.Text));
                            cmd.Parameters.AddWithValue("@GenelToplam", genelToplam);
                            cmd.Parameters.AddWithValue("@Aciklama", txtAciklama.Text);
                            cmd.Parameters.AddWithValue("@OlusturanKullaniciID", olusturanKullaniciID);
                            cmd.Parameters.AddWithValue("@Resmi", chkResmi.Checked);

                            Guid deliveryNoteID = (Guid)cmd.ExecuteScalar();

                            string insertDeliveryNoteDetailQuery = @"
                                    INSERT INTO IrsaliyeDetaylari (IrsaliyeID, UrunID, Miktar, BirimID, BirimFiyat, SatirToplam)
                                    VALUES (@IrsaliyeID, @UrunID, @Miktar, @BirimID, @BirimFiyat, @SatirToplam);";

                            foreach (var detail in details)
                            {
                                SqlCommand detailCmd = new SqlCommand(insertDeliveryNoteDetailQuery, conn, transaction);
                                detailCmd.Parameters.AddWithValue("@IrsaliyeID", deliveryNoteID);
                                detailCmd.Parameters.AddWithValue("@UrunID", detail.ProductID);
                                detailCmd.Parameters.AddWithValue("@Miktar", detail.Quantity);
                                detailCmd.Parameters.AddWithValue("@BirimID", detail.UnitID);
                                detailCmd.Parameters.AddWithValue("@BirimFiyat", detail.UnitPrice);
                                detailCmd.Parameters.AddWithValue("@SatirToplam", detail.LineTotal);
                                detailCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            // SweetAlert ile başarı mesajı göster ve sayfayı yenile
                            Session["ShowSuccessMessage"] = true;
                            Session["IrsaliyeNumarasi"] = null; // İrsaliye numarasını sıfırla
                            Response.Redirect(Request.RawUrl);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            lblMessage.Text = $"İrsaliye kaydedilirken bir hata oluştu: {ex.Message}";
                            lblMessage.CssClass = "text-danger";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"İrsaliye kaydedilirken bir hata oluştu: {ex.Message}";
                lblMessage.CssClass = "text-danger";
            }
        }
    }

    [Serializable]
    public class DeliveryNoteDetail
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public Guid UnitID { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}

