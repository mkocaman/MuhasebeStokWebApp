using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class CashAccounts : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCashAccounts();
                LoadCurrencies();

                string kasaID = Request.QueryString["KasaID"];
                if (!string.IsNullOrEmpty(kasaID))
                {
                    LoadCashAccountDetails(kasaID);
                }
            }

            // Eğer silme işlemi başarılı olduysa, SweetAlert göster ve Session'ı temizle
            if (Session["deleteSuccess"] != null && (bool)Session["deleteSuccess"])
            {
                Session["deleteSuccess"] = null; // Tekrar çalışmasını engelle

                ScriptManager.RegisterStartupScript(this, GetType(), "deleteSuccess", @"
            Swal.fire({
                title: 'Başarılı!',
                text: 'Kasa başarıyla silindi.',
                icon: 'success',
                confirmButtonText: 'Tamam'
            }).then(() => {
                window.location.href = 'CashAccounts.aspx';
            });
        ", true);
            }

            // __doPostBack ile gelen komutu kontrol et
            if (Request["__EVENTTARGET"] == "DeleteCashAccount" && !string.IsNullOrEmpty(Request["__EVENTARGUMENT"]))
            {
                string kasaID = Request["__EVENTARGUMENT"];
                DeleteCashAccount(kasaID);
            }
        }

        /// <summary>
        /// Kasaları listeler ve döviz sembolünü getirir.
        /// </summary>
        private void LoadCashAccounts()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT 
            k.KasaID,
            k.KasaAdi,
            d.DovizSembol,
            COALESCE(SUM(CASE WHEN kh.IslemTuru = 'Tahsilat' AND kh.SoftDelete = 0 THEN kh.Tutar ELSE 0 END), 0) -
            COALESCE(SUM(CASE WHEN kh.IslemTuru = 'Ödeme' AND kh.SoftDelete = 0 THEN kh.Tutar ELSE 0 END), 0) AS Bakiye
        FROM Kasalar k
        LEFT JOIN KasaHareketleri kh ON k.KasaID = kh.KasaID AND kh.SoftDelete = 0
        LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
        WHERE k.SoftDelete = 0
        GROUP BY k.KasaID, k.KasaAdi, d.DovizSembol
        ORDER BY k.KasaAdi ASC;";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptCashAccounts.DataSource = dt;
                rptCashAccounts.DataBind();
            }
        }

        private void LoadCashAccountDetails(string kasaID)
        {
            // Veritabanından veriyi çek
            decimal bakiye = GetCurrentBalance(kasaID);
            string formattedBakiye = bakiye.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));

            txtEditBalance.Text = formattedBakiye;
            // Diğer alanları doldur
        }

        /// <summary>
        /// Döviz türlerini dropdown listeye yükler.
        /// </summary>
        private void LoadCurrencies()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT DovizID, DovizSembol FROM Dovizler WHERE DovizSembol IS NOT NULL ORDER BY DovizSembol desc";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlNewCurrency.DataSource = dt;
                ddlNewCurrency.DataTextField = "DovizSembol";
                ddlNewCurrency.DataValueField = "DovizID";
                ddlNewCurrency.DataBind();

                ddlEditCurrency.DataSource = dt;
                ddlEditCurrency.DataTextField = "DovizSembol";
                ddlEditCurrency.DataValueField = "DovizID";
                ddlEditCurrency.DataBind();

                ddlNewCurrency.Items.Insert(0, new ListItem("Döviz Seçiniz", ""));
                ddlEditCurrency.Items.Insert(0, new ListItem("Döviz Seçiniz", ""));
            }
        }

        /// <summary>
        /// Yeni kasa ekler.
        /// </summary>
        protected void btnSaveNewCashAccount_Click(object sender, EventArgs e)
        {
            string kasaAdi = txtNewCashAccountName.Text.Trim();
            decimal bakiye = string.IsNullOrEmpty(txtNewBalance.Text) ? 0 : Convert.ToDecimal(txtNewBalance.Text);
            string dovizID = ddlNewCurrency.SelectedValue;
            string olusturanKullaniciID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";

            if (string.IsNullOrWhiteSpace(kasaAdi) || string.IsNullOrEmpty(dovizID))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error", "Swal.fire('Hata!', 'Lütfen tüm alanları doldurun.', 'error');", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Yeni Kasa Ekle
                    string kasaID = Guid.NewGuid().ToString();
                    string insertKasaQuery = "INSERT INTO Kasalar (KasaID, KasaAdi, Bakiye, DovizID, SoftDelete, OlusturmaTarihi, OlusturanKullaniciID) " +
                                             "VALUES (@KasaID, @KasaAdi, @Bakiye, @DovizID, 0, GETDATE(), @OlusturanKullaniciID)";
                    SqlCommand cmdKasa = new SqlCommand(insertKasaQuery, conn, transaction);
                    cmdKasa.Parameters.AddWithValue("@KasaID", kasaID);
                    cmdKasa.Parameters.AddWithValue("@KasaAdi", kasaAdi);
                    cmdKasa.Parameters.AddWithValue("@Bakiye", bakiye);
                    cmdKasa.Parameters.AddWithValue("@DovizID", dovizID);
                    cmdKasa.Parameters.AddWithValue("@OlusturanKullaniciID", olusturanKullaniciID);
                    cmdKasa.ExecuteNonQuery();

                    // Eğer bakiye sıfırdan büyükse kasa hareketi ekle
                    if (bakiye > 0)
                    {
                        string insertHareketQuery = "INSERT INTO KasaHareketleri (HareketID, KasaID, IslemTarihi, Tutar, IslemTuru, Aciklama, OlusturanKullaniciID) " +
                                                    "VALUES (NEWID(), @KasaID, GETDATE(), @Bakiye, 'Tahsilat', 'Kasa Açılış Bakiyesi', @OlusturanKullaniciID)";
                        SqlCommand cmdHareket = new SqlCommand(insertHareketQuery, conn, transaction);
                        cmdHareket.Parameters.AddWithValue("@KasaID", kasaID);
                        cmdHareket.Parameters.AddWithValue("@Bakiye", bakiye);
                        cmdHareket.Parameters.AddWithValue("@OlusturanKullaniciID", olusturanKullaniciID);
                        cmdHareket.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    LoadCashAccounts();
                    ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Başarılı!', 'Kasa eklendi.', 'success').then(() => { $('#addCashAccountModal').modal('hide'); });", true);
                }
                catch
                {
                    transaction.Rollback();
                    ScriptManager.RegisterStartupScript(this, GetType(), "error", "Swal.fire('Hata!', 'Kasa eklenirken bir hata oluştu.', 'error');", true);
                }
            }
        }


        protected void rptCashAccounts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string kasaID = e.CommandArgument.ToString();

            if (e.CommandName == "Edit")
            {
                LoadCashAccountForEdit(kasaID);
            }
            else if (e.CommandName == "Transactions")
            {
                Response.Redirect($"CashTransactions.aspx?KasaID={kasaID}");
            }
            else if (e.CommandName == "Delete")
            {
                DeleteCashAccount(kasaID);
            }
        }


        private void DeleteCashAccount(string kasaID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Kasalar SET SoftDelete = 1 WHERE KasaID = @KasaID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@KasaID", kasaID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // SweetAlert'in tekrar tekrar açılmasını engellemek için Session kullan
            Session["deleteSuccess"] = true;

            // Sayfayı yenileyerek Script'in tekrar çalışmasını engelle
            Response.Redirect(Request.RawUrl);
        }

        private void LoadCashAccountForEdit(string kasaID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT KasaAdi, Bakiye, DovizID FROM Kasalar WHERE KasaID = @KasaID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@KasaID", kasaID);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    hfEditKasaID.Value = kasaID;
                    txtEditCashAccountName.Text = reader["KasaAdi"].ToString();
                    txtEditBalance.Text = Convert.ToDecimal(reader["Bakiye"]).ToString("N2", System.Globalization.CultureInfo.InvariantCulture);
                    ddlEditCurrency.SelectedValue = reader["DovizID"].ToString();
                }
                conn.Close();
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "openEditModal", "showEditModal();", true);
        }
        private string GetCurrentDovizSymbol(string kasaID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT d.DovizSembol 
            FROM Kasalar k
            LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
            WHERE k.KasaID = @KasaID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@KasaID", kasaID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
        }

        private string GetCurrentDovizID(string kasaID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT DovizID FROM Kasalar WHERE KasaID = @KasaID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@KasaID", kasaID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
        }
        private decimal GetCurrentBalance(string kasaID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Bakiye FROM Kasalar WHERE KasaID = @KasaID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@KasaID", kasaID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToDecimal(result) : 0;
            }
        }

        /// <summary>
        /// Kasa güncelleme işlemini yapar.
        /// </summary>
        protected void btnUpdateCashAccount_Click(object sender, EventArgs e)
        {
            string kasaID = hfEditKasaID.Value;
            string kasaAdi = txtEditCashAccountName.Text.Trim();
            string bakiyeText = txtEditBalance.Text.Trim();
            string dovizID = ddlEditCurrency.SelectedValue;

            // Döviz değişikliğine izin verme
            string mevcutDovizSembol = GetCurrentDovizSymbol(kasaID);
            string mevcutDovizID = GetCurrentDovizID(kasaID);
            if (mevcutDovizID != dovizID)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error", "Swal.fire('Hata!', 'Döviz değişikliğine izin verilmiyor!', 'error');", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(kasaAdi) || string.IsNullOrEmpty(dovizID))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error", "Swal.fire('Hata!', 'Lütfen tüm alanları doldurun.', 'error');", true);
                return;
            }

            // Bakiye format dönüşümünü düzelt
            bakiyeText = bakiyeText.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(bakiyeText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal yeniBakiye))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error", "Swal.fire('Hata!', 'Geçerli bir bakiye girin.', 'error');", true);
                return;
            }

            decimal eskiBakiye = GetCurrentBalance(kasaID);
            decimal fark = yeniBakiye - eskiBakiye;
            string islemTuru = fark > 0 ? "Tahsilat" : "Ödeme";
            decimal absFark = Math.Abs(fark);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Kasa güncelleme
                    string updateQuery = "UPDATE Kasalar SET KasaAdi = @KasaAdi, Bakiye = @Bakiye WHERE KasaID = @KasaID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@KasaAdi", kasaAdi);
                        cmd.Parameters.AddWithValue("@Bakiye", yeniBakiye);
                        cmd.Parameters.AddWithValue("@KasaID", kasaID);
                        cmd.ExecuteNonQuery();
                    }

                    // Bakiye değişmişse hareket ekle
                    if (fark != 0)
                    {
                        string insertHareketQuery = @"
                INSERT INTO KasaHareketleri (HareketID, KasaID, IslemTarihi, Tutar, IslemTuru, Aciklama, SoftDelete)
                VALUES (NEWID(), @KasaID, GETDATE(), @Tutar, @IslemTuru, @Aciklama, 0)";

                        using (SqlCommand cmd = new SqlCommand(insertHareketQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@KasaID", kasaID);
                            cmd.Parameters.AddWithValue("@Tutar", absFark);
                            cmd.Parameters.AddWithValue("@IslemTuru", islemTuru);
                            cmd.Parameters.AddWithValue("@Aciklama", $"Manuel Kasa Bakiyesi Değişikliği: {eskiBakiye.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))} {mevcutDovizSembol} → {yeniBakiye.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))} {mevcutDovizSembol}");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();

                    ScriptManager.RegisterStartupScript(this, GetType(), "success", @"
            Swal.fire({
                title: 'Başarılı!',
                text: 'Kasa başarıyla güncellendi.',
                icon: 'success',
                confirmButtonText: 'Tamam'
            }).then(() => {
                window.location.href = 'CashAccounts.aspx'; // Sayfayı yenile
            });
            ", true);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ScriptManager.RegisterStartupScript(this, GetType(), "error", $"Swal.fire('Hata!', 'İşlem sırasında hata oluştu: {ex.Message}', 'error');", true);
                }
            }
        }

    }
}