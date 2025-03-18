using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class BankTransactions : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Kültür ayarları
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("tr-TR");

            if (!IsPostBack)
            {
                PopulateBankAccountFilter();
                PopulateCustomerFilter();
                PopulateYearFilter();
                LoadBanks();
                LoadTransferBanks();
                LoadCustomers();
                LoadTransactions();

                // Session'dan banka hesap ID'sini al
                string bankaHesapID = Session["SelectedBankAccountID"] as string;
                if (!string.IsNullOrEmpty(txtStartDate.Text) && !string.IsNullOrEmpty(txtEndDate.Text))
                {
                    DateTime startDate = Convert.ToDateTime(txtStartDate.Text);
                    DateTime endDate = Convert.ToDateTime(txtEndDate.Text);
                    LoadBankTransactionsByDateRange(bankaHesapID, startDate, endDate);
                }
                else
                {
                    LoadBankTransactions(bankaHesapID);
                }
                hfFilterPanelState.Value = "closed";
            }

            string eventTarget = Request["__EVENTTARGET"];
            if (eventTarget == "ResetFilters")
            {
                ResetFilters();
            }
        }

        #region Filtreleme & Veri Yükleme

        protected void ddlTransactionBank_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedBankID = ddlTransactionBank.SelectedValue;
            if (!string.IsNullOrEmpty(selectedBankID))
            {
                LoadBankAccounts(selectedBankID, ddlTransactionBankAccount);
            }
            else
            {
                ddlTransactionBankAccount.Items.Clear();
                ddlTransactionBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            }
        }
        private void checkExchangeRate(string sourceBankAccountID, string targetBankAccountID,
          TextBox exchangeRateTextBox, TextBox convertedAmountTextBox, TextBox transferAmountTextBox)
        {
            if (string.IsNullOrEmpty(sourceBankAccountID) || string.IsNullOrEmpty(targetBankAccountID))
            {
                exchangeRateTextBox.Text = "";
                convertedAmountTextBox.Text = "";
                exchangeRateTextBox.Enabled = true;
                convertedAmountTextBox.Enabled = true;
                return;
            }

            dynamic currencySymbols = GetCurrencySymbols(sourceBankAccountID, targetBankAccountID);
            string sourceCurrency = currencySymbols.sourceCurrency;
            string targetCurrency = currencySymbols.targetCurrency;

            if (sourceCurrency == targetCurrency)
            {
                exchangeRateTextBox.Text = "1";
                exchangeRateTextBox.Enabled = false;
                convertedAmountTextBox.Enabled = false;
                decimal amount;
                if (decimal.TryParse(transferAmountTextBox.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out amount))
                {
                    convertedAmountTextBox.Text = amount.ToString("N2");
                }
                return;
            }

            exchangeRateTextBox.Enabled = true;
            convertedAmountTextBox.Enabled = true;

            // Sunucudan hesaplanmış oranı alalım (örn. GetExchangeRate metodunu kullanarak)
            decimal serverRate = GetExchangeRate(sourceCurrency, targetCurrency);

            // Şimdi, hedef USD ise özel gösterim yapalım
            if ((sourceCurrency == "TL" || sourceCurrency == "UZS") && targetCurrency == "USD")
            {
                // Gerçek oran 1 / dbRate, fakat textbox'ta "1/dbRate" formatında gösterilecek.
                exchangeRateTextBox.Text = "1/" + (1 / serverRate).ToString("N8"); // Burada 1/serverRate yerine oranın tersini hesaplamak için dbRate kullanılabilir.
            }
            else
            {
                exchangeRateTextBox.Text = serverRate.ToString("N4");
            }

            // Transfer miktarını okuyup, çevrilmiş tutarı hesaplayalım
            decimal transferAmount;
            if (decimal.TryParse(transferAmountTextBox.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out transferAmount))
            {
                decimal realRate = serverRate;
                if ((sourceCurrency == "TL" || sourceCurrency == "UZS") && targetCurrency == "USD")
                {
                    realRate = 1 / serverRate;
                }
                convertedAmountTextBox.Text = (transferAmount * realRate).ToString("N2");
            }
        }
        private void ResetFilters()
        {
            ddlBankAccountFilter.SelectedIndex = 0;
            ddlCustomerFilter.SelectedIndex = 0;
            ddlYearFilter.SelectedIndex = 0;
            ddlMonthFilter.SelectedIndex = 0;
            txtStartDate.Text = "";
            txtEndDate.Text = "";
            LoadTransactions();
        }

        protected void ddlSourceBankAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sourceBankAccountID = ddlSourceBankAccount.SelectedValue;
            string targetBankAccountID = ddlTargetBankAccount.SelectedValue;

            checkExchangeRate(sourceBankAccountID, targetBankAccountID, txtExchangeRate, txtConvertedAmount, txtTransferAmount);

            if (!string.IsNullOrEmpty(sourceBankAccountID))
            {
                decimal balance = GetBankAccountBalance(sourceBankAccountID);
                lblSourceBankBalance.Text = $"Bakiye: {balance:N2} {GetCurrencySymbol(sourceBankAccountID)}";
                lblSourceBankBalance.Visible = true;
            }
            else
            {
                lblSourceBankBalance.Text = string.Empty;
                lblSourceBankBalance.Visible = false;
            }
        }

        protected void btnApplyFilter_Click(object sender, EventArgs e)
        {
            string bankaHesapID = Session["SelectedBankAccountID"] as string;
            DateTime startDate, endDate;
            if (DateTime.TryParse(txtStartDate.Text, out startDate) && DateTime.TryParse(txtEndDate.Text, out endDate))
            {
                LoadBankTransactionsByDateRange(bankaHesapID, startDate, endDate);
            }
            hfFilterPanelState.Value = "open";
        }

        private void PopulateYearFilter()
        {
            ddlYearFilter.Items.Clear();
            ddlYearFilter.Items.Add(new ListItem("Tüm Yıllar", ""));
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= currentYear - 10; i--)
            {
                ddlYearFilter.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }
        }

        private void PopulateBankAccountFilter()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT bh.BankaHesapID, 
                           ba.BankaAdi + ' - ' + bh.HesapAdi + ' (' + bh.HesapNo + ')' AS HesapBilgi
                    FROM BankaHesaplari bh
                    LEFT JOIN Bankalar ba ON bh.BankaID = ba.BankaID
                    WHERE bh.SoftDelete = 0 
                    ORDER BY ba.BankaAdi, bh.HesapAdi";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlBankAccountFilter.DataSource = dt;
                ddlBankAccountFilter.DataTextField = "HesapBilgi";
                ddlBankAccountFilter.DataValueField = "BankaHesapID";
                ddlBankAccountFilter.DataBind();
                ddlBankAccountFilter.Items.Insert(0, new ListItem("Tüm Hesaplar", ""));
            }
        }

        private void PopulateCustomerFilter()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CariID, CariAdi FROM Cariler WHERE SoftDelete = 0 ORDER BY CariAdi";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlTransactionCustomer.DataSource = dt;
                ddlTransactionCustomer.DataTextField = "CariAdi";
                ddlTransactionCustomer.DataValueField = "CariID";
                ddlTransactionCustomer.DataBind();
                ddlTransactionCustomer.Items.Insert(0, new ListItem("Müşteri Seçin", ""));
                ddlEditTransactionCustomer.DataSource = dt;
                ddlEditTransactionCustomer.DataTextField = "CariAdi";
                ddlEditTransactionCustomer.DataValueField = "CariID";
                ddlEditTransactionCustomer.DataBind();
                ddlEditTransactionCustomer.Items.Insert(0, new ListItem("Müşteri Seçin", ""));
            }
        }

        private void LoadBankTransactions(string bankaHesapID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT bh.HareketID, bh.HareketNo, bh.IslemTarihi, bh.Tutar, bh.HareketTuru, 
                           bh.Aciklama, ba.BankaAdi AS BankaGosterim, bh.BankaHesapID, 
                           bh.CariID, bh.OlusturmaTarihi, 
                           h.HesapNo, c.CariAdi, d.DovizSembol, bh.TransferID
                    FROM BankaHareketleri bh
                    LEFT JOIN BankaHesaplari h ON bh.BankaHesapID = h.BankaHesapID
                    LEFT JOIN Bankalar ba ON h.BankaID = ba.BankaID
                    LEFT JOIN Cariler c ON bh.CariID = c.CariID
                    LEFT JOIN Dovizler d ON h.DovizID = d.DovizID
                    WHERE bh.SoftDelete = 0";
                if (!string.IsNullOrEmpty(bankaHesapID))
                {
                    query += " AND bh.BankaHesapID = @BankaHesapID";
                }
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(bankaHesapID))
                        cmd.Parameters.AddWithValue("@BankaHesapID", bankaHesapID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count == 0)
                    {
                        rptBankTransactions.DataSource = null;
                        rptBankTransactions.DataBind();
                        lblNoRecords.Visible = true;
                    }
                    else
                    {
                        rptBankTransactions.DataSource = dt;
                        rptBankTransactions.DataBind();
                        lblNoRecords.Visible = false;
                    }
                }
            }
        }

        private void LoadBankTransactionsByDateRange(string bankaHesapID, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT bh.HareketID, bh.HareketNo, bh.IslemTarihi, bh.Tutar, bh.HareketTuru, 
                           bh.Aciklama, ba.BankaAdi AS BankaGosterim, bh.BankaHesapID, 
                           bh.CariID, bh.OlusturmaTarihi, 
                           h.HesapNo, c.CariAdi, d.DovizSembol, bh.TransferID
                    FROM BankaHareketleri bh
                    LEFT JOIN BankaHesaplari h ON bh.BankaHesapID = h.BankaHesapID
                    LEFT JOIN Bankalar ba ON h.BankaID = ba.BankaID
                    LEFT JOIN Cariler c ON bh.CariID = c.CariID
                    LEFT JOIN Dovizler d ON h.DovizID = d.DovizID
                    WHERE bh.SoftDelete = 0 
                      AND bh.IslemTarihi BETWEEN @StartDate AND @EndDate";
                if (!string.IsNullOrEmpty(bankaHesapID))
                {
                    query += " AND bh.BankaHesapID = @BankaHesapID";
                }
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    if (!string.IsNullOrEmpty(bankaHesapID))
                        cmd.Parameters.AddWithValue("@BankaHesapID", bankaHesapID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    rptBankTransactions.DataSource = dt;
                    rptBankTransactions.DataBind();
                }
            }
        }

        private void LoadTransactions()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT bh.HareketID, bh.HareketNo, bh.IslemTarihi, bh.Tutar, bh.HareketTuru, 
                           bh.Aciklama, ba.BankaAdi AS BankaGosterim, bh.BankaHesapID, 
                           bh.CariID, bh.OlusturmaTarihi, 
                           h.HesapNo, c.CariAdi, d.DovizSembol, bh.TransferID
                    FROM BankaHareketleri bh
                    LEFT JOIN BankaHesaplari h ON bh.BankaHesapID = h.BankaHesapID
                    LEFT JOIN Bankalar ba ON h.BankaID = ba.BankaID
                    LEFT JOIN Cariler c ON bh.CariID = c.CariID
                    LEFT JOIN Dovizler d ON h.DovizID = d.DovizID
                    WHERE bh.SoftDelete = 0
                    ORDER BY bh.HareketNo DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count == 0)
                {
                    rptBankTransactions.DataSource = null;
                    rptBankTransactions.DataBind();
                    lblNoRecords.Visible = true;
                }
                else
                {
                    rptBankTransactions.DataSource = dt;
                    rptBankTransactions.DataBind();
                    lblNoRecords.Visible = false;
                }
            }
        }

        private void LoadBanks()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT BankaID, BankaAdi FROM Bankalar WHERE SoftDelete = 0 ORDER BY BankaAdi";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlTransactionBank.DataSource = dt;
                ddlTransactionBank.DataTextField = "BankaAdi";
                ddlTransactionBank.DataValueField = "BankaID";
                ddlTransactionBank.DataBind();

                ddlEditTransactionBank.DataSource = dt;
                ddlEditTransactionBank.DataTextField = "BankaAdi";
                ddlEditTransactionBank.DataValueField = "BankaID";
                ddlEditTransactionBank.DataBind();

                ddlTransactionBank.Items.Insert(0, new ListItem("Banka Seçiniz", ""));
                ddlEditTransactionBank.Items.Insert(0, new ListItem("Banka Seçiniz", ""));
            }
        }

        private void LoadTransferBanks()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT BankaID, BankaAdi FROM Bankalar WHERE SoftDelete = 0 ORDER BY BankaAdi ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlSourceBank.DataSource = dt;
                ddlSourceBank.DataTextField = "BankaAdi";
                ddlSourceBank.DataValueField = "BankaID";
                ddlSourceBank.DataBind();
                ddlSourceBank.Items.Insert(0, new ListItem("Kaynak Banka Seçin", ""));

                ddlTargetBank.DataSource = dt;
                ddlTargetBank.DataTextField = "BankaAdi";
                ddlTargetBank.DataValueField = "BankaID";
                ddlTargetBank.DataBind();
                ddlTargetBank.Items.Insert(0, new ListItem("Hedef Banka Seçin", ""));
            }
        }

        protected void ddlSourceBank_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedBankID = ddlSourceBank.SelectedValue;
            if (!string.IsNullOrEmpty(selectedBankID))
            {
                LoadTransferBankAccounts(selectedBankID, ddlSourceBankAccount);
            }
            else
            {
                ddlSourceBankAccount.Items.Clear();
                ddlSourceBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            }
        }

        protected void ddlTargetBank_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedBankID = ddlTargetBank.SelectedValue;
            if (!string.IsNullOrEmpty(selectedBankID))
            {
                LoadTransferBankAccounts(selectedBankID, ddlTargetBankAccount);
            }
            else
            {
                ddlTargetBankAccount.Items.Clear();
                ddlTargetBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            }
        }

        private void LoadTransferBankAccounts(string bankaID, DropDownList ddlBankAccount)
        {
            if (string.IsNullOrEmpty(bankaID))
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT BankaHesapID, HesapAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS HesapGosterim
                    FROM BankaHesaplari b
                    LEFT JOIN Dovizler d ON b.DovizID = d.DovizID
                    WHERE b.SoftDelete = 0 AND b.BankaID = @BankaID
                    ORDER BY HesapAdi ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaID", bankaID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlBankAccount.DataSource = dt;
                ddlBankAccount.DataTextField = "HesapGosterim";
                ddlBankAccount.DataValueField = "BankaHesapID";
                ddlBankAccount.DataBind();
                ddlBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            }
        }

        #endregion

        #region Ekleme, Güncelleme, Silme İşlemleri

        protected void btnSaveTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                // Sunucu tarafı validasyonu
                if (string.IsNullOrEmpty(ddlTransactionBankAccount.SelectedValue))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "validationError", @"
                Swal.fire({
                    title: 'Hata!',
                    text: 'Lütfen bir banka hesabı seçin!',
                    icon: 'error'
                });", true);
                    return;
                }

                // Müşteri seçimi kontrolü
                if (string.IsNullOrEmpty(ddlTransactionCustomer.SelectedValue))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "validationError", @"
                Swal.fire({
                    title: 'Hata!',
                    text: 'Lütfen bir müşteri seçin!',
                    icon: 'error'
                });", true);
                    return;
                }

                decimal tutar;
                if (!decimal.TryParse(txtTransactionAmount.Text?.Replace(".", "").Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out tutar) || tutar <= 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "validationError", @"
                Swal.fire({
                    title: 'Hata!',
                    text: 'Lütfen geçerli bir tutar girin!',
                    icon: 'error'
                });", true);
                    return;
                }
            
         

        DateTime islemTarihi;
                if (!DateTime.TryParse(txtTransactionDate.Text, out islemTarihi))
                    islemTarihi = DateTime.Now;

                string hesapID = ddlTransactionBankAccount.SelectedValue;
                string cariID = ddlTransactionCustomer.SelectedValue;
                string hareketTuru = ddlTransactionType.SelectedValue;
                string aciklama = txtTransactionDescription.Text;
                string olusturanKullaniciID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";
                string hareketNo = GenerateNextBankTransactionNumber();
                Guid bankaHareketID = Guid.NewGuid();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string bankQuery = @"
                        INSERT INTO BankaHareketleri 
                        (HareketID, HareketNo, BankaHesapID, IslemTarihi, Tutar, HareketTuru, Aciklama, CariID, OlusturanKullaniciID, SoftDelete, OlusturmaTarihi) 
                        VALUES 
                        (@HareketID, @HareketNo, @BankaHesapID, @IslemTarihi, @Tutar, @HareketTuru, @Aciklama, @CariID, @OlusturanKullaniciID, 0, GETDATE())";
                            SqlCommand bankCmd = new SqlCommand(bankQuery, conn, transaction);
                            bankCmd.Parameters.AddWithValue("@HareketID", bankaHareketID);
                            bankCmd.Parameters.AddWithValue("@HareketNo", hareketNo);
                            bankCmd.Parameters.AddWithValue("@BankaHesapID", hesapID);
                            bankCmd.Parameters.AddWithValue("@IslemTarihi", islemTarihi);
                            bankCmd.Parameters.AddWithValue("@Tutar", tutar);
                            bankCmd.Parameters.AddWithValue("@HareketTuru", hareketTuru);
                            bankCmd.Parameters.AddWithValue("@Aciklama", aciklama);
                            bankCmd.Parameters.AddWithValue("@CariID", string.IsNullOrEmpty(cariID) ? (object)DBNull.Value : cariID);
                            bankCmd.Parameters.AddWithValue("@OlusturanKullaniciID", olusturanKullaniciID);
                            bankCmd.ExecuteNonQuery();

                            if (!string.IsNullOrEmpty(cariID))
                            {
                                string cariQuery = @"
                            INSERT INTO CariHareketler 
                            (HareketID, CariID, IslemTarihi, Tutar, IslemTuru, Aciklama, EvrakID, OlusturanKullaniciID, SoftDelete, OlusturmaTarihi) 
                            VALUES 
                            (NEWID(), @CariID, @IslemTarihi, @Tutar, @IslemTuru, @Aciklama, @EvrakID, @OlusturanKullaniciID, 0, GETDATE())";
                                SqlCommand cariCmd = new SqlCommand(cariQuery, conn, transaction);
                                cariCmd.Parameters.AddWithValue("@CariID", cariID);
                                cariCmd.Parameters.AddWithValue("@IslemTarihi", islemTarihi);
                                cariCmd.Parameters.AddWithValue("@Tutar", tutar);
                                cariCmd.Parameters.AddWithValue("@IslemTuru", hareketTuru);
                                cariCmd.Parameters.AddWithValue("@Aciklama", aciklama);
                                cariCmd.Parameters.AddWithValue("@EvrakID", bankaHareketID);
                                cariCmd.Parameters.AddWithValue("@OlusturanKullaniciID", olusturanKullaniciID);
                                cariCmd.ExecuteNonQuery();
                            }
                            transaction.Commit();

                            // İşlem başarılı
                            ClearAddTransactionModal();
                            LoadTransactions();
                            UpdatePanel1.Update();
                            ScriptManager.RegisterStartupScript(this, GetType(), "success", @"
                        Swal.fire({
                            title: 'Başarılı!',
                            text: 'Banka hareketi başarıyla eklendi.',
                            icon: 'success'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                $('#addTransactionModal').modal('hide');
                                var form = document.getElementById('BankaHareket');
                                if (form) { form.reset(); }
                            }
                        });", true);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ScriptManager.RegisterStartupScript(this, GetType(), "error", $@"
                        Swal.fire({{
                            title: 'Hata!',
                            text: 'Kayıt işlemi sırasında hata oluştu: {ex.Message}',
                            icon: 'error'
                        }});", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Genel hata durumu
                ScriptManager.RegisterStartupScript(this, GetType(), "error", $@"
            Swal.fire({{
                title: 'Hata!',
                text: 'İşlem sırasında beklenmeyen bir hata oluştu: {ex.Message}',
                icon: 'error'
            }});", true);
            }
        }


        protected void btnSaveTransfer_Click(object sender, EventArgs e)
        {
            // Zorunlu alan kontrolü
            if (string.IsNullOrEmpty(ddlSourceBankAccount.SelectedValue) ||
                string.IsNullOrEmpty(ddlTargetBankAccount.SelectedValue) ||
                string.IsNullOrEmpty(txtTransferAmount.Text) ||
                string.IsNullOrEmpty(txtExchangeRate.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "validationError", @"
            Swal.fire({
                title: 'Hata!',
                text: 'Lütfen tüm zorunlu alanları doldurun!',
                icon: 'error'
            });", true);
                return;
            }

            string sourceBankID = ddlSourceBankAccount.SelectedValue;
            string targetBankID = ddlTargetBankAccount.SelectedValue;
            // Hedef banka adını almak için; örneğin ddlTargetBank seçili olan banka ID'si kullanılabilir.
            string bankIDForName = ddlTargetBank.SelectedValue;
            decimal amount = ParseTurkishDecimal(txtTransferAmount.Text);
            decimal exchangeRate = ParseTurkishDecimal(txtExchangeRate.Text);
            decimal convertedAmount = amount * exchangeRate;
            DateTime transactionDate;
            if (string.IsNullOrEmpty(txtTransferDate.Text) || !DateTime.TryParse(txtTransferDate.Text, out transactionDate))
                transactionDate = DateTime.Now;

            // Kullanıcının girdiği açıklama
            string userDesc = txtTransferDescription.Text;
            // Ayraç (sistem açıklaması ile kullanıcı açıklaması arasına eklenecek)
            string separator = " || ";

            string userID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";
            string sourceTransactionNo = GenerateNextBankTransactionNumber();
            string targetTransactionNo = GenerateNextBankTransactionNumber();
            Guid transferID = Guid.NewGuid();
            Guid transferOutID = Guid.NewGuid();
            Guid transferInID = Guid.NewGuid();

            // Banka hesap ID'lerinin GUID formatında olduğunu kontrol edelim
            Guid parsedSourceID, parsedTargetID;
            if (!Guid.TryParse(sourceBankID, out parsedSourceID) || !Guid.TryParse(targetBankID, out parsedTargetID))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "validationError", @"
            Swal.fire({
                title: 'Hata!',
                text: 'Banka hesabı bilgileri geçersiz!',
                icon: 'error'
            });", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Sistemin oluşturduğu açıklama kısmı
                        string sistemDescOut = $"Transfer Çıkış: {amount:N2} {GetCurrencySymbol(sourceBankID)} ➝ {GetBankAccountName(bankIDForName)} (Kur: {exchangeRate:N4})";
                        string descOut = sistemDescOut + separator + userDesc;
                        string queryOut = @"
                    INSERT INTO BankaHareketleri 
                    (HareketID, TransferID, HareketNo, BankaHesapID, IslemTarihi, Tutar, HareketTuru, Aciklama, TransferYonu, OlusturanKullaniciID, SoftDelete, OlusturmaTarihi) 
                    VALUES 
                    (@HareketID1, @TransferID, @HareketNo1, @SourceBankID, @IslemTarihi, @Amount, 'Ödeme', @DescriptionOut, @TransferYonuOut, @UserID, 0, GETDATE())";
                        SqlCommand cmdOut = new SqlCommand(queryOut, conn, transaction);
                        cmdOut.Parameters.AddWithValue("@HareketID1", transferOutID);
                        cmdOut.Parameters.AddWithValue("@TransferID", transferID);
                        cmdOut.Parameters.AddWithValue("@HareketNo1", sourceTransactionNo);
                        cmdOut.Parameters.AddWithValue("@SourceBankID", sourceBankID);
                        cmdOut.Parameters.AddWithValue("@IslemTarihi", transactionDate);
                        cmdOut.Parameters.AddWithValue("@Amount", amount);
                        cmdOut.Parameters.AddWithValue("@DescriptionOut", descOut);
                        cmdOut.Parameters.AddWithValue("@TransferYonuOut", "OUT");
                        cmdOut.Parameters.AddWithValue("@UserID", userID);
                        cmdOut.ExecuteNonQuery();

                        string sistemDescIn = $"Transfer Giriş: {amount:N2} {GetCurrencySymbol(sourceBankID)} ➝ {convertedAmount:N2} {GetCurrencySymbol(targetBankID)} (Kur: {exchangeRate:N4})";
                        string descIn = sistemDescIn + separator + userDesc;
                        string queryIn = @"
                    INSERT INTO BankaHareketleri 
                    (HareketID, TransferID, HareketNo, BankaHesapID, IslemTarihi, Tutar, HareketTuru, Aciklama, TransferYonu, OlusturanKullaniciID, SoftDelete, OlusturmaTarihi) 
                    VALUES 
                    (@HareketID2, @TransferID, @HareketNo2, @TargetBankID, @IslemTarihi, @ConvertedAmount, 'Tahsilat', @DescriptionIn, @TransferYonuIn, @UserID, 0, GETDATE())";
                        SqlCommand cmdIn = new SqlCommand(queryIn, conn, transaction);
                        cmdIn.Parameters.AddWithValue("@HareketID2", transferInID);
                        cmdIn.Parameters.AddWithValue("@TransferID", transferID);
                        cmdIn.Parameters.AddWithValue("@HareketNo2", targetTransactionNo);
                        cmdIn.Parameters.AddWithValue("@TargetBankID", targetBankID);
                        cmdIn.Parameters.AddWithValue("@IslemTarihi", transactionDate);
                        cmdIn.Parameters.AddWithValue("@ConvertedAmount", convertedAmount);
                        cmdIn.Parameters.AddWithValue("@DescriptionIn", descIn);
                        cmdIn.Parameters.AddWithValue("@TransferYonuIn", "IN");
                        cmdIn.Parameters.AddWithValue("@UserID", userID);
                        cmdIn.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Transfer işlemi sırasında hata oluştu: " + ex.Message);
                    }
                }
            }
            ClearTransferModal();
            LoadTransactions();
            UpdatePanel1.Update();
            ScriptManager.RegisterStartupScript(this, GetType(), "success", @"
        Swal.fire({
            title: 'Başarılı!',
            text: 'Banka transferi başarıyla eklendi.',
            icon: 'success'
        }).then((result) => {
            if (result.isConfirmed) {
                $('#transferModal').modal('hide');
                var form = document.getElementById('pnlTransferForm');
                if (form) {
                    form.reset();
                    form.classList.remove('was-validated');
                }
            }
        });
    ", true);
        }

        protected void btnUpdateTransfer_Click(object sender, EventArgs e)
        {
            string transferID = hfEditTransferID.Value;
            string sourceBankAccountID = ddlEditSourceBankAccount.SelectedValue;
            string targetBankAccountID = ddlEditTargetBankAccount.SelectedValue;
            decimal amount = ParseTurkishDecimal(txtEditTransferAmount.Text);
            decimal exchangeRate = ParseTurkishDecimal(txtEditExchangeRate.Text);
            decimal convertedAmount = amount * exchangeRate;
            DateTime transactionDate;
            if (string.IsNullOrEmpty(txtEditTransferDate.Text) ||
                !DateTime.TryParse(txtEditTransferDate.Text, out transactionDate))
                transactionDate = DateTime.Now;
            // Kullanıcının düzenlediği açıklamaları alıyoruz
            string userDescSource = txtEditTransferDescription.Text;
            string userDescTarget = txtEditTransferDescription.Text;
            string separator = " || ";
            string userID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Sistem tarafından oluşturulan açıklamaları yeniden oluşturuyoruz
                        string sistemDescOut = $"Transfer Çıkış: {amount:N2} {GetCurrencySymbol(sourceBankAccountID)} ➝ {GetBankAccountName(GetBankIDByBankAccountID(targetBankAccountID))} (Kur: {exchangeRate:N4})";
                        string descOut = sistemDescOut + separator + userDescSource;

                        string queryOut = @"
                    UPDATE BankaHareketleri 
                    SET BankaHesapID = @SourceBankID, IslemTarihi = @IslemTarihi, Tutar = @Amount, 
                        Aciklama = @SourceDescription, SonGuncelleyenKullaniciID = @UserID, GuncellemeTarihi = GETDATE() 
                    WHERE TransferID = @TransferID AND HareketTuru = 'Ödeme'";
                        SqlCommand cmdOut = new SqlCommand(queryOut, conn, transaction);
                        cmdOut.Parameters.AddWithValue("@SourceBankID", sourceBankAccountID);
                        cmdOut.Parameters.AddWithValue("@IslemTarihi", transactionDate);
                        cmdOut.Parameters.AddWithValue("@Amount", amount);
                        cmdOut.Parameters.AddWithValue("@SourceDescription", descOut);
                        cmdOut.Parameters.AddWithValue("@UserID", userID);
                        cmdOut.Parameters.AddWithValue("@TransferID", transferID);
                        cmdOut.ExecuteNonQuery();

                        string sistemDescIn = $"Transfer Giriş: {amount:N2} {GetCurrencySymbol(sourceBankAccountID)} ➝ {convertedAmount:N2} {GetCurrencySymbol(targetBankAccountID)} (Kur: {exchangeRate:N4})";
                        string descIn = sistemDescIn + separator + userDescTarget;

                        string queryIn = @"
                    UPDATE BankaHareketleri 
                    SET BankaHesapID = @TargetBankID, IslemTarihi = @IslemTarihi, Tutar = @ConvertedAmount, 
                        Aciklama = @TargetDescription, SonGuncelleyenKullaniciID = @UserID, GuncellemeTarihi = GETDATE() 
                    WHERE TransferID = @TransferID AND HareketTuru = 'Tahsilat'";
                        SqlCommand cmdIn = new SqlCommand(queryIn, conn, transaction);
                        cmdIn.Parameters.AddWithValue("@TargetBankID", targetBankAccountID);
                        cmdIn.Parameters.AddWithValue("@IslemTarihi", transactionDate);
                        cmdIn.Parameters.AddWithValue("@ConvertedAmount", convertedAmount);
                        cmdIn.Parameters.AddWithValue("@TargetDescription", descIn);
                        cmdIn.Parameters.AddWithValue("@UserID", userID);
                        cmdIn.Parameters.AddWithValue("@TransferID", transferID);
                        cmdIn.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Transfer güncelleme işlemi sırasında hata oluştu: " + ex.Message);
                    }
                }
            }
            LoadTransactions();
            UpdatePanel1.Update();
            ScriptManager.RegisterStartupScript(this, GetType(), "success", @"
        Swal.fire({
            title: 'Başarılı!',
            text: 'Transfer başarıyla güncellendi.',
            icon: 'success'
        }).then(() => { $('#editTransferModal').modal('hide'); });
    ", true);
            Response.Redirect(Request.RawUrl);
        }

        protected void btnUpdateTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                // HiddenField'den gelen HareketID değerini Guid'e çevirelim:
                string hareketIDString = hfEditTransactionID.Value;
                if (!Guid.TryParse(hareketIDString, out Guid hareketID))
                {
                    throw new Exception("Geçersiz HareketID değeri.");
                }

                // Formdaki güncellenebilir verileri alalım:
                string bankaHesapIDString = ddlEditTransactionBankAccount.SelectedValue;
                if (!Guid.TryParse(bankaHesapIDString, out Guid bankaHesapID))
                {
                    throw new Exception("Geçersiz BankaHesapID değeri.");
                }

                if (!DateTime.TryParse(txtEditTransactionDate.Text, out DateTime islemTarihi))
                    islemTarihi = DateTime.Now;

                if (!decimal.TryParse(txtEditTransactionAmount.Text, System.Globalization.NumberStyles.Any,
                    new System.Globalization.CultureInfo("tr-TR"), out decimal tutar) || tutar <= 0)
                    throw new Exception("Geçersiz tutar değeri.");

                string hareketTuru = ddlEditTransactionType.SelectedValue;
                string aciklama = txtEditTransactionDescription.Text;
                string cariIDString = ddlEditTransactionCustomer.SelectedValue;
                Guid? cariID = null;
                if (!string.IsNullOrEmpty(cariIDString))
                {
                    if (Guid.TryParse(cariIDString, out Guid parsedCariID))
                        cariID = parsedCariID;
                    else
                        throw new Exception("Geçersiz CariID değeri.");
                }

                string guncelleyenKullaniciIDString = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";
                if (!Guid.TryParse(guncelleyenKullaniciIDString, out Guid guncelleyenKullaniciID))
                    guncelleyenKullaniciID = Guid.Empty;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // BankaHareketleri tablosundaki güncelleme:
                            string updateQuery = @"
                        UPDATE BankaHareketleri 
                        SET 
                            BankaHesapID = @BankaHesapID,
                            IslemTarihi = @IslemTarihi,
                            Tutar = @Tutar,
                            HareketTuru = @HareketTuru,
                            Aciklama = @Aciklama,
                            CariID = @CariID,
                            SonGuncelleyenKullaniciID = @SonGuncelleyenKullaniciID,
                            GuncellemeTarihi = GETDATE()
                        WHERE HareketID = @HareketID";
                            SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn, transaction);
                            cmdUpdate.Parameters.AddWithValue("@BankaHesapID", bankaHesapID);
                            cmdUpdate.Parameters.AddWithValue("@IslemTarihi", islemTarihi);
                            cmdUpdate.Parameters.AddWithValue("@Tutar", tutar);
                            cmdUpdate.Parameters.AddWithValue("@HareketTuru", hareketTuru);
                            cmdUpdate.Parameters.AddWithValue("@Aciklama", aciklama);
                            if (cariID.HasValue)
                                cmdUpdate.Parameters.AddWithValue("@CariID", cariID.Value);
                            else
                                cmdUpdate.Parameters.AddWithValue("@CariID", DBNull.Value);
                            cmdUpdate.Parameters.AddWithValue("@SonGuncelleyenKullaniciID", guncelleyenKullaniciID);
                            cmdUpdate.Parameters.AddWithValue("@HareketID", hareketID);
                            cmdUpdate.ExecuteNonQuery();

                            // CariHareketler tablosunda güncelleme (CariID dolu ise)
                            if (cariID.HasValue)
                            {
                                string updateCariQuery = @"
                            UPDATE CariHareketler
                            SET 
                                IslemTarihi = @IslemTarihi,
                                Tutar = @Tutar,
                                IslemTuru = @HareketTuru,
                                Aciklama = @Aciklama,
                                CariID = @CariID,
                                SonGuncelleyenKullaniciID = @SonGuncelleyenKullaniciID,
                                GuncellemeTarihi = GETDATE()
                            WHERE EvrakID = @HareketID";
                                SqlCommand cmdCariUpdate = new SqlCommand(updateCariQuery, conn, transaction);
                                cmdCariUpdate.Parameters.AddWithValue("@IslemTarihi", islemTarihi);
                                cmdCariUpdate.Parameters.AddWithValue("@Tutar", tutar);
                                cmdCariUpdate.Parameters.AddWithValue("@HareketTuru", hareketTuru);
                                cmdCariUpdate.Parameters.AddWithValue("@Aciklama", aciklama);
                                cmdCariUpdate.Parameters.AddWithValue("@CariID", cariID.Value);
                                cmdCariUpdate.Parameters.AddWithValue("@SonGuncelleyenKullaniciID", guncelleyenKullaniciID);
                                cmdCariUpdate.Parameters.AddWithValue("@HareketID", hareketID);
                                cmdCariUpdate.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Update Error: " + ex.ToString());
                            transaction.Rollback();
                            throw new Exception("Güncelleme işlemi sırasında hata oluştu: " + ex.Message);
                        }
                    }
                }

                LoadTransactions();
                UpdatePanel1.Update();
                ScriptManager.RegisterStartupScript(this, GetType(), "success", @"
            try {
                Swal.fire({
                    title: 'Başarılı!',
                    text: 'Hareket başarıyla güncellendi.',
                    icon: 'success'
                }).then((result) => {
                    if (result.isConfirmed || result.isDismissed) {
                        closeEditTransactionModal();
                    }
                });
            } catch(ex) {
                console.error('Script hatası:', ex);
                $('#editTransactionModal').modal('hide');
            }", true);
                Response.Redirect(Request.RawUrl);
            
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error", $@"
            Swal.fire({{
                title: 'Hata!',
                text: 'Güncelleme işlemi sırasında hata oluştu: {ex.Message}',
                icon: 'error'
            }});", true);
            }
        }
        private void LoadTransactionDetails(string hareketID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT IslemTarihi, Aciklama, Tutar, CariID, HareketTuru FROM BankaHareketleri WHERE HareketID = @HareketID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HareketID", hareketID);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtEditTransactionDate.Text = Convert.ToDateTime(reader["IslemTarihi"]).ToString("yyyy-MM-dd");
                    txtEditTransactionAmount.Text = Convert.ToDecimal(reader["Tutar"]).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
                    txtEditTransactionDescription.Text = reader["Aciklama"].ToString();
                    ddlEditTransactionCustomer.SelectedValue = reader["CariID"].ToString();
                    ddlEditTransactionType.SelectedValue = reader["HareketTuru"].ToString();
                }
            }
        }

        protected void ddlEditTransactionBank_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedBankID = ddlEditTransactionBank.SelectedValue;
            ViewState["SelectedEditTransactionBank"] = selectedBankID;
            if (!string.IsNullOrEmpty(selectedBankID))
            {
                LoadBankAccounts(selectedBankID, ddlEditTransactionBankAccount);
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (ViewState["SelectedEditTransactionBank"] != null)
            {
                ddlEditTransactionBank.SelectedValue = ViewState["SelectedEditTransactionBank"].ToString();
            }
        }

        [WebMethod]
        public static object GetTransactionDetails(string hareketID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT HareketID, BankaHesapID, IslemTarihi, Tutar, HareketTuru, Aciklama, CariID
                    FROM BankaHareketleri
                    WHERE HareketID = @HareketID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HareketID", hareketID);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new
                        {
                            HareketID = reader["HareketID"].ToString(),
                            BankaHesapID = reader["BankaHesapID"].ToString(),
                            IslemTarihi = Convert.ToDateTime(reader["IslemTarihi"]).ToString("yyyy-MM-dd"),
                            Tutar = Convert.ToDecimal(reader["Tutar"]).ToString("N2"),
                            HareketTuru = reader["HareketTuru"].ToString(),
                            Aciklama = reader["Aciklama"].ToString(),
                            CariID = reader["CariID"]?.ToString() ?? ""
                        };
                    }
                }
            }
            return null;
        }

        private void DeleteTransaction(string hareketID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string deleteQuery = "UPDATE BankaHareketleri SET SoftDelete = 1 WHERE HareketID = @HareketID";
                    SqlCommand cmdDelete = new SqlCommand(deleteQuery, conn, transaction);
                    cmdDelete.Parameters.AddWithValue("@HareketID", hareketID);
                    cmdDelete.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Silme işlemi sırasında hata oluştu: " + ex.Message);
                }
            }
            LoadTransactions();
            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Başarılı!', 'Hareket başarıyla silindi.', 'success');", true);
        }

        protected void ddlEditSourceBank_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedBankID = ddlEditSourceBank.SelectedValue;
            if (!string.IsNullOrEmpty(selectedBankID))
            {
                LoadTransferBankAccounts(selectedBankID, ddlEditSourceBankAccount);
            }
            else
            {
                ddlEditSourceBankAccount.Items.Clear();
                ddlEditSourceBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            }
        }

        protected void ddlEditSourceBankAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sourceBankAccountID = ddlEditSourceBankAccount.SelectedValue;
            string targetBankAccountID = ddlEditTargetBankAccount.SelectedValue;
            checkExchangeRate(sourceBankAccountID, targetBankAccountID, txtEditExchangeRate, txtEditConvertedAmount, txtEditTransferAmount);
            if (!string.IsNullOrEmpty(sourceBankAccountID))
            {
                decimal balance = GetBankAccountBalance(sourceBankAccountID);
                lblSourceBankBalance.Text = $"Bakiye: {balance:N2} {GetCurrencySymbol(sourceBankAccountID)}";
                lblSourceBankBalance.Visible = true;
            }
            else
            {
                lblSourceBankBalance.Text = string.Empty;
                lblSourceBankBalance.Visible = false;
            }
        }

        protected void ddlEditTargetBank_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedBankID = ddlEditTargetBank.SelectedValue;
            if (!string.IsNullOrEmpty(selectedBankID))
            {
                LoadTransferBankAccounts(selectedBankID, ddlEditTargetBankAccount);
            }
            else
            {
                ddlEditTargetBankAccount.Items.Clear();
                ddlEditTargetBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            }
        }

        protected void ddlEditTargetBankAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sourceBankAccountID = ddlEditSourceBankAccount.SelectedValue;
            string targetBankAccountID = ddlEditTargetBankAccount.SelectedValue;
            checkExchangeRate(sourceBankAccountID, targetBankAccountID, txtEditExchangeRate, txtEditConvertedAmount, txtEditTransferAmount);
            if (!string.IsNullOrEmpty(targetBankAccountID))
            {
                decimal balance = GetBankAccountBalance(targetBankAccountID);
                lblTargetBankBalance.Text = $"Bakiye: {balance:N2} {GetCurrencySymbol(targetBankAccountID)}";
                lblTargetBankBalance.Visible = true;
            }
            else
            {
                lblTargetBankBalance.Text = string.Empty;
                lblTargetBankBalance.Visible = false;
            }
        }

        protected void txtEditTransferAmount_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtEditTransferAmount.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal transferAmount) &&
                decimal.TryParse(txtEditExchangeRate.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal exchangeRate))
            {
                txtEditConvertedAmount.Text = (transferAmount * exchangeRate).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
            }
        }

        protected void txtEditExchangeRate_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtEditTransferAmount.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal transferAmount) &&
                decimal.TryParse(txtEditExchangeRate.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal exchangeRate))
            {
                txtEditConvertedAmount.Text = (transferAmount * exchangeRate).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
            }
        }

        protected void txtEditConvertedAmount_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtEditConvertedAmount.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal convertedAmount) &&
                decimal.TryParse(txtEditExchangeRate.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal exchangeRate) &&
                exchangeRate != 0)
            {
                txtEditTransferAmount.Text = (convertedAmount / exchangeRate).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
            }
        }

        

        #endregion

        #region Yardımcı Metotlar

        protected void ddlTransactionBankAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedBankAccountID = ddlTransactionBankAccount.SelectedValue;
            if (!string.IsNullOrEmpty(selectedBankAccountID))
            {
                decimal balance = GetBankAccountBalance(selectedBankAccountID);
                lblBankBalance.Text = $"Bakiye: {balance:N2} {GetCurrencySymbol(selectedBankAccountID)}";
                lblBankBalance.Visible = true;
            }
            else
            {
                lblBankBalance.Text = string.Empty;
                lblBankBalance.Visible = false;
            }
        }

        private decimal GetBankAccountBalance(string bankAccountID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        COALESCE(SUM(CASE WHEN bhk.HareketTuru = 'Tahsilat' AND bhk.SoftDelete = 0 THEN bhk.Tutar ELSE 0 END), 0)
                      - COALESCE(SUM(CASE WHEN bhk.HareketTuru = 'Ödeme' AND bhk.SoftDelete = 0 THEN bhk.Tutar ELSE 0 END), 0) AS Bakiye
                    FROM BankaHareketleri bhk
                    WHERE bhk.BankaHesapID = @BankaHesapID AND bhk.SoftDelete = 0";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaHesapID", bankAccountID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? Convert.ToDecimal(result) : 0;
            }
        }
        private static decimal GetSatisKurFromDB(string currency)
        {
            // "1 currency = ? USD" dönen metot
            string connStr = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT TOP 1 SatisKur 
            FROM DovizKurlari
            WHERE DovizID = (SELECT DovizID FROM Dovizler WHERE DovizSembol = @Currency)
            ORDER BY KurTarihi DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Currency", currency);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? Convert.ToDecimal(result) : 1m;
            }
        }

        [WebMethod]
        public static decimal GetExchangeRate(string sourceCurrency, string targetCurrency)
        {
            // Aynı döviz ise oran 1
            if (sourceCurrency == targetCurrency)
                return 1;

            // 1 source = x USD
            decimal sourceRate = GetSatisKurFromDB(sourceCurrency);
            // 1 target = y USD
            decimal targetRate = GetSatisKurFromDB(targetCurrency);

            // 1 source = (sourceRate / targetRate) target
            return targetRate / sourceRate;
        }

        private static decimal GetLatestExchangeRate(string targetCurrency)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT TOP 1 SatisKur 
            FROM DovizKurlari 
            WHERE DovizID = (SELECT DovizID FROM Dovizler WHERE DovizSembol = @TargetCurrency)
            ORDER BY KurTarihi DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TargetCurrency", targetCurrency);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? Convert.ToDecimal(result) : 1;
            }
        }

        protected void txtConvertedAmount_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtConvertedAmount.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal convertedAmount) &&
                decimal.TryParse(txtExchangeRate.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal exchangeRate) &&
                exchangeRate != 0)
            {
                txtTransferAmount.Text = (convertedAmount / exchangeRate).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
            }
        }

        protected void txtTransferAmount_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtTransferAmount.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal transferAmount) &&
                decimal.TryParse(txtExchangeRate.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal exchangeRate))
            {
                txtConvertedAmount.Text = (transferAmount * exchangeRate).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
            }
        }

        protected void txtExchangeRate_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtTransferAmount.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal transferAmount) &&
                decimal.TryParse(txtExchangeRate.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal exchangeRate))
            {
                txtConvertedAmount.Text = (transferAmount * exchangeRate).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
            }
        }

        private string GetCurrencySymbol(string bankAccountID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT d.DovizSembol
                    FROM BankaHesaplari b
                    LEFT JOIN Dovizler d ON b.DovizID = d.DovizID
                    WHERE b.BankaHesapID = @BankAccountID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankAccountID", bankAccountID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? result.ToString() : "";
            }
        }

        private string GenerateNextBankTransactionNumber()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    IF NOT EXISTS (SELECT 1 FROM BankaHareketNumaralari WHERE Yil = YEAR(GETDATE()))
                    BEGIN
                        INSERT INTO BankaHareketNumaralari (Yil, SonNumara) VALUES (YEAR(GETDATE()), 0);
                    END
                    ELSE
                    BEGIN
                        UPDATE BankaHareketNumaralari
                        SET SonNumara = SonNumara + 1
                        WHERE Yil = YEAR(GETDATE());
                    END;

                    SELECT CONCAT(
                        'BH-', 
                        FORMAT(GETDATE(), 'yyyyMMdd'), 
                        '-', 
                        FORMAT(SonNumara, '0000')
                    ) AS HareketNo
                    FROM BankaHareketNumaralari WHERE Yil = YEAR(GETDATE());";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "HATA";
            }
        }

        private Guid GetCurrencyIDByBankAccount(string bankAccountID)
        {
            if (string.IsNullOrEmpty(bankAccountID))
                return Guid.Empty;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT DovizID
                    FROM BankaHesaplari
                    WHERE BankaHesapID = @BankAccountID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankAccountID", Guid.Parse(bankAccountID));
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? Guid.Parse(result.ToString()) : Guid.Empty;
            }
        }

        [WebMethod]
        public static dynamic GetCurrencySymbols(string sourceBankAccountID, string targetBankAccountID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        (SELECT d.DovizSembol FROM BankaHesaplari b LEFT JOIN Dovizler d ON b.DovizID = d.DovizID WHERE b.BankaHesapID = @SourceBankAccountID) AS SourceCurrency,
                        (SELECT d.DovizSembol FROM BankaHesaplari b LEFT JOIN Dovizler d ON b.DovizID = d.DovizID WHERE b.BankaHesapID = @TargetBankAccountID) AS TargetCurrency";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SourceBankAccountID", sourceBankAccountID);
                cmd.Parameters.AddWithValue("@TargetBankAccountID", targetBankAccountID);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new
                        {
                            sourceCurrency = reader["SourceCurrency"].ToString(),
                            targetCurrency = reader["TargetCurrency"].ToString()
                        };
                    }
                }
            }
            return null;
        }

         // Türkçe formatlı sayıyı decimal'a çevirir (örneğin "1.234,56" -> 1234.56)
        private decimal ParseTurkishDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            // Binlik ayırıcıları kaldır, ondalık ayırıcıyı nokta yap
            value = value.Replace(".", "").Replace(",", ".");
            decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out decimal result);
            return result;
        }

        // Müşterileri doldurur
        private void LoadCustomers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CariID, CariAdi FROM Cariler WHERE SoftDelete = 0 ORDER BY CariAdi ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlTransactionCustomer.DataSource = dt;
                ddlTransactionCustomer.DataTextField = "CariAdi";
                ddlTransactionCustomer.DataValueField = "CariID";
                ddlTransactionCustomer.DataBind();
                ddlTransactionCustomer.Items.Insert(0, new ListItem("Müşteri Seçin", ""));

                ddlEditTransactionCustomer.DataSource = dt;
                ddlEditTransactionCustomer.DataTextField = "CariAdi";
                ddlEditTransactionCustomer.DataValueField = "CariID";
                ddlEditTransactionCustomer.DataBind();
                ddlEditTransactionCustomer.Items.Insert(0, new ListItem("Müşteri Seçin", ""));
            }
        }

        // Belirtilen banka (BankaID) için banka hesaplarını doldurur
        private void LoadBankAccounts(string bankaID, DropDownList ddlBankAccount)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT BankaHesapID, HesapAdi + ' - ' + HesapNo AS HesapBilgi
            FROM BankaHesaplari 
            WHERE BankaID = @BankaID AND SoftDelete = 0 
            ORDER BY HesapAdi";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaID", bankaID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlBankAccount.DataSource = dt;
                ddlBankAccount.DataTextField = "HesapBilgi";
                ddlBankAccount.DataValueField = "BankaHesapID";
                ddlBankAccount.DataBind();
                ddlBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            }
        }

        // Verilen banka ID'sine göre banka adını getirir
        private string GetBankAccountName(string bankID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT BankaAdi FROM Bankalar WHERE BankaID = @BankaID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaID", Guid.Parse(bankID));
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? result.ToString() : "Bilinmeyen Banka";
            }
        }
        // Banka Hesabı filtresi değiştiğinde işlem listesini yeniler.
        protected void ddlBankAccountFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        // Cari filtresi değiştiğinde işlem listesini yeniler.
        protected void ddlCustomerFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        // Yıl filtresi değiştiğinde işlem listesini yeniler.
        protected void ddlYearFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        // Ay filtresi değiştiğinde işlem listesini yeniler.
        protected void ddlMonthFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTransactions();
        }
        protected void rptBankTransactions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                string hareketID = e.CommandArgument.ToString();
                LoadTransactionForEdit(hareketID);
                // Edit Transaction Modal'ını açan JS fonksiyonunu tetikleyelim
                // ScriptManager.RegisterStartupScript(this, GetType(), "OpenEditTransactionModal", "showEditTransactionModal();", true);
            }
            else if (e.CommandName == "Delete")
            {
                string hareketID = e.CommandArgument.ToString();
                DeleteTransaction(hareketID);
            }
            else if (e.CommandName == "EditTransfer")
            {
                string transferID = e.CommandArgument.ToString();
                LoadTransferForEdit(transferID);
                // Edit Transfer Modal'ını açan JS fonksiyonunu tetikleyelim
                ScriptManager.RegisterStartupScript(this, GetType(), "OpenEditTransferModal", "showEditTransferModal();", true);
            }
            else if (e.CommandName == "DeleteTransfer")
            {
                string transferID = e.CommandArgument.ToString();
                DeleteTransfer(transferID);
            }
            else if (e.CommandName == "ShowDetails")
            {
                string hareketID = e.CommandArgument.ToString();
                LoadTransactionDetailsInModal(hareketID);
            }
        }
        // Transfer düzenleme modalı için veritabanından transfer detaylarını yükler
        private void LoadTransferBanksForEdit()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT BankaID, BankaAdi FROM Bankalar WHERE SoftDelete = 0 ORDER BY BankaAdi ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Kaynak banka dropdown'unu doldur
                ddlEditSourceBank.DataSource = dt;
                ddlEditSourceBank.DataTextField = "BankaAdi";
                ddlEditSourceBank.DataValueField = "BankaID";
                ddlEditSourceBank.DataBind();
                ddlEditSourceBank.Items.Insert(0, new ListItem("Banka Seçiniz", ""));

                // Hedef banka dropdown'unu doldur
                ddlEditTargetBank.DataSource = dt;
                ddlEditTargetBank.DataTextField = "BankaAdi";
                ddlEditTargetBank.DataValueField = "BankaID";
                ddlEditTargetBank.DataBind();
                ddlEditTargetBank.Items.Insert(0, new ListItem("Banka Seçiniz", ""));
            }
        }

        private void LoadTransactionDetailsInModal(string hareketID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT 
                bh.HareketID,
                bh.HareketNo,
                bh.IslemTarihi,
                bh.Tutar,
                bh.HareketTuru,
                bh.Aciklama,
                bh.TransferID,
                bh.TransferYonu,
                bh.OlusturanKullaniciID,
                bh.SonGuncelleyenKullaniciID,
                bh.OlusturmaTarihi,
                bh.GuncellemeTarihi,
                c.CariAdi,
                bnk.BankaAdi,
                bha.BankaHesapID,  -- Bu alanı ekledik
                k1.KullaniciAdi AS OlusturanKullaniciAdi,
                k2.KullaniciAdi AS SonGuncelleyenKullaniciAdi
            FROM BankaHareketleri bh
            LEFT JOIN Cariler c ON bh.CariID = c.CariID
            LEFT JOIN BankaHesaplari bha ON bh.BankaHesapID = bha.BankaHesapID
            LEFT JOIN Bankalar bnk ON bha.BankaID = bnk.BankaID
            LEFT JOIN Kullanicilar k1 ON bh.OlusturanKullaniciID = k1.KullaniciID
            LEFT JOIN Kullanicilar k2 ON bh.SonGuncelleyenKullaniciID = k2.KullaniciID
            WHERE bh.HareketID = @HareketID AND bh.SoftDelete = 0;
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HareketID", hareketID);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblDetailHareketNo.Text = reader["HareketNo"].ToString();
                        lblDetailTarih.Text = Convert.ToDateTime(reader["IslemTarihi"]).ToString("dd.MM.yyyy HH:mm");

                        // Tutarı alıp döviz sembolüyle birlikte gösteriyoruz:
                        decimal tutar = Convert.ToDecimal(reader["Tutar"]);
                        string bankHesapID = reader["BankaHesapID"].ToString();
                        string currencySymbol = GetCurrencySymbol(bankHesapID);
                        lblDetailTutar.Text = tutar.ToString("N2") + " " + currencySymbol;

                        lblDetailHareketTuru.Text = reader["HareketTuru"].ToString();
                        lblDetailAciklama.Text = reader["Aciklama"].ToString();

                        // Join üzerinden gelen veriler:
                        lblDetailBanka.Text = reader["BankaAdi"].ToString();
                        lblDetailCariAdi.Text = reader["CariAdi"].ToString();
                        lblDetailOlusturanKullaniciID.Text = reader["OlusturanKullaniciAdi"].ToString();
                        lblDetailSonGuncelleyenKullaniciID.Text = reader["SonGuncelleyenKullaniciAdi"].ToString();
                        // İstediğiniz diğer alanları da ekleyebilirsiniz.
                    }
                }
            }

            // Modalı açan JS kodunu tetikliyoruz:
            ScriptManager.RegisterStartupScript(this, GetType(), "openDetailModal", "showDetailModal();", true);
        }
        private void LoadTransferForEdit(string transferID)
        {
            try
            {
                // Önce edit için banka dropdown’larını dolduralım.
                LoadTransferBanksForEdit();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT bh.*, b.BankaID
                FROM BankaHareketleri bh
                INNER JOIN BankaHesaplari bha ON bh.BankaHesapID = bha.BankaHesapID
                INNER JOIN Bankalar b ON bha.BankaID = b.BankaID
                WHERE bh.TransferID = @TransferID AND bh.SoftDelete = 0";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@TransferID", transferID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count < 2)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "error",
                            "Swal.fire('Hata!', 'Transfer kaydı bulunamadı.', 'error');", true);
                        return;
                    }

                    DataRow sourceRow = null;
                    DataRow targetRow = null;

                    foreach (DataRow row in dt.Rows)
                    {
                        string hareketTuru = row["HareketTuru"].ToString().Trim();
                        if (hareketTuru.Equals("Ödeme", StringComparison.OrdinalIgnoreCase))
                            sourceRow = row;
                        else if (hareketTuru.Equals("Tahsilat", StringComparison.OrdinalIgnoreCase))
                            targetRow = row;
                    }

                    if (sourceRow == null || targetRow == null)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "error",
                            "Swal.fire('Hata!', 'Transfer kayıtları eksik.', 'error');", true);
                        return;
                    }

                    // Kaynak (Ödeme - OUT) işlemin bilgileri:
                    string sourceBankAccountID = sourceRow["BankaHesapID"].ToString();
                    decimal sourceTutar = Convert.ToDecimal(sourceRow["Tutar"]);
                    DateTime islemTarihi = Convert.ToDateTime(sourceRow["IslemTarihi"]);
                    string sourceAciklama = sourceRow["Aciklama"].ToString();
                    string sourceBankaID = sourceRow["BankaID"].ToString();

                    // Hedef (Tahsilat - IN) işlemin bilgileri:
                    string targetBankAccountID = targetRow["BankaHesapID"].ToString();
                    decimal targetTutar = Convert.ToDecimal(targetRow["Tutar"]);
                    string targetAciklama = targetRow["Aciklama"].ToString();
                    string targetBankaID = targetRow["BankaID"].ToString();

                    // Kaynak banka dropdown'unda seçili yap ve hesaplarını yükle
                    if (!string.IsNullOrEmpty(sourceBankaID))
                    {
                        if (ddlEditSourceBank.Items.FindByValue(sourceBankaID) != null)
                            ddlEditSourceBank.SelectedValue = sourceBankaID;
                        LoadTransferBankAccounts(sourceBankaID, ddlEditSourceBankAccount);
                        if (ddlEditSourceBankAccount.Items.FindByValue(sourceBankAccountID) != null)
                            ddlEditSourceBankAccount.SelectedValue = sourceBankAccountID;
                    }

                    // Hedef banka dropdown'unda seçili yap ve hesaplarını yükle
                    if (!string.IsNullOrEmpty(targetBankaID))
                    {
                        if (ddlEditTargetBank.Items.FindByValue(targetBankaID) != null)
                            ddlEditTargetBank.SelectedValue = targetBankaID;
                        LoadTransferBankAccounts(targetBankaID, ddlEditTargetBankAccount);
                        if (ddlEditTargetBankAccount.Items.FindByValue(targetBankAccountID) != null)
                            ddlEditTargetBankAccount.SelectedValue = targetBankAccountID;
                    }

                    // Diğer bilgileri dolduruyoruz:
                    hfEditTransferID.Value = transferID;
                    txtEditTransferDate.Text = islemTarihi.ToString("yyyy-MM-dd");
                    txtEditTransferAmount.Text = sourceTutar.ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
                    txtEditConvertedAmount.Text = targetTutar.ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
                    decimal kur = (sourceTutar != 0) ? targetTutar / sourceTutar : 1;
                    txtEditExchangeRate.Text = kur.ToString("N4");

                    // Açıklamaları birleştirip tek satır haline getiriyoruz:
                    string birlesikAciklama = $"{sourceAciklama} | {targetAciklama}";
                    txtEditTransferDescription.Text = birlesikAciklama;

                    UpdatePanelEditTransfer.Update();
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "editTransferModal",
                    "setTimeout(function() { showEditTransferModal(); }, 100);", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    $"Swal.fire('Hata!', 'İşlem sırasında bir hata oluştu: {ex.Message}', 'error');", true);
            }
        }



        // Banka hareketi düzenleme modalı için veritabanından işlem detaylarını yükler
        private void LoadTransactionForEdit(string hareketID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT HareketID, BankaHesapID, IslemTarihi, Tutar, HareketTuru, Aciklama, CariID
            FROM BankaHareketleri
            WHERE HareketID = @HareketID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HareketID", hareketID);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hfEditTransactionID.Value = reader["HareketID"].ToString();
                        txtEditTransactionDate.Text = Convert.ToDateTime(reader["IslemTarihi"]).ToString("yyyy-MM-dd");
                        txtEditTransactionAmount.Text = Convert.ToDecimal(reader["Tutar"]).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
                        txtEditTransactionDescription.Text = reader["Aciklama"].ToString();
                        if (reader["CariID"] != DBNull.Value)
                        {
                            ddlEditTransactionCustomer.SelectedValue = reader["CariID"].ToString();
                        }
                        ddlEditTransactionType.SelectedValue = reader["HareketTuru"].ToString();

                        // Banka hesabı bilgilerini doldurmak için:
                        string bankAccountID = reader["BankaHesapID"].ToString();
                        string bankID = GetBankIDByBankAccountID(bankAccountID);
                        if (ddlEditTransactionBank.Items.FindByValue(bankID) != null)
                        {
                            ddlEditTransactionBank.SelectedValue = bankID;
                            LoadBankAccounts(bankID, ddlEditTransactionBankAccount);
                        }
                        if (ddlEditTransactionBankAccount.Items.FindByValue(bankAccountID) != null)
                        {
                            ddlEditTransactionBankAccount.SelectedValue = bankAccountID;
                        }
                    }
                }
            }
            // Modalı açan JavaScript çağrısını ekliyoruz:
            ScriptManager.RegisterStartupScript(this, GetType(), "editTransactionModal", "showEditTransactionModal();", true);


        }

        // Transfer silme işlemini gerçekleştirir
        private void DeleteTransfer(string transferID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string deleteQuery = "UPDATE BankaHareketleri SET SoftDelete = 1 WHERE TransferID = @TransferID";
                    SqlCommand cmd = new SqlCommand(deleteQuery, conn, transaction);
                    cmd.Parameters.AddWithValue("@TransferID", transferID);
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Transfer silme işlemi sırasında hata oluştu: " + ex.Message);
                }
            }
            LoadTransactions();
            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Başarılı!', 'Transfer başarıyla silindi.', 'success');", true);
        }
        // Verilen kaynak ve hedef döviz ID'sine göre, hedef dövizin satış kurunu veritabanından çeker.
        // Eğer döviz kurları mevcutsa, hedef döviz kuru değerini döndürür; aksi halde 1 döndürür.
        private decimal FetchExchangeRateFromDB(Guid sourceCurrencyID, Guid targetCurrencyID)
        {
            decimal exchangeRate = 1;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT TOP 1 SatisKur 
            FROM DovizKurlari 
            WHERE DovizID = @TargetCurrencyID 
            ORDER BY KurTarihi DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TargetCurrencyID", targetCurrencyID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                if (result != null)
                {
                    exchangeRate = Convert.ToDecimal(result);
                }
            }
            return exchangeRate;
        }

        // Verilen banka hesap ID'sine göre, ilgili bankanın ID'sini döndürür.
        // Eğer banka hesabı bulunamazsa boş string döndürür.
        private string GetBankIDByBankAccountID(string bankAccountID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT BankaID FROM BankaHesaplari WHERE BankaHesapID = @BankAccountID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankAccountID", bankAccountID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? result.ToString() : string.Empty;
            }
        }

        protected void ddlTargetBankAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sourceBankAccountID = ddlSourceBankAccount.SelectedValue;
            string targetBankAccountID = ddlTargetBankAccount.SelectedValue;

            // Döviz kuru kontrolü ve hesap bakiyesini güncelle
            checkExchangeRate(sourceBankAccountID, targetBankAccountID, txtExchangeRate, txtConvertedAmount, txtTransferAmount);

            if (!string.IsNullOrEmpty(targetBankAccountID))
            {
                decimal balance = GetBankAccountBalance(targetBankAccountID);
                lblTargetBankBalance.Text = $"Bakiye: {balance:N2} {GetCurrencySymbol(targetBankAccountID)}";
                lblTargetBankBalance.Visible = true;
            }
            else
            {
                lblTargetBankBalance.Text = "";
                lblTargetBankBalance.Visible = false;
            }
        }
        // Add Transaction (Yeni Banka İşlemi) modalındaki kontrolleri temizler.
        private void ClearAddTransactionModal()
        {
            // Banka ve banka hesabı dropdown'larını sıfırla.
            ddlTransactionBank.ClearSelection();
            ddlTransactionBankAccount.Items.Clear();
            ddlTransactionBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));

            // İşlem türü ve müşteri seçimini sıfırla.
            ddlTransactionType.ClearSelection();
            ddlTransactionCustomer.ClearSelection();

            // Tarih, tutar ve açıklama textbox'larını temizle.
            txtTransactionDate.Text = "";
            txtTransactionAmount.Text = "";
            txtTransactionDescription.Text = "";

            // Banka bakiyesi label'ını temizle ve gizle.
            lblBankBalance.Text = "";
            lblBankBalance.Visible = false;
        }

        // Transfer modalındaki kontrolleri temizler.
        private void ClearTransferModal()
        {
            // Kaynak ve hedef banka dropdown'larını sıfırla.
            ddlSourceBank.ClearSelection();
            ddlSourceBankAccount.Items.Clear();
            ddlSourceBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));

            ddlTargetBank.ClearSelection();
            ddlTargetBankAccount.Items.Clear();
            ddlTargetBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));

            // Transfer tarihini bugünün tarihine ayarla.
            txtTransferDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

            // Transfer tutar, kur ve çevrilmiş tutar textbox'larını temizle.
            txtTransferAmount.Text = "";
            txtExchangeRate.Text = "";
            txtConvertedAmount.Text = "";

            // Transfer açıklamasını temizle.
            txtTransferDescription.Text = "";

            // Kaynak ve hedef banka bakiyesi label'larını temizle ve gizle.
            lblSourceBankBalance.Text = "";
            lblSourceBankBalance.Visible = false;
            lblTargetBankBalance.Text = "";
            lblTargetBankBalance.Visible = false;
        }

        // Edit Transaction (Banka Hareketi Düzenleme) modalındaki kontrolleri temizler.
        private void ClearEditTransactionModal()
        {
            hfEditTransactionID.Value = "";
            ddlEditTransactionBank.ClearSelection();
            ddlEditTransactionBankAccount.Items.Clear();
            ddlEditTransactionBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            ddlEditTransactionType.ClearSelection();
            ddlEditTransactionCustomer.ClearSelection();
            txtEditTransactionDate.Text = "";
            txtEditTransactionAmount.Text = "";
            txtEditTransactionDescription.Text = "";
        }

        // Edit Transfer (Transfer Düzenleme) modalındaki kontrolleri temizler.
        private void ClearEditTransferModal()
        {
            hfEditTransferID.Value = "";
            ddlEditSourceBank.ClearSelection();
            ddlEditSourceBankAccount.Items.Clear();
            ddlEditSourceBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            ddlEditTargetBank.ClearSelection();
            ddlEditTargetBankAccount.Items.Clear();
            ddlEditTargetBankAccount.Items.Insert(0, new ListItem("Hesap Seçiniz", ""));
            txtEditTransferDate.Text = "";
            txtEditTransferAmount.Text = "";
            txtEditExchangeRate.Text = "";
            txtEditConvertedAmount.Text = "";
            txtEditTransferDescription.Text = "";
            
        }
        #endregion
    }
}