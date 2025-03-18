using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class BankAccounts : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBanks();      // Bankaları yükle
                LoadBankList();   // Banka listesi (Dropdown için)
                LoadCurrencies(); // Döviz türlerini yükle
            }

            // Silme işlemi için __doPostBack çağrısını yakala
            string eventTarget = Request["__EVENTTARGET"];
            if (eventTarget == "DeleteBank")
            {
                DeleteBank();
            }
            else if (eventTarget == "DeleteBankAccount")
            {
                DeleteBankAccount();
            }

        }

        protected void lnkBankTransactions_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string bankaHesapID = btn.CommandArgument;

            // Seçilen banka ID'sini Session'da sakla
            Session["SelectedBankID"] = bankaHesapID;

            // BankTransactions.aspx sayfasına yönlendir
            Response.Redirect("BankTransactions.aspx");
        }
        /// <summary>
        /// Bankaları listeler
        /// </summary>
        private void LoadBanks()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT BankaID, BankaAdi FROM Bankalar WHERE SoftDelete = 0 ORDER BY BankaAdi ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptBanks.DataSource = dt;
                rptBanks.DataBind();
            }
        }

        /// <summary>
        /// DropdownList için banka listesini yükler
        /// </summary>
        private void LoadBankList()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT BankaID, BankaAdi FROM Bankalar WHERE SoftDelete = 0 ORDER BY BankaAdi ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlSelectBank.DataSource = dt;
                ddlSelectBank.DataTextField = "BankaAdi";
                ddlSelectBank.DataValueField = "BankaID";
                ddlSelectBank.DataBind();
                ddlSelectBank.Items.Insert(0, new ListItem("Banka Seçin", ""));
            }
        }

        /// <summary>
        /// Döviz türlerini yükler
        /// </summary>
        protected void LoadCurrencies()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // DovizSembol boş olmayan kayıtları getir
                string query = "SELECT DovizID, DovizSembol FROM Dovizler WHERE DovizSembol IS NOT NULL AND DovizSembol <> '' ORDER BY DovizSembol ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (ddlEditCurrency != null)
                {
                    ddlEditCurrency.DataSource = dt;
                    ddlEditCurrency.DataTextField = "DovizSembol";
                    ddlEditCurrency.DataValueField = "DovizID";
                    ddlEditCurrency.DataBind();
                    ddlEditCurrency.Items.Insert(0, new ListItem("Döviz Seçin", ""));
                }
            }
        }

        /// <summary>
        /// Repeater içinde banka hesaplarını yükler
        /// </summary>
        protected void rptBanks_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string bankaID = DataBinder.Eval(e.Item.DataItem, "BankaID").ToString();
                Repeater rptBankAccounts = (Repeater)e.Item.FindControl("rptBankAccounts");

                if (rptBankAccounts != null)
                {
                    LoadBankAccounts(rptBankAccounts, bankaID);
                }
            }
        }

        /// <summary>
        /// Banka hesaplarını ve bakiyelerini yükler
        /// </summary>
        private void LoadBankAccounts(Repeater rptBankAccounts, string bankID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT bh.BankaHesapID, bh.HesapAdi, bh.IBAN, bh.HesapNo, bh.SubeAdi, d.DovizSembol, d.DovizID,
            COALESCE(SUM(CASE WHEN bhk.HareketTuru = 'Tahsilat' AND bhk.SoftDelete = 0 THEN bhk.Tutar ELSE 0 END), 0)
          - COALESCE(SUM(CASE WHEN bhk.HareketTuru = 'Ödeme' AND bhk.SoftDelete = 0 THEN bhk.Tutar ELSE 0 END), 0) AS Bakiye
        FROM BankaHesaplari bh
        LEFT JOIN Dovizler d ON bh.DovizID = d.DovizID
        LEFT JOIN BankaHareketleri bhk ON bh.BankaHesapID = bhk.BankaHesapID AND bhk.SoftDelete = 0
        WHERE bh.SoftDelete = 0 AND bh.BankaID = @BankaID
        GROUP BY bh.BankaHesapID, bh.HesapAdi, bh.IBAN, bh.HesapNo, bh.SubeAdi, d.DovizSembol, d.DovizID
        ORDER BY bh.HesapAdi ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaID", bankID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (rptBankAccounts != null)
                {
                    rptBankAccounts.DataSource = dt;
                    rptBankAccounts.DataBind();
                }
            }
        }
        /// <summary>
        /// Yeni banka ekler
        /// </summary>

        protected void LoadEditBankAccountModal(string bankAccountID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT BankaHesapID, HesapNo, HesapAdi, IBAN, SubeAdi, DovizID 
        FROM BankaHesaplari 
        WHERE BankaHesapID = @BankaHesapID AND SoftDelete = 0";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaHesapID", bankAccountID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    hfEditBankAccountID.Value = row["BankaHesapID"].ToString();
                    txtEditAccountNumber.Text = row["HesapNo"].ToString();
                    txtEditAccountName.Text = row["HesapAdi"].ToString();
                    txtEditIBAN.Text = row["IBAN"].ToString();
                    txtEditBranch.Text = row["SubeAdi"].ToString();

                    // Döviz DropDownList'i yükle
                    LoadCurrencies();
                    ddlEditCurrency.SelectedValue = row["DovizID"].ToString();
                }
            }

            // JavaScript ile modalı aç
            ScriptManager.RegisterStartupScript(this, GetType(), "editModal",
                "$('#editBankAccountModal').modal('show');", true);
        }
        protected void btnSaveBank_Click(object sender, EventArgs e)
        {
            string bankName = txtBankName.Text.Trim();

            if (string.IsNullOrEmpty(bankName))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error", "Swal.fire('Hata!', 'Lütfen banka adını giriniz.', 'error');", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Bankalar (BankaID, BankaAdi, Aktif, OlusturmaTarihi, SoftDelete) VALUES (NEWID(), @BankaAdi, 1, GETDATE(), 0)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaAdi", bankName);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadBanks();
            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Başarılı!', 'Banka başarıyla eklendi.', 'success').then(() => { $('#addBankModal').modal('hide'); });", true);
        }

        /// <summary>
        /// Yeni banka hesabı ekler
        /// </summary

        protected void rptBanks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "DeleteBank")
            {
                string bankID = e.CommandArgument.ToString();
                DeleteBank();
            }
        }

        protected void rptBankAccounts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "EditBankAccount")
            {
                string bankAccountID = e.CommandArgument.ToString();
                LoadEditBankAccountModal(bankAccountID); // Düzenleme modalını yükle
            }
            else if (e.CommandName == "DeleteBankAccount")
            {
                string bankAccountID = e.CommandArgument.ToString();
                DeleteBankAccount(); // Silme işlemini gerçekleştir
            }
        }

        protected void DeleteBank()
        {
            string bankID = hfDeleteBankID.Value;

            if (!string.IsNullOrEmpty(bankID))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // Bankayı SoftDelete yap
                        string bankDeleteQuery = "UPDATE Bankalar SET SoftDelete = 1 WHERE BankaID = @BankaID";
                        SqlCommand bankCmd = new SqlCommand(bankDeleteQuery, conn, transaction);
                        bankCmd.Parameters.AddWithValue("@BankaID", bankID);
                        bankCmd.ExecuteNonQuery();

                        // Bankaya bağlı hesapları SoftDelete yap
                        string bankAccountsDeleteQuery = "UPDATE BankaHesaplari SET SoftDelete = 1 WHERE BankaID = @BankaID";
                        SqlCommand bankAccountsCmd = new SqlCommand(bankAccountsDeleteQuery, conn, transaction);
                        bankAccountsCmd.Parameters.AddWithValue("@BankaID", bankID);
                        bankAccountsCmd.ExecuteNonQuery();

                        // İşlemleri onayla
                        transaction.Commit();

                        LoadBanks();

                        // Silme başarılı mesajı göster
                        ScriptManager.RegisterStartupScript(this, GetType(), "success",
                            "Swal.fire('Başarılı!', 'Banka ve bağlı hesaplar başarıyla silindi.', 'success');", true);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        // Hata mesajı göster
                        ScriptManager.RegisterStartupScript(this, GetType(), "error",
                            $"Swal.fire('Hata!', 'Silme işlemi başarısız: {ex.Message}', 'error');", true);
                    }
                }
            }
        }

        protected void DeleteBankAccount()
        {
            string bankAccountID = hfDeleteBankAccountID.Value;

            if (!string.IsNullOrEmpty(bankAccountID))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE BankaHesaplari SET SoftDelete = 1 WHERE BankaHesapID = @BankaHesapID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@BankaHesapID", bankAccountID);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                LoadBanks(); // Listeyi güncelle

                // Silme başarılı mesajı göster
                ScriptManager.RegisterStartupScript(this, GetType(), "success",
                    "Swal.fire('Başarılı!', 'Banka hesabı başarıyla silindi.', 'success');", true);
            }
        }
        protected void btnEditBankAccount_Click(object sender, EventArgs e)
        {
            // Düzenleme işlemi için gerekli veriyi modal içerisine doldurma işlemi
            LinkButton btn = (LinkButton)sender;
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;

            HiddenField hfBankAccountID = (HiddenField)item.FindControl("hfEditBankAccountID");
            TextBox txtEditAccountNumber = (TextBox)item.FindControl("txtEditAccountNumber");
            TextBox txtEditAccountName = (TextBox)item.FindControl("txtEditAccountName");
            TextBox txtEditIBAN = (TextBox)item.FindControl("txtEditIBAN");
            TextBox txtEditBranch = (TextBox)item.FindControl("txtEditBranch");
            DropDownList ddlEditCurrency = (DropDownList)item.FindControl("ddlEditCurrency");

            if (hfBankAccountID != null)
            {
                string bankAccountID = hfBankAccountID.Value;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT HesapNo, HesapAdi, IBAN, SubeAdi, DovizID 
                             FROM BankaHesaplari 
                             WHERE BankaHesapID = @BankaHesapID";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@BankaHesapID", bankAccountID);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtEditAccountNumber.Text = reader["HesapNo"].ToString();
                        txtEditAccountName.Text = reader["HesapAdi"].ToString();
                        txtEditIBAN.Text = reader["IBAN"].ToString();
                        txtEditBranch.Text = reader["SubeAdi"].ToString();
                        ddlEditCurrency.SelectedValue = reader["DovizID"].ToString();
                    }
                    reader.Close();
                }

                // JavaScript ile modalı aç
                ScriptManager.RegisterStartupScript(this, GetType(), "showEditBankAccountModal",
                    "$('#editBankAccountModal').modal('show');", true);
            }
        }
        protected void btnUpdateBankAccount_Click(object sender, EventArgs e)
        {
            string bankAccountID = hfEditBankAccountID.Value;
            string accountNumber = txtEditAccountNumber.Text.Trim();
            string accountName = txtEditAccountName.Text.Trim();
            string iban = txtEditIBAN.Text.Trim();
            string branchName = txtEditBranch.Text.Trim();
            string currencyID = ddlEditCurrency.SelectedValue;

            if (string.IsNullOrEmpty(bankAccountID) || string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(currencyID))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Hata!', 'Lütfen zorunlu alanları doldurun.', 'error');", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            UPDATE BankaHesaplari 
            SET HesapNo = @HesapNo, HesapAdi = @HesapAdi, IBAN = @IBAN, 
                SubeAdi = @SubeAdi, DovizID = @DovizID, GuncellemeTarihi = GETDATE() 
            WHERE BankaHesapID = @BankaHesapID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaHesapID", bankAccountID);
                cmd.Parameters.AddWithValue("@HesapNo", accountNumber);
                cmd.Parameters.AddWithValue("@HesapAdi", accountName);
                cmd.Parameters.AddWithValue("@IBAN", iban);
                cmd.Parameters.AddWithValue("@SubeAdi", branchName);
                cmd.Parameters.AddWithValue("@DovizID", currencyID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadBanks();
            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "Swal.fire('Başarılı!', 'Banka hesabı güncellendi.', 'success').then(() => { $('#editBankAccountModal').modal('hide'); });", true);
        }
        protected void btnSaveBankAccount_Click(object sender, EventArgs e)
        {
            string bankID = ddlSelectBank.SelectedValue;
            string accountNumber = txtAccountNumber.Text.Trim();
            string accountName = txtAccountName.Text.Trim();
            string iban = txtIBAN.Text.Trim();
            string branchName = txtBranch.Text.Trim();
            string currencyID = ddlCurrency.SelectedValue;

            if (string.IsNullOrEmpty(bankID) || string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(currencyID))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Hata!', 'Lütfen zorunlu alanları doldurun.', 'error');", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        INSERT INTO BankaHesaplari (BankaHesapID, BankaID, HesapNo, HesapAdi, IBAN, SubeAdi, DovizID, Aktif, SoftDelete, OlusturmaTarihi) 
        VALUES (NEWID(), @BankaID, @HesapNo, @HesapAdi, @IBAN, @SubeAdi, @DovizID, 1, 0, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaID", bankID);
                cmd.Parameters.AddWithValue("@HesapNo", accountNumber ?? "000000"); // NULL OLMAMASI İÇİN
                cmd.Parameters.AddWithValue("@HesapAdi", string.IsNullOrEmpty(accountName) ? DBNull.Value : (object)accountName);
                cmd.Parameters.AddWithValue("@IBAN", string.IsNullOrEmpty(iban) ? DBNull.Value : (object)iban);
                cmd.Parameters.AddWithValue("@SubeAdi", string.IsNullOrEmpty(branchName) ? DBNull.Value : (object)branchName);
                cmd.Parameters.AddWithValue("@DovizID", currencyID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadBanks();

            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "Swal.fire('Başarılı!', 'Banka hesabı başarıyla eklendi.', 'success').then(() => { $('#addBankAccountModal').modal('hide'); });", true);
        }
        protected void btnUpdateBank_Click(object sender, EventArgs e)
        {
            string bankID = hfEditBankID.Value;
            string bankName = txtEditBankName.Text.Trim();

            if (string.IsNullOrEmpty(bankID) || string.IsNullOrEmpty(bankName))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Hata!', 'Lütfen banka adını giriniz.', 'error');", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Bankalar SET BankaAdi = @BankaAdi, GuncellemeTarihi = GETDATE() WHERE BankaID = @BankaID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaID", bankID);
                cmd.Parameters.AddWithValue("@BankaAdi", bankName);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadBanks();
            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "Swal.fire('Başarılı!', 'Banka bilgileri güncellendi.', 'success').then(() => { $('#editBankModal').modal('hide'); });", true);
        }
        protected void btnDeleteBank_Click(object sender, EventArgs e)
        {
            string bankID = hfDeleteBankID.Value;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Bankalar SET SoftDelete = 1 WHERE BankaID = @BankaID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaID", bankID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadBanks();

            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "Swal.fire('Başarılı!', 'Banka başarıyla silindi.', 'success');", true);
        }
        protected void btnDeleteBankAccount_Click(object sender, EventArgs e)
        {
            string bankAccountID = hfDeleteBankAccountID.Value;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE BankaHesaplari SET SoftDelete = 1 WHERE BankaHesapID = @BankaHesapID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BankaHesapID", bankAccountID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadBanks();

            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "Swal.fire('Başarılı!', 'Banka hesabı başarıyla silindi.', 'success');", true);
        }

    }
}