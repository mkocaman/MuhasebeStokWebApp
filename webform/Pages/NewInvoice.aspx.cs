using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using MaOaKApp.Services;
using System.Web;
using System.Web.Services;
using System.Text.RegularExpressions;

namespace MaOaKApp.Pages
{
    public partial class NewInvoice : System.Web.UI.Page
    {
        private readonly InvoiceService _invoiceService = new InvoiceService();
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected Label lblAraToplam;
        protected Label lblKdvToplam;
        protected Label lblGenelToplam;
        protected DropDownList ddlUrun;
        protected DropDownList ddlBirim;
        protected Repeater rptInvoiceDetails;

        public string ProductOptionsJson { get; private set; }
        public string UnitOptionsJson { get; private set; }

        [WebMethod]
        public static string AddCustomer(string cariAdi, string vergiNo, string telefon, string email, string adres, string aciklama)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Aynı kullanıcı olup olmadığını kontrol et
                string checkQuery = "SELECT COUNT(*) FROM Cariler WHERE CariAdi = @CariAdi AND Email = @Email";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@CariAdi", cariAdi);
                checkCmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                int userCount = (int)checkCmd.ExecuteScalar();
                conn.Close();

                if (userCount > 0)
                {
                    return "exists";
                }

                string query = @"
            INSERT INTO Cariler (CariID, CariAdi, VergiNo, Telefon, Email, Adres, Aciklama, Aktif, SoftDelete, OlusturanKullaniciID, OlusturmaTarihi)
            VALUES (NEWID(), @CariAdi, @VergiNo, @Telefon, @Email, @Adres, @Aciklama, 1, 0, @OlusturanKullaniciID, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CariAdi", cariAdi);
                cmd.Parameters.AddWithValue("@VergiNo", (object)vergiNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefon", (object)telefon ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Adres", (object)adres ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Aciklama", (object)aciklama ?? DBNull.Value);

                // Session'da kullanıcı yoksa belirtilen kullanıcı ID'sini kullan
                var olusturanKullaniciID = HttpContext.Current.Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";
                cmd.Parameters.AddWithValue("@OlusturanKullaniciID", olusturanKullaniciID);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0 ? "success" : "error";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCustomers();
                LoadInvoiceTypes();
                LoadPaymentTypes();
                LoadCurrencyTypes();
                CacheProducts();
                SetProductOptionsJson();
                SetUnitOptionsJson();
                RegisterProductsScript();
                RegisterUnitsScript();
                ViewState["InvoiceDetails"] = new List<InvoiceDetail>();
                BindInvoiceDetails();
                ClientScript.RegisterStartupScript(this.GetType(), "ProductOptions", $"var productOptions = {ProductOptionsJson};", true);
                ClientScript.RegisterStartupScript(this.GetType(), "UnitOptions", $"var unitOptions = {UnitOptionsJson};", true);
                pnlBtnUpdate.Visible = false;
                pnlBtnSave.Visible = true;
                lblBaslik.Text = "Yeni Fatura Oluştur";

                // Mevcut fatura düzenleme işlemi için fatura bilgilerini yükle
                if (!string.IsNullOrEmpty(Request.QueryString["InvoiceID"]))
                {
                    LoadInvoiceDetails(Request.QueryString["InvoiceID"]);
                    pnlBtnSave.Visible = false;
                    pnlBtnUpdate.Visible = true;
                    lblBaslik.Text = "Fatura Düzenle";
                }
            }

            if (IsPostBack && Request["__EVENTTARGET"] == "UpdateCustomers")
            {
                LoadCustomers();
            }
        }

        private void LoadInvoiceDetails(string invoiceID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Faturalar WHERE FaturaID = @FaturaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FaturaID", invoiceID);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lblFaturaNumarasi.Text = reader["FaturaNumarasi"].ToString();
                    ddlCari.SelectedValue = reader["CariID"].ToString();
                    ddlFaturaTuru.SelectedValue = reader["FaturaTuruID"].ToString();
                    txtFaturaTarihi.Text = Convert.ToDateTime(reader["FaturaTarihi"]).ToString("yyyy-MM-dd");
                    txtVadeTarihi.Text = Convert.ToDateTime(reader["VadeTarihi"]).ToString("yyyy-MM-dd");
                    ddlOdemeTuru.SelectedValue = reader["OdemeTuruID"].ToString();
                    ddlDovizTuru.SelectedValue = reader["DovizTuru"].ToString();
                    txtDovizKuru.Text = reader["DovizKuru"].ToString();
                    txtFaturaNotu.Text = reader["FaturaNotu"].ToString();
                    chkResmi.Checked = Convert.ToBoolean(reader["Resmi"]);
                }
            }

            // Fatura detaylarını yükle
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT FD.*, U.UrunAdi 
            FROM FaturaDetaylari FD
            INNER JOIN Urunler U ON FD.UrunID = U.UrunID
            WHERE FD.FaturaID = @FaturaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FaturaID", invoiceID);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<InvoiceDetail> details = new List<InvoiceDetail>();
                while (reader.Read())
                {
                    details.Add(new InvoiceDetail
                    {
                        ProductID = Guid.Parse(reader["UrunID"].ToString()),
                        ProductName = reader["UrunAdi"].ToString(),
                        Quantity = Convert.ToInt32(reader["Miktar"]),
                        UnitID = Guid.Parse(reader["BirimID"].ToString()),
                        UnitPrice = Convert.ToDecimal(reader["BirimFiyat"]),
                        LineVatTotal = Convert.ToDecimal(reader["SatirKdvToplam"]),
                        LineTotal = Convert.ToDecimal(reader["SatirToplam"])
                    });
                }
                hfInvoiceDetails.Value = new JavaScriptSerializer().Serialize(details); // Hidden field'a JSON formatında detayları kaydet
            }
        }



        protected void ddlDovizTuru_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Döviz kuru güncelleme işlemi
            UpdateCurrencyRate();
        }

        private void UpdateCurrencyRate()
        {
            string selectedCurrency = ddlDovizTuru.SelectedValue;
            if (!string.IsNullOrEmpty(selectedCurrency))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT TOP 1 SatisKur 
                FROM DovizKurlari DK
                INNER JOIN Dovizler D ON DK.DovizID = D.DovizID
                WHERE D.DovizKodu = @DovizKodu AND CONVERT(VARCHAR(10), DK.KurTarihi, 120) = CONVERT(VARCHAR(10), GETDATE(), 120)
                ORDER BY DK.KurTarihi DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@DovizKodu", selectedCurrency);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        txtDovizKuru.Text = result.ToString();
                    }
                }
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

        protected void btnSaveCustomer_Click(object sender, EventArgs e)
        {
            string cariAdi = txtCustomerName.Text;
            string vergiNo = txtVergiNo.Text;
            string telefon = txtTelefon.Text;
            string email = txtEmail.Text;
            string adres = txtAdres.Text;
            string aciklama = txtAciklama.Text;

            // Validasyonlar
            if (string.IsNullOrEmpty(cariAdi))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Müşteri Adı zorunludur.', 'error');", true);
                return;
            }

            if (!string.IsNullOrEmpty(vergiNo) && !Regex.IsMatch(vergiNo, @"^\d+$"))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Vergi No sadece rakam olmalıdır.', 'error');", true);
                return;
            }

            if (!string.IsNullOrEmpty(telefon) && !Regex.IsMatch(telefon, @"^\+?\d{10,15}$"))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Geçerli bir telefon numarası giriniz.', 'error');", true);
                return;
            }

            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Geçerli bir e-posta adresi giriniz.', 'error');", true);
                return;
            }

            string result = AddCustomer(cariAdi, vergiNo, telefon, email, adres, aciklama);
            if (result == "success")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "onCustomerAdded('success');", true);
            }
            else if (result == "exists")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Bu müşteri zaten mevcut.', 'error');", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "onCustomerAdded('error');", true);
            }
        }

        private void LoadCustomers()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString))
            {
                string query = "SELECT CariID, CariAdi FROM Cariler WHERE Aktif = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                ddlCari.Items.Clear();
                ddlCari.Items.Add(new ListItem("Müşteri Seçiniz", ""));
                ddlCari.DataSource = cmd.ExecuteReader();
                ddlCari.DataTextField = "CariAdi";
                ddlCari.DataValueField = "CariID";
                ddlCari.DataBind();
            }
        }

        private void LoadInvoiceTypes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM FaturaTuru";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        ddlFaturaTuru.DataSource = dt;
                        ddlFaturaTuru.DataTextField = "FaturaTuru";
                        ddlFaturaTuru.DataValueField = "FaturaTuruID";
                        ddlFaturaTuru.DataBind();
                    }
                }
            }
            ddlFaturaTuru.Items.Insert(0, new ListItem("Fatura Tipini Seçin", ""));
        }


        private void LoadPaymentTypes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT OdemeTuruID, OdemeTuru FROM OdemeTurleri";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                ddlOdemeTuru.Items.Clear(); // Mevcut öğeleri temizle

                ddlOdemeTuru.DataSource = cmd.ExecuteReader();
                ddlOdemeTuru.DataTextField = "OdemeTuru";
                ddlOdemeTuru.DataValueField = "OdemeTuruID";
                ddlOdemeTuru.DataBind();
                ddlOdemeTuru.Items.Insert(0, new ListItem("Ödeme Türü Seçiniz", ""));
            }
        }

        private void LoadCurrencyTypes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT DovizID, DovizAdi, DovizKodu, DovizSembol FROM Dovizler WHERE DovizSembol IS NOT NULL ORDER BY DovizSembol desc";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                ddlDovizTuru.Items.Clear(); // Mevcut öğeleri temizle
                ddlDovizTuru.DataSource = cmd.ExecuteReader();
                ddlDovizTuru.DataTextField = "DovizSembol";
                ddlDovizTuru.DataValueField = "DovizKodu";
                ddlDovizTuru.DataBind();
                ddlDovizTuru.Items.Insert(0, new ListItem("Döviz Türü Seçiniz", ""));

            }
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

        private void BindInvoiceDetails()
        {
            if (rptInvoiceDetails != null)
            {
                rptInvoiceDetails.DataSource = InvoiceDetails;
                rptInvoiceDetails.DataBind();
                CalculateTotals();
            }
            else
            {
                // Null durumu ele alın, muhtemelen bir hata kaydedin veya kontrolü başlatın
            }
        }

        private void CalculateTotals()
        {
            decimal araToplam = 0;
            decimal kdvToplam = 0;
            decimal genelToplam = 0;

            foreach (var detail in InvoiceDetails)
            {
                araToplam += detail.LineTotal;
            }

            kdvToplam = araToplam * 0.12m; // %12 KDV
            genelToplam = araToplam + kdvToplam;

            lblAraToplam.Text = araToplam.ToString("C");
            lblKdvToplam.Text = kdvToplam.ToString("C");
            lblGenelToplam.Text = genelToplam.ToString("C");
        }

        private List<InvoiceDetail> InvoiceDetails
        {
            get
            {
                if (ViewState["InvoiceDetails"] == null)
                    ViewState["InvoiceDetails"] = new List<InvoiceDetail>();
                return (List<InvoiceDetail>)ViewState["InvoiceDetails"];
            }
            set
            {
                ViewState["InvoiceDetails"] = value;
            }
        }

        private void IncrementReservation()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    UPDATE FaturaNumaralari
                    SET Rezervasyon = Rezervasyon + 1
                    WHERE Yil = YEAR(GETDATE()) AND FaturaTuruID = @FaturaTuruID;
                ";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FaturaTuruID", ddlFaturaTuru.SelectedValue);
                cmd.ExecuteNonQuery();
            }
        }

        private string GetInvoiceNumberWithoutIncrement()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    IF NOT EXISTS (SELECT 1 FROM FaturaNumaralari WHERE Yil = YEAR(GETDATE()))
                    BEGIN
                        INSERT INTO FaturaNumaralari (Yil, SonNumara, Rezervasyon)
                        VALUES (YEAR(GETDATE()), 0, 0);
                    END;

                    SELECT CONCAT(
                        CASE @FaturaTuruID
                            WHEN 1 THEN 'RE-SF-' -- Satış Faturası
                            WHEN 2 THEN 'RE-AF-' -- Alış Faturası
                            WHEN 4 THEN 'RE-AIF-' -- Alış İade Faturası
                            WHEN 3 THEN 'RE-SIF-' -- Satış İade Faturası
                            ELSE 'REF-'           -- Genel Fatura
                        END,
                        YEAR(GETDATE()),
                        FORMAT(Rezervasyon + 1, '000000')
                    ) AS InvoiceNumber
                    FROM FaturaNumaralari
                    WHERE Yil = YEAR(GETDATE()) AND FaturaTuruID = @FaturaTuruID;
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FaturaTuruID", ddlFaturaTuru.SelectedValue);
                return cmd.ExecuteScalar()?.ToString() ?? "HATA";
            }
        }

        protected void BtnUpdateInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ddlCari.SelectedValue) || string.IsNullOrEmpty(ddlFaturaTuru.SelectedValue))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Cari ve Fatura Türü seçilmeden fatura kaydedilemez.', 'error');", true);
                    return;
                }

                var detailsJson = hfInvoiceDetails.Value;
                var details = new JavaScriptSerializer().Deserialize<List<InvoiceDetail>>(detailsJson);

                if (details == null || details.Count == 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Fatura içeriği boş! Lütfen en az bir ürün ekleyin.', 'error');", true);
                    return;
                }

                details.RemoveAll(d => d.Quantity <= 0 || d.UnitPrice <= 0);

                if (details.Count == 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Geçerli miktar ve birim fiyat girilmeden fatura kaydedilemez.', 'error');", true);
                    return;
                }

                // Fatura tarihlerini kontrol et
                DateTime faturaTarihi = DateTime.Today;
                DateTime vadeTarihi = faturaTarihi;

                if (!string.IsNullOrEmpty(txtFaturaTarihi.Text))
                {
                    faturaTarihi = DateTime.Parse(txtFaturaTarihi.Text);
                }

                if (!string.IsNullOrEmpty(txtVadeTarihi.Text))
                {
                    vadeTarihi = DateTime.Parse(txtVadeTarihi.Text);
                }

                // Fatura toplamlarını hesapla
                decimal araToplam = 0;
                decimal kdvToplam = 0;
                decimal genelToplam = 0;
                decimal kdvOrani = 0.12m; // %12 KDV

                foreach (var detail in details)
                {
                    araToplam += detail.UnitPrice * detail.Quantity;
                    kdvToplam += detail.LineVatTotal;
                }
                genelToplam = araToplam + kdvToplam;

                // Kullanıcı ID'sini kontrol et ve ekle
                if (!Guid.TryParse(Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B", out Guid createdByUserId))
                {
                    throw new ArgumentException("Invalid createdByUserId format");
                }

                if (!Guid.TryParse(ddlCari.SelectedValue, out Guid customerId))
                {
                    throw new ArgumentException("Invalid customerId format");
                }

                if (!int.TryParse(ddlFaturaTuru.SelectedValue, out int invoiceTypeId))
                {
                    throw new ArgumentException("Invalid invoiceTypeId format");
                }

                // Güncelleme kontrolü
                if (!Guid.TryParse(Request.QueryString["InvoiceID"], out Guid existingInvoiceId))
                {
                    throw new ArgumentException("Invalid InvoiceID format in query string");
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Fatura güncelleme
                            string irsaliyeNumarasi = _invoiceService.UpdateInvoice(
                                existingInvoiceId,
                                lblFaturaNumarasi.Text,
                                customerId,
                                invoiceTypeId,
                                faturaTarihi,
                                araToplam,
                                kdvToplam,
                                genelToplam,
                                kdvOrani,
                                createdByUserId,
                                vadeTarihi,
                                int.Parse(ddlOdemeTuru.SelectedValue),
                                txtFaturaNotu.Text,
                                ddlDovizTuru.SelectedValue,
                                decimal.Parse(txtDovizKuru.Text),
                                details,
                                chkResmi.Checked,
                                hfHareketTuru.Value,
                                transaction, // transaction parametresi
                                conn); // conn parametresi

                            if (string.IsNullOrEmpty(irsaliyeNumarasi))
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Fatura güncellenirken bir hata oluştu.', 'error');", true);
                                transaction.Rollback();
                                return;
                            }

                            // Cari hareketleri güncelle
                            _invoiceService.UpdateCariHareketler(
                                existingInvoiceId,
                                customerId,
                                genelToplam,
                                faturaTarihi,
                                hfHareketTuru.Value,
                                createdByUserId,
                                transaction,
                                conn,
                                existingInvoiceId);

                            transaction.Commit();

                            // Başarılı mesajı göster ve invoices.aspx sayfasına yönlendir
                            string script = $@"
                                Swal.fire({{
                                    title: 'Başarılı!',
                                    text: '{lblFaturaNumarasi.Text} numaralı fatura ve {irsaliyeNumarasi} numaralı irsaliye başarıyla güncellendi.',
                                    icon: 'success'
                                }}).then(function() {{
                                    window.location.href = 'invoices.aspx';
                                }});";
                            ScriptManager.RegisterStartupScript(this, GetType(), "showalert", script, true);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ScriptManager.RegisterStartupScript(this, GetType(), "showalert", $"Swal.fire('Hata!', 'Fatura güncellenirken bir hata oluştu: {ex.Message}', 'error');", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", $"Swal.fire('Hata!', 'Fatura güncellenirken bir hata oluştu: {ex.Message}', 'error');", true);
            }
        }

        protected void ddlFaturaTuru_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblFaturaNumarasi.Text = GetInvoiceNumberWithoutIncrement();
            upFaturaNumarasi.Update(); // UpdatePanel'i güncelle

            // Fatura türüne göre gerekli işlemler yapılır
            // Örneğin, ürün ve birim listeleri güncellenebilir

            // Ürün ve birim seçeneklerini güncelle
            SetProductOptionsJson();
            SetUnitOptionsJson();

            // JavaScript kodlarını tekrar kaydet
            RegisterProductsScript();
            RegisterUnitsScript();

            // Fatura detaylarını tekrar bağla
            BindInvoiceDetails();

            // HareketTuru değerini güncelle
            hfHareketTuru.Value = ddlFaturaTuru.SelectedItem.Text;
            ViewState["HareketTuru"] = ddlFaturaTuru.SelectedItem.Text;
            System.Diagnostics.Debug.WriteLine($"ddlFaturaTuru_SelectedIndexChanged - hfHareketTuru.Value: {hfHareketTuru.Value}");
        }

        protected void BtnSaveInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                // HareketTuru değerini kontrol et
                if (string.IsNullOrEmpty(hfHareketTuru.Value))
                {
                    // Önce ViewState'den almayı dene
                    hfHareketTuru.Value = ViewState["HareketTuru"]?.ToString();

                    // Hala boşsa dropdown'dan al
                    if (string.IsNullOrEmpty(hfHareketTuru.Value))
                    {
                        hfHareketTuru.Value = ddlFaturaTuru.SelectedItem.Text;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"BtnSaveInvoice_Click - Final hfHareketTuru.Value: {hfHareketTuru.Value}");

                if (string.IsNullOrEmpty(ddlCari.SelectedValue) || string.IsNullOrEmpty(ddlFaturaTuru.SelectedValue))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Cari ve Fatura Türü seçilmeden fatura kaydedilemez.', 'error');", true);
                    return;
                }

                var detailsJson = hfInvoiceDetails.Value;
                var details = new JavaScriptSerializer().Deserialize<List<InvoiceDetail>>(detailsJson);

                if (details == null || details.Count == 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Fatura içeriği boş! Lütfen en az bir ürün ekleyin.', 'error');", true);
                    return;
                }

                details.RemoveAll(d => d.Quantity <= 0 || d.UnitPrice <= 0);

                if (details.Count == 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Geçerli miktar ve birim fiyat girilmeden fatura kaydedilemez.', 'error');", true);
                    return;
                }

                // Fatura tarihlerini kontrol et
                DateTime faturaTarihi = DateTime.Today;
                DateTime vadeTarihi = faturaTarihi;

                if (!string.IsNullOrEmpty(txtFaturaTarihi.Text))
                {
                    faturaTarihi = DateTime.Parse(txtFaturaTarihi.Text);
                }

                if (!string.IsNullOrEmpty(txtVadeTarihi.Text))
                {
                    vadeTarihi = DateTime.Parse(txtVadeTarihi.Text);
                }

                // Fatura toplamlarını hesapla
                decimal araToplam = 0;
                decimal kdvToplam = 0;
                decimal genelToplam = 0;
                decimal kdvOrani = 0.12m; // %12 KDV

                foreach (var detail in details)
                {
                    araToplam += detail.UnitPrice * detail.Quantity;
                    kdvToplam += detail.LineVatTotal;
                }
                genelToplam = araToplam + kdvToplam;

                // Kullanıcı ID'sini kontrol et ve ekle
                if (!Guid.TryParse(Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B", out Guid createdByUserId))
                {
                    throw new ArgumentException("Invalid createdByUserId format");
                }

                if (!Guid.TryParse(ddlCari.SelectedValue, out Guid customerId))
                {
                    throw new ArgumentException("Invalid customerId format");
                }

                if (!int.TryParse(ddlFaturaTuru.SelectedValue, out int invoiceTypeId))
                {
                    throw new ArgumentException("Invalid invoiceTypeId format");
                }

                System.Diagnostics.Debug.WriteLine($"BtnSaveInvoice_Click - hfHareketTuru.Value: {hfHareketTuru.Value}");

                // Fatura kaydetme
                var (invoiceID, irsaliyeNumarasi) = _invoiceService.SaveInvoice(
                    lblFaturaNumarasi.Text,
                    customerId,
                    invoiceTypeId,
                    faturaTarihi,
                    araToplam,
                    kdvToplam,
                    genelToplam,
                    kdvOrani,
                    createdByUserId,
                    vadeTarihi,
                    int.Parse(ddlOdemeTuru.SelectedValue),
                    txtFaturaNotu.Text,
                    ddlDovizTuru.SelectedValue,
                    decimal.Parse(txtDovizKuru.Text),
                    details,
                    chkResmi.Checked,
                    hfHareketTuru.Value);

                if (invoiceID == Guid.Empty)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "Swal.fire('Hata!', 'Fatura kaydedilirken bir hata oluştu.', 'error');", true);
                    return;
                }

                IncrementReservation(); // Yeni fatura numarasını artır
                hfInvoiceNumber.Value = lblFaturaNumarasi.Text; // Fatura numarasını hidden field'a kaydet

                // Başarılı mesajı göster ve sayfayı yenile
                string script = $@"
        Swal.fire({{
            title: 'Başarılı!',
            text: '{lblFaturaNumarasi.Text} numaralı fatura ve {irsaliyeNumarasi} numaralı irsaliye başarıyla kaydedildi.',
            icon: 'success'
        }}).then(function() {{
            window.location.href = window.location.href;
        }});";
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", script, true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", $"Swal.fire('Hata!', 'Fatura kaydedilirken bir hata oluştu: {ex.Message}', 'error');", true);
            }
        }





    }
}