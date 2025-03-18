using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class CashTransactions : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;
        private string kasaID;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["KasaID"]))
                {
                    kasaID = Request.QueryString["KasaID"];
                }

                LoadCashAccounts();
                LoadDropdowns();
                LoadSourceCashAccounts();
                LoadCustomers();
                LoadCashAccountDropdown();

                if (!string.IsNullOrEmpty(kasaID))
                {
                    ddlFilterCashAccount.SelectedValue = kasaID;
                    LoadTransactionsByCashAccount(kasaID);
                }
                else
                {
                    LoadTransactions();
                }

            }
        }

        private void LoadCashAccountDropdown()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT KasaID, KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim 
        FROM Kasalar k
        LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
        WHERE k.SoftDelete = 0
        ORDER BY k.KasaAdi ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlFilterCashAccount.DataSource = dt;
                ddlFilterCashAccount.DataTextField = "KasaGosterim";
                ddlFilterCashAccount.DataValueField = "KasaID";
                ddlFilterCashAccount.DataBind();

                ddlFilterCashAccount.Items.Insert(0, new ListItem("Tüm Kasalar", ""));
            }
        }

        private bool IsValidGuid(string value)
        {
            Guid guidOutput;
            return Guid.TryParse(value, out guidOutput);
        }

        private void LoadTransactions()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT kh.HareketID, kh.IslemTarihi, kh.Tutar, kh.IslemTuru, kh.Aciklama, kh.HareketNo, 
                       k.KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim, 
                       d.DovizSembol, c.CariAdi, kh.TransferID
                FROM KasaHareketleri kh
                LEFT JOIN Kasalar k ON kh.KasaID = k.KasaID
                LEFT JOIN Cariler c ON kh.CariID = c.CariID
                LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
                WHERE kh.SoftDelete = 0
                ORDER BY kh.IslemTarihi DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptTransactions.DataSource = dt;
                rptTransactions.DataBind();
            }
        }

        private void LoadTransactionsByCashAccount(string kasaID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT kh.HareketID, kh.HareketNo, kh.IslemTarihi, kh.Tutar, kh.IslemTuru, kh.Aciklama, 
                   k.KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim,
                   d.DovizSembol, c.CariAdi, kh.TransferID    
            FROM KasaHareketleri kh
            LEFT JOIN Kasalar k ON kh.KasaID = k.KasaID
            LEFT JOIN Cariler c ON kh.CariID = c.CariID
            LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
            WHERE kh.SoftDelete = 0 AND kh.KasaID = @KasaID
            ORDER BY kh.IslemTarihi DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@KasaID", kasaID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptTransactions.DataSource = dt;
                rptTransactions.DataBind();
            }
        }

        private void LoadTransactionsByDate(DateTime? startDate, DateTime? endDate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT kh.HareketID, kh.HareketNo, kh.IslemTarihi, kh.Tutar, kh.IslemTuru, kh.Aciklama, 
                   k.KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim,
                   d.DovizSembol, c.CariAdi, kh.TransferID    
            FROM KasaHareketleri kh
            LEFT JOIN Kasalar k ON kh.KasaID = k.KasaID
            LEFT JOIN Cariler c ON kh.CariID = c.CariID
            LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
            WHERE kh.SoftDelete = 0";

                if (startDate.HasValue)
                {
                    query += " AND kh.IslemTarihi >= @StartDate";
                }

                if (endDate.HasValue)
                {
                    query += " AND kh.IslemTarihi <= @EndDate";
                }

                query += " ORDER BY kh.IslemTarihi DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                if (startDate.HasValue)
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                }
                if (endDate.HasValue)
                {
                    cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptTransactions.DataSource = dt;
                rptTransactions.DataBind();
            }
        }

        private void LoadTransactionsByCustomer(string cariID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT kh.HareketID, kh.HareketNo, kh.IslemTarihi, kh.Tutar, kh.IslemTuru, kh.Aciklama, 
                   k.KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim,
                   d.DovizSembol, c.CariAdi, kh.TransferID    
            FROM KasaHareketleri kh
            LEFT JOIN Kasalar k ON kh.KasaID = k.KasaID
            LEFT JOIN Cariler c ON kh.CariID = c.CariID
            LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
            WHERE kh.SoftDelete = 0 AND kh.CariID = @CariID
            ORDER BY kh.IslemTarihi DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CariID", cariID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptTransactions.DataSource = dt;
                rptTransactions.DataBind();
            }
        }

        protected void ddlFilterCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCariID = ddlFilterCustomer.SelectedValue;

            if (!string.IsNullOrEmpty(selectedCariID))
            {
                LoadTransactionsByCustomer(selectedCariID);
            }
            else
            {
                LoadTransactions();
            }
        }

        protected void ddlFilterCashAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedKasaID = ddlFilterCashAccount.SelectedValue;

            if (!string.IsNullOrEmpty(selectedKasaID))
            {
                LoadTransactionsByCashAccount(selectedKasaID);
            }
            else
            {
                LoadTransactions();
            }
        }

        private void LoadDropdowns()
        {
            LoadFilterCashAccounts();
            LoadFilterCustomers();
        }

        private void LoadFilterCashAccounts()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT KasaID, KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim 
                FROM Kasalar k
                LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
                WHERE k.SoftDelete = 0
                ORDER BY k.KasaAdi ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlFilterCashAccount.DataSource = dt;
                ddlFilterCashAccount.DataTextField = "KasaGosterim";
                ddlFilterCashAccount.DataValueField = "KasaID";
                ddlFilterCashAccount.DataBind();
                ddlFilterCashAccount.Items.Insert(0, new ListItem("Tüm Kasalar", ""));
            }
        }

        protected void btnFilterByDate_Click(object sender, EventArgs e)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(txtStartDate.Text))
            {
                startDate = Convert.ToDateTime(txtStartDate.Text);
            }
            if (!string.IsNullOrEmpty(txtEndDate.Text))
            {
                endDate = Convert.ToDateTime(txtEndDate.Text);
            }

            LoadTransactionsByDate(startDate, endDate);
        }

        private void DeleteTransaction(string hareketID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "UPDATE KasaHareketleri SET SoftDelete = 1 WHERE HareketID = @HareketID";
                    SqlCommand cmd = new SqlCommand(query, conn, transaction);
                    cmd.Parameters.AddWithValue("@HareketID", hareketID);
                    cmd.ExecuteNonQuery();

                    string cariQuery = "UPDATE CariHareketler SET SoftDelete = 1 WHERE EvrakID = @HareketID";
                    SqlCommand cmdCari = new SqlCommand(cariQuery, conn, transaction);
                    cmdCari.Parameters.AddWithValue("@HareketID", hareketID);
                    cmdCari.ExecuteNonQuery();

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

        private void DeleteTransfer(string transferID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "UPDATE KasaHareketleri SET SoftDelete = 1 WHERE TransferID = @TransferID";
                    SqlCommand cmd = new SqlCommand(query, conn, transaction);
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

        private void LoadSourceCashAccounts()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT KasaID, KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim
            FROM Kasalar k
            LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
            WHERE k.SoftDelete = 0
            ORDER BY k.KasaAdi ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlSourceCashAccount.DataSource = dt;
                ddlSourceCashAccount.DataTextField = "KasaGosterim";
                ddlSourceCashAccount.DataValueField = "KasaID";
                ddlSourceCashAccount.DataBind();

                ddlSourceCashAccount.Items.Insert(0, new ListItem("Kaynak Kasa Seçin", ""));
            }
        }

        protected void ddlSourceCashAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTargetCashAccounts();
        }

        private void LoadTargetCashAccounts()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT KasaID, KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim
            FROM Kasalar k
            LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
            WHERE k.SoftDelete = 0 AND k.KasaID <> @SourceKasaID
            ORDER BY k.KasaAdi ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SourceKasaID", ddlSourceCashAccount.SelectedValue);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlTargetCashAccount.DataSource = dt;
                ddlTargetCashAccount.DataTextField = "KasaGosterim";
                ddlTargetCashAccount.DataValueField = "KasaID";
                ddlTargetCashAccount.DataBind();

                ddlTargetCashAccount.Items.Insert(0, new ListItem("Hedef Kasa Seçin", ""));
            }
        }

        protected void ddlTargetCashAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            decimal exchangeRate = LoadExchangeRate(ddlSourceCashAccount.SelectedValue, ddlTargetCashAccount.SelectedValue);

            if (exchangeRate > 0)
            {
                txtExchangeRate.Text = exchangeRate.ToString("N6");
            }
            else
            {
                txtExchangeRate.Text = "Kur Bulunamadı";
            }
        }

        private decimal LoadExchangeRate(string sourceKasaID, string targetKasaID)
        {
            decimal sourceExchangeRate = 1;
            decimal targetExchangeRate = 1;
            string sourceDoviz = "";
            string targetDoviz = "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT k.KasaID, d.DovizSembol
                FROM Kasalar k
                LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
                WHERE k.KasaID IN (@SourceKasaID, @TargetKasaID)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SourceKasaID", sourceKasaID);
                cmd.Parameters.AddWithValue("@TargetKasaID", targetKasaID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["KasaID"].ToString() == sourceKasaID)
                        sourceDoviz = reader["DovizSembol"].ToString();
                    else if (reader["KasaID"].ToString() == targetKasaID)
                        targetDoviz = reader["DovizSembol"].ToString();
                }
                conn.Close();
            }

            if (sourceDoviz == targetDoviz)
            {
                return 1;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT TOP 1 AlisKur 
                FROM DovizKurlari dk
                INNER JOIN Dovizler d ON dk.DovizID = d.DovizID
                WHERE d.DovizSembol = @DovizSembol
                ORDER BY KurTarihi DESC";

                SqlCommand cmdSource = new SqlCommand(query, conn);
                cmdSource.Parameters.AddWithValue("@DovizSembol", sourceDoviz);

                SqlCommand cmdTarget = new SqlCommand(query, conn);
                cmdTarget.Parameters.AddWithValue("@DovizSembol", targetDoviz);

                conn.Open();
                object sourceResult = cmdSource.ExecuteScalar();
                if (sourceResult != null)
                {
                    sourceExchangeRate = Convert.ToDecimal(sourceResult);
                }

                object targetResult = cmdTarget.ExecuteScalar();
                if (targetResult != null)
                {
                    targetExchangeRate = Convert.ToDecimal(targetResult);
                }
                conn.Close();
            }

            decimal calculatedExchangeRate = Math.Round(targetExchangeRate / sourceExchangeRate, 6);
            return calculatedExchangeRate;
        }
        protected void rptTransactions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Veri kaynağından geçerli veriyi alın
                DataRowView rowView = (DataRowView)e.Item.DataItem;
                object transferID = rowView["TransferID"];

                // Kontrolleri bulun
                PlaceHolder phTransferControls = (PlaceHolder)e.Item.FindControl("phTransferControls");
                PlaceHolder phNormalControls = (PlaceHolder)e.Item.FindControl("phNormalControls");

                // TransferID'yi kontrol edin ve ilgili kontrolleri gösterin veya gizleyin
                if (transferID != DBNull.Value)
                {
                    phTransferControls.Visible = true;
                    phNormalControls.Visible = false;
                }
                else
                {
                    phTransferControls.Visible = false;
                    phNormalControls.Visible = true;
                }
            }
        }

        // Transfer işlemleri hesaplama
        protected void txtTransferAmount_TextChanged(object sender, EventArgs e)
        {
            decimal amount, exchangeRate;

            if (decimal.TryParse(txtTransferAmount.Text, out amount) &&
                decimal.TryParse(txtExchangeRate.Text, out exchangeRate))
            {
                txtConvertedAmount.Text = (amount * exchangeRate).ToString("N2");
            }
        }
        protected void txtExchangeRate_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtExchangeRate.Text) || string.IsNullOrEmpty(txtTransferAmount.Text))
            {
                return;
            }

            try
            {
                decimal originalAmount = Convert.ToDecimal(txtTransferAmount.Text);
                decimal exchangeRate = Convert.ToDecimal(txtExchangeRate.Text);
                decimal convertedAmount = originalAmount * exchangeRate;
                txtConvertedAmount.Text = convertedAmount.ToString("N2");
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Hata!', 'Döviz kuru hesaplanırken bir hata oluştu.', 'error');", true);
            }
        }
        protected void txtConvertedAmount_TextChanged(object sender, EventArgs e)
        {
            decimal amount, convertedAmount;

            if (decimal.TryParse(txtTransferAmount.Text, out amount) &&
                decimal.TryParse(txtConvertedAmount.Text, out convertedAmount) && amount != 0)
            {
                txtExchangeRate.Text = (convertedAmount / amount).ToString("N6");
            }
        }

        // Transfer düzenleme işlemleri hesaplama

        protected void txtEditTransferAmount_TextChanged(object sender, EventArgs e)
        {
            decimal amount, exchangeRate;

            if (decimal.TryParse(txtEditTransferAmount.Text, out amount) &&
                decimal.TryParse(txtEditExchangeRate.Text, out exchangeRate) && exchangeRate > 0)
            {
                txtEditConvertedAmount.Text = (amount * exchangeRate).ToString("N2");
            }
        }

        protected void txtEditExchangeRate_TextChanged(object sender, EventArgs e)
        {
            decimal amount, exchangeRate;

            if (decimal.TryParse(txtEditTransferAmount.Text, out amount) &&
                decimal.TryParse(txtEditExchangeRate.Text, out exchangeRate) && amount > 0)
            {
                txtEditConvertedAmount.Text = (amount * exchangeRate).ToString("N2");
            }
        }

        protected void txtEditConvertedAmount_TextChanged(object sender, EventArgs e)
        {
            decimal amount, convertedAmount;

            if (decimal.TryParse(txtEditTransferAmount.Text, out amount) &&
                decimal.TryParse(txtEditConvertedAmount.Text, out convertedAmount) && amount != 0)
            {
                txtEditExchangeRate.Text = (convertedAmount / amount).ToString("N6");
            }
        }
        //protected void txtEditTransferAmount_TextChanged(object sender, EventArgs e)
        //{
        //    decimal amount, exchangeRate;

        //    if (decimal.TryParse(txtEditTransferAmount.Text, out amount) &&
        //        decimal.TryParse(txtEditExchangeRate.Text, out exchangeRate))
        //    {
        //        txtConvertedAmount.Text = (amount * exchangeRate).ToString("N2");
        //    }
        //}
        //protected void txtEditExchangeRate_TextChanged(object sender, EventArgs e)
        //{
        //    decimal amount, exchangeRate;

        //    if (decimal.TryParse(txtEditTransferAmount.Text, out amount) &&
        //        decimal.TryParse(txtEditExchangeRate.Text, out exchangeRate))
        //    {
        //        txtEditConvertedAmount.Text = (amount * exchangeRate).ToString("N2");
        //    }
        //}
        //protected void txtEditConvertedAmount_TextChanged(object sender, EventArgs e)
        //{
        //    decimal amount, convertedAmount;

        //    if (decimal.TryParse(txtEditTransferAmount.Text, out amount) &&
        //        decimal.TryParse(txtEditConvertedAmount.Text, out convertedAmount) && amount != 0)
        //    {
        //        txtEditExchangeRate.Text = (convertedAmount / amount).ToString("N6");
        //    }
        //}
        



        protected void btnUpdateTransaction_Click(object sender, EventArgs e)
        {
            string hareketID = hfEditTransactionID.Value;
            string kasaID = ddlEditCashAccount.SelectedValue;
            string cariID = ddlEditCustomer.SelectedValue;
            decimal tutar = Convert.ToDecimal(txtEditAmount.Text);
            string islemTuru = ddlEditTransactionType.SelectedValue;
            string aciklama = txtEditDescription.Text;
            DateTime islemTarihi = string.IsNullOrEmpty(txtEditTransactionDate.Text) ? DateTime.Now : Convert.ToDateTime(txtEditTransactionDate.Text);
            string guncelleyenKullaniciID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = @"
            UPDATE KasaHareketleri 
            SET 
                KasaID = @KasaID, 
                IslemTarihi = @IslemTarihi, 
                Tutar = @Tutar, 
                IslemTuru = @IslemTuru, 
                Aciklama = @Aciklama, 
                CariID = @CariID, 
                SonGuncelleyenKullaniciID = @SonGuncelleyenKullaniciID, 
                GuncellemeTarihi = GETDATE() 
            WHERE HareketID = @HareketID";

                    SqlCommand cmd = new SqlCommand(query, conn, transaction);
                    cmd.Parameters.AddWithValue("@HareketID", hareketID);
                    cmd.Parameters.AddWithValue("@KasaID", kasaID);
                    cmd.Parameters.AddWithValue("@IslemTarihi", islemTarihi);
                    cmd.Parameters.AddWithValue("@Tutar", tutar);
                    cmd.Parameters.AddWithValue("@IslemTuru", islemTuru);
                    cmd.Parameters.AddWithValue("@Aciklama", aciklama);
                    cmd.Parameters.AddWithValue("@CariID", string.IsNullOrEmpty(cariID) ? (object)DBNull.Value : cariID);
                    cmd.Parameters.AddWithValue("@SonGuncelleyenKullaniciID", guncelleyenKullaniciID);
                    cmd.ExecuteNonQuery();

                    if (!string.IsNullOrEmpty(cariID) && (islemTuru == "Tahsilat" || islemTuru == "Ödeme"))
                    {
                        string cariQuery = @"
                UPDATE CariHareketler 
                SET 
                    CariID = @CariID,
                    IslemTarihi = @IslemTarihi, 
                    Tutar = @Tutar, 
                    IslemTuru = @IslemTuru, 
                    Aciklama = @Aciklama, 
                    SonGuncelleyenKullaniciID = @SonGuncelleyenKullaniciID, 
                    GuncellemeTarihi = GETDATE()
                WHERE EvrakID = @HareketID";

                        SqlCommand cmdCari = new SqlCommand(cariQuery, conn, transaction);
                        cmdCari.Parameters.AddWithValue("@HareketID", hareketID);
                        cmdCari.Parameters.AddWithValue("@CariID", cariID);
                        cmdCari.Parameters.AddWithValue("@IslemTarihi", islemTarihi);
                        cmdCari.Parameters.AddWithValue("@Tutar", tutar);
                        cmdCari.Parameters.AddWithValue("@IslemTuru", islemTuru);
                        cmdCari.Parameters.AddWithValue("@Aciklama", aciklama);
                        cmdCari.Parameters.AddWithValue("@SonGuncelleyenKullaniciID", guncelleyenKullaniciID);
                        cmdCari.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Güncelleme işlemi sırasında hata oluştu: " + ex.Message);
                }
            }

            LoadTransactions();
            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Başarılı!', 'Hareket başarıyla güncellendi.', 'success').then(() => { $('#editTransactionModal').modal('hide'); });", true);
        }

        private void ClearEditTransactionFields()
        {
            hfEditTransactionID.Value = string.Empty;
            ddlEditCashAccount.SelectedIndex = 0;
            ddlEditCustomer.SelectedIndex = 0;
            txtEditAmount.Text = string.Empty;
            ddlEditTransactionType.SelectedIndex = 0;
            txtEditDescription.Text = string.Empty;
            txtEditTransactionDate.Text = string.Empty;
        }

        protected void btnSaveTransaction_Click(object sender, EventArgs e)
        {
            string kasaID = ddlNewCashAccount.SelectedValue;
            string cariID = ddlNewCustomer.SelectedValue;
            decimal tutar = Convert.ToDecimal(txtNewAmount.Text);
            string islemTuru = ddlNewTransactionType.SelectedValue;
            string aciklama = txtNewDescription.Text;
            DateTime islemTarihi = string.IsNullOrEmpty(txtNewTransactionDate.Text) ? DateTime.Now : Convert.ToDateTime(txtNewTransactionDate.Text);
            string olusturanKullaniciID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";

            string hareketNo = GenerateNextCashTransactionNumber();
            string hareketID = Guid.NewGuid().ToString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = @"
            INSERT INTO KasaHareketleri 
            (HareketID, CariID, HareketNo, KasaID, IslemTarihi, Tutar, IslemTuru, Aciklama, OlusturanKullaniciID, SoftDelete, OlusturmaTarihi) 
            VALUES 
            (@HareketID, @CariID, @HareketNo, @KasaID, @IslemTarihi, @Tutar, @IslemTuru, @Aciklama, @OlusturanKullaniciID, 0, GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, conn, transaction);
                    cmd.Parameters.AddWithValue("@HareketID", hareketID);
                    cmd.Parameters.AddWithValue("@CariID", cariID);
                    cmd.Parameters.AddWithValue("@HareketNo", hareketNo);
                    cmd.Parameters.AddWithValue("@KasaID", kasaID);
                    cmd.Parameters.AddWithValue("@IslemTarihi", islemTarihi);
                    cmd.Parameters.AddWithValue("@Tutar", tutar);
                    cmd.Parameters.AddWithValue("@IslemTuru", islemTuru);
                    cmd.Parameters.AddWithValue("@Aciklama", aciklama);
                    cmd.Parameters.AddWithValue("@OlusturanKullaniciID", olusturanKullaniciID);
                    cmd.ExecuteNonQuery();

                    if (!string.IsNullOrEmpty(cariID) && (islemTuru == "Tahsilat" || islemTuru == "Ödeme"))
                    {
                        string cariHareketID = Guid.NewGuid().ToString();

                        string cariQuery = @"
                INSERT INTO CariHareketler 
                (HareketID, CariID, IslemTarihi, Tutar, IslemTuru, Aciklama, OlusturanKullaniciID, SoftDelete, OlusturmaTarihi, EvrakID) 
                VALUES 
                (@CariHareketID, @CariID, @IslemTarihi, @Tutar, @IslemTuru, @Aciklama, @OlusturanKullaniciID, 0, GETDATE(), @EvrakID)";

                        SqlCommand cmdCari = new SqlCommand(cariQuery, conn, transaction);
                        cmdCari.Parameters.AddWithValue("@CariHareketID", cariHareketID);
                        cmdCari.Parameters.AddWithValue("@CariID", cariID);
                        cmdCari.Parameters.AddWithValue("@IslemTarihi", islemTarihi);
                        cmdCari.Parameters.AddWithValue("@Tutar", tutar);
                        cmdCari.Parameters.AddWithValue("@IslemTuru", islemTuru);
                        cmdCari.Parameters.AddWithValue("@Aciklama", aciklama);
                        cmdCari.Parameters.AddWithValue("@OlusturanKullaniciID", olusturanKullaniciID);
                        cmdCari.Parameters.AddWithValue("@EvrakID", hareketID);
                        cmdCari.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("İşlem sırasında hata oluştu: " + ex.Message);
                }
            }

            LoadTransactions();
            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Başarılı!', 'Hareket başarıyla eklendi.', 'success').then(() => { $('#addTransactionModal').modal('hide'); });", true);
        }

        private void ClearNewTransactionFields()
        {
            ddlNewCashAccount.SelectedIndex = 0;
            ddlNewCustomer.SelectedIndex = 0;
            txtNewAmount.Text = string.Empty;
            ddlNewTransactionType.SelectedIndex = 0;
            txtNewDescription.Text = string.Empty;
            txtNewTransactionDate.Text = string.Empty;
        }

        private void LoadTransactionForEdit(string hareketID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT KasaID, IslemTarihi, Tutar, IslemTuru, Aciklama, CariID 
        FROM KasaHareketleri 
        WHERE HareketID = @HareketID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HareketID", hareketID);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    if (ddlEditCashAccount.Items.Count == 0)
                    {
                        LoadCashAccounts();
                    }
                    if (ddlEditCustomer.Items.Count == 0)
                    {
                        LoadCustomers();
                    }

                    hfEditTransactionID.Value = hareketID;
                    txtEditTransactionDate.Text = Convert.ToDateTime(reader["IslemTarihi"]).ToString("yyyy-MM-dd");
                    txtEditAmount.Text = Convert.ToDecimal(reader["Tutar"]).ToString("N2");
                    ddlEditTransactionType.SelectedValue = reader["IslemTuru"].ToString();
                    txtEditDescription.Text = reader["Aciklama"].ToString();

                    string kasaID = reader["KasaID"].ToString();
                    if (ddlEditCashAccount.Items.FindByValue(kasaID) != null)
                    {
                        ddlEditCashAccount.SelectedValue = kasaID;
                    }

                    string cariID = reader["CariID"] != DBNull.Value ? reader["CariID"].ToString() : "";
                    if (!string.IsNullOrEmpty(cariID) && ddlEditCustomer.Items.FindByValue(cariID) != null)
                    {
                        ddlEditCustomer.SelectedValue = cariID;
                    }
                }
                conn.Close();
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "openEditModal", "showEditTransactionModal();", true);
        }

        private void LoadTransferForEdit(string transferID)
        {
            string sourceKasaID = null;
            string targetKasaID = null;
            decimal amount = 0;
            decimal convertedAmount = 0;
            string sourceDescription = null;
            string targetDescription = null;
            DateTime transactionDate = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT KasaID, Tutar, IslemTarihi, Aciklama, TransferYonu
        FROM KasaHareketleri
        WHERE TransferID = @TransferID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TransferID", transferID);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        transactionDate = Convert.ToDateTime(reader["IslemTarihi"]);
                        txtEditTransferDate.Text = transactionDate.ToString("yyyy-MM-dd");

                        if (reader["TransferYonu"].ToString() == "Source")
                        {
                            sourceKasaID = reader["KasaID"].ToString();
                            amount = Convert.ToDecimal(reader["Tutar"]);
                            sourceDescription = reader["Aciklama"].ToString();
                        }
                        else if (reader["TransferYonu"].ToString() == "Target")
                        {
                            targetKasaID = reader["KasaID"].ToString();
                            convertedAmount = Convert.ToDecimal(reader["Tutar"]);
                            targetDescription = reader["Aciklama"].ToString();
                        }
                    }
                }
                conn.Close();
            }

            hfEditTransferID.Value = transferID;

            LoadSourceCashAccounts(); // Load source accounts first

            if (!string.IsNullOrEmpty(sourceKasaID))
            {
                ddlEditSourceCashAccount.SelectedValue = sourceKasaID;
                LoadTargetCashAccounts(sourceKasaID); // Load target accounts based on sourceKasaID
            }
            else
            {
                // Kaynak kasa ID'si boşsa hata mesajı göster
                throw new ArgumentException("sourceKasaID null veya boş olamaz", nameof(sourceKasaID));
            }

            if (!string.IsNullOrEmpty(targetKasaID) && ddlEditTargetCashAccount.Items.FindByValue(targetKasaID) != null)
            {
                ddlEditTargetCashAccount.SelectedValue = targetKasaID;
            }

            txtEditTransferAmount.Text = amount.ToString("N2");
            txtEditConvertedAmount.Text = convertedAmount.ToString("N2");
            txtEditSourceDescription.Text = sourceDescription;
            txtEditTargetDescription.Text = targetDescription;

            if (amount != 0)
            {
                decimal exchangeRate = convertedAmount / amount;
                txtEditExchangeRate.Text = exchangeRate.ToString("N6");
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "openEditTransferModal", "showEditTransferModal();", true);
        }

        private void LoadTargetCashAccounts(string sourceKasaID)
        {
            if (string.IsNullOrEmpty(sourceKasaID))
            {
                throw new ArgumentException("sourceKasaID null veya boş olamaz", nameof(sourceKasaID));
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT KasaID, KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim
        FROM Kasalar k
        LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
        WHERE k.SoftDelete = 0 AND k.KasaID <> @SourceKasaID
        ORDER BY k.KasaAdi ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SourceKasaID", sourceKasaID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlEditTargetCashAccount.DataSource = dt;
                ddlEditTargetCashAccount.DataTextField = "KasaGosterim";
                ddlEditTargetCashAccount.DataValueField = "KasaID";
                ddlEditTargetCashAccount.DataBind();

                ddlEditTargetCashAccount.Items.Insert(0, new ListItem("Hedef Kasa Seçin", ""));
            }
        }


        protected void btnEditTransfer_Click(object sender, EventArgs e)
        {
            // Transfer düzenleme butonuna tıklanınca
            Guid transferID = Guid.Parse(hfEditTransferID.Value);

            DataRow transferData = GetTransferData(transferID);
            if (transferData != null)
            {
                string transferDirection = transferData["TransferYonu"].ToString();
                decimal transferAmount = decimal.Parse(transferData["Tutar"].ToString());
                decimal convertedAmount = decimal.Parse(transferData["ConvertedAmount"].ToString());

                if (transferDirection == "Source")
                {
                    ddlEditSourceCashAccount.SelectedValue = transferData["KasaID"].ToString();
                    txtEditTransferAmount.Text = transferAmount.ToString("N2");
                    txtEditConvertedAmount.Text = convertedAmount.ToString("N2");
                    txtEditExchangeRate.Text = (convertedAmount / transferAmount).ToString("N6");
                }
                else if (transferDirection == "Target")
                {
                    ddlEditTargetCashAccount.SelectedValue = transferData["KasaID"].ToString();
                    txtEditTransferAmount.Text = convertedAmount.ToString("N2");
                    txtEditConvertedAmount.Text = transferAmount.ToString("N2");
                    txtEditExchangeRate.Text = (transferAmount / convertedAmount).ToString("N6");
                }

                txtEditSourceDescription.Text = transferData["SourceDescription"].ToString();
                txtEditTargetDescription.Text = transferData["TargetDescription"].ToString();

                // Modal'ı aç
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditTransferModal", "$('#editTransferModal').modal('show');", true);
            }
        }

        protected void btnUpdateTransfer_Click(object sender, EventArgs e)
        {
            // Güncellenecek transfer bilgilerini al
            string transferID = hfEditTransferID.Value;
            string sourceKasaID = ddlEditSourceCashAccount.SelectedValue;
            string targetKasaID = ddlEditTargetCashAccount.SelectedValue;
            decimal amount = Convert.ToDecimal(txtEditTransferAmount.Text);
            DateTime transactionDate = string.IsNullOrEmpty(txtEditTransferDate.Text) ? DateTime.Now : Convert.ToDateTime(txtEditTransferDate.Text);
            string sourceDescription = txtEditSourceDescription.Text;
            string targetDescription = txtEditTargetDescription.Text;
            string userID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";

            // Döviz kurunu hesapla
            decimal exchangeRate;
            if (!decimal.TryParse(txtEditExchangeRate.Text, out exchangeRate) || exchangeRate <= 0)
            {
                exchangeRate = LoadExchangeRate(sourceKasaID, targetKasaID);
            }
            decimal convertedAmount = amount * exchangeRate;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Kaynak kasa hareketini güncelle
                    string queryOut = @"
        UPDATE KasaHareketleri 
        SET 
            KasaID = @SourceKasaID, 
            IslemTarihi = @IslemTarihi, 
            Tutar = @Amount, 
            Aciklama = @SourceDescription, 
            SonGuncelleyenKullaniciID = @SonGuncelleyenKullaniciID, 
            GuncellemeTarihi = GETDATE() 
        WHERE TransferID = @TransferID AND TransferYonu = 'Source'";

                    SqlCommand cmdOut = new SqlCommand(queryOut, conn, transaction);
                    cmdOut.Parameters.AddWithValue("@TransferID", transferID);
                    cmdOut.Parameters.AddWithValue("@SourceKasaID", sourceKasaID);
                    cmdOut.Parameters.AddWithValue("@IslemTarihi", transactionDate);
                    cmdOut.Parameters.AddWithValue("@Amount", amount);
                    cmdOut.Parameters.AddWithValue("@SourceDescription", sourceDescription);
                    cmdOut.Parameters.AddWithValue("@SonGuncelleyenKullaniciID", userID);
                    cmdOut.ExecuteNonQuery();

                    // Hedef kasa hareketini güncelle
                    string queryIn = @"
        UPDATE KasaHareketleri 
        SET 
            KasaID = @TargetKasaID, 
            IslemTarihi = @IslemTarihi, 
            Tutar = @ConvertedAmount, 
            Aciklama = @TargetDescription, 
            SonGuncelleyenKullaniciID = @SonGuncelleyenKullaniciID, 
            GuncellemeTarihi = GETDATE() 
        WHERE TransferID = @TransferID AND TransferYonu = 'Target'";

                    SqlCommand cmdIn = new SqlCommand(queryIn, conn, transaction);
                    cmdIn.Parameters.AddWithValue("@TransferID", transferID);
                    cmdIn.Parameters.AddWithValue("@TargetKasaID", targetKasaID);
                    cmdIn.Parameters.AddWithValue("@IslemTarihi", transactionDate);
                    cmdIn.Parameters.AddWithValue("@ConvertedAmount", convertedAmount);
                    cmdIn.Parameters.AddWithValue("@TargetDescription", targetDescription);
                    cmdIn.Parameters.AddWithValue("@SonGuncelleyenKullaniciID", userID);
                    cmdIn.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Transfer güncelleme işlemi sırasında hata oluştu: " + ex.Message);
                }
            }

            // Güncellenmiş transfer hareketlerini yükle ve modalı kapat
            LoadTransactions();
            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Başarılı!', 'Transfer başarıyla güncellendi.', 'success').then(() => { $('#editTransferModal').modal('hide'); });", true);
        }


        protected DataRow GetTransferData(Guid transferID)
        {
            // Veritabanı bağlantı dizesi
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

            // Sorgu
            string query = @"
                SELECT
                    KH.HareketID,
                    KH.KasaID,
                    KH.IslemTarihi,
                    KH.Tutar,
                    KH.IslemTuru,
                    KH.Aciklama AS SourceDescription,
                    KH2.Aciklama AS TargetDescription,
                    KH.TransferYonu,
                    KH2.Tutar AS ConvertedAmount
                FROM
                    KasaHareketleri KH
                LEFT JOIN
                    KasaHareketleri KH2 ON KH.TransferID = KH2.HareketID
                WHERE
                    KH.HareketID = @TransferID";

            // DataTable oluştur
            DataTable dt = new DataTable();

            // Veritabanı bağlantısı
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // SqlCommand oluştur
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TransferID", transferID);

                    // SqlDataAdapter oluştur
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        // Verileri DataTable'a doldur
                        da.Fill(dt);
                    }
                }
            }

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }
        protected void btnSaveTransfer_Click(object sender, EventArgs e)
        {
            string sourceKasaID = ddlSourceCashAccount.SelectedValue;
            string targetKasaID = ddlTargetCashAccount.SelectedValue;
            decimal amount = Convert.ToDecimal(txtTransferAmount.Text);
            DateTime transactionDate = string.IsNullOrEmpty(txtTransferDate.Text) ? DateTime.Now : Convert.ToDateTime(txtTransferDate.Text);
            string description = txtTransferDescription.Text;
            string userID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";

            decimal exchangeRate;
            if (!decimal.TryParse(txtExchangeRate.Text, out exchangeRate) || exchangeRate <= 0)
            {
                exchangeRate = LoadExchangeRate(sourceKasaID, targetKasaID);
            }
            decimal convertedAmount = amount * exchangeRate;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string sourceTransactionNo = GenerateNextCashTransactionNumber();
                    string targetTransactionNo = GenerateNextCashTransactionNumber();
                    string transferID = Guid.NewGuid().ToString();

                    // 🟢 **Kaynak Kasa İşlem Kaydı (Ödeme)**
                    string queryOut = @"
            INSERT INTO KasaHareketleri (HareketID, HareketNo, KasaID, IslemTarihi, Tutar, IslemTuru, Aciklama, OlusturanKullaniciID, SoftDelete, OlusturmaTarihi, TransferID, TransferYonu) 
            VALUES (NEWID(), @HareketNo, @SourceKasaID, @IslemTarihi, @Amount, 'Ödeme', @Description, @UserID, 0, GETDATE(), @TransferID, 'Source')";

                    SqlCommand cmdOut = new SqlCommand(queryOut, conn, transaction);
                    cmdOut.Parameters.AddWithValue("@HareketNo", sourceTransactionNo);
                    cmdOut.Parameters.AddWithValue("@SourceKasaID", sourceKasaID);
                    cmdOut.Parameters.AddWithValue("@IslemTarihi", transactionDate);
                    cmdOut.Parameters.AddWithValue("@Amount", amount);
                    cmdOut.Parameters.AddWithValue("@Description", $"{description} - {amount} gönderildi (Kur: {exchangeRate})");
                    cmdOut.Parameters.AddWithValue("@UserID", userID);
                    cmdOut.Parameters.AddWithValue("@TransferID", transferID);
                    cmdOut.ExecuteNonQuery();

                    // 🔵 **Hedef Kasa İşlem Kaydı (Tahsilat)**
                    string queryIn = @"
            INSERT INTO KasaHareketleri (HareketID, HareketNo, KasaID, IslemTarihi, Tutar, IslemTuru, Aciklama, OlusturanKullaniciID, SoftDelete, OlusturmaTarihi, TransferID, TransferYonu) 
            VALUES (NEWID(), @HareketNo, @TargetKasaID, @IslemTarihi, @ConvertedAmount, 'Tahsilat', @Description, @UserID, 0, GETDATE(), @TransferID, 'Target')";

                    SqlCommand cmdIn = new SqlCommand(queryIn, conn, transaction);
                    cmdIn.Parameters.AddWithValue("@HareketNo", targetTransactionNo);
                    cmdIn.Parameters.AddWithValue("@TargetKasaID", targetKasaID);
                    cmdIn.Parameters.AddWithValue("@IslemTarihi", transactionDate);
                    cmdIn.Parameters.AddWithValue("@ConvertedAmount", convertedAmount);
                    cmdIn.Parameters.AddWithValue("@Description", $"{description} - {convertedAmount} alındı (Kur: {exchangeRate})");
                    cmdIn.Parameters.AddWithValue("@UserID", userID);
                    cmdIn.Parameters.AddWithValue("@TransferID", transferID);
                    cmdIn.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Transfer işlemi sırasında hata oluştu: " + ex.Message);
                }
            }

            ClearTransferFields();
            LoadTransactions();
            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Başarılı!', 'Transfer işlemi başarıyla kaydedildi.', 'success').then(() => { $('#transferModal').modal('hide'); });", true);
        }

        private void ClearTransferFields()
        {
            ddlSourceCashAccount.SelectedIndex = 0;
            ddlTargetCashAccount.SelectedIndex = 0;
            txtTransferAmount.Text = string.Empty;
            txtExchangeRate.Text = string.Empty;
            txtConvertedAmount.Text = string.Empty;
            txtTransferDescription.Text = string.Empty;
            txtTransferDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }

        private string GenerateNextCashTransactionNumber()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string todayPrefix = "KH-" + DateTime.Now.ToString("yyyyMMdd") + "-";
                string query = @"
            IF NOT EXISTS (SELECT 1 FROM KasaHareketNumaralari WHERE Yil = YEAR(GETDATE()))
            BEGIN
                INSERT INTO KasaHareketNumaralari (Yil, SonNumara) VALUES (YEAR(GETDATE()), 0);
            END
            ELSE
            BEGIN
                UPDATE KasaHareketNumaralari
                SET SonNumara = SonNumara + 1
                WHERE Yil = YEAR(GETDATE());
            END;

            SELECT CONCAT(
                'KH-', 
                FORMAT(GETDATE(), 'yyyyMMdd'), 
                '-', 
                FORMAT(SonNumara, '0000')
            ) AS HareketNo
            FROM KasaHareketNumaralari WHERE Yil = YEAR(GETDATE());";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString() ?? "HATA";
            }
        }

        protected void btnShowTransferModal_Click(object sender, EventArgs e)
        {
            LoadCashAccounts();
            ScriptManager.RegisterStartupScript(this, GetType(), "openTransferModal", "$('#transferModal').modal('show');", true);
        }

        protected void rptTransactions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string hareketID = e.CommandArgument.ToString();

            if (e.CommandName == "Edit")
            {
                LoadCashAccounts();
                LoadCustomers();
                LoadTransactionForEdit(hareketID);
            }
            else if (e.CommandName == "EditTransfer")
            {
                LoadCashAccounts();
                LoadTransferForEdit(hareketID);
            }
            else if (e.CommandName == "Delete")
            {
                DeleteTransaction(hareketID);
            }
            else if (e.CommandName == "DeleteTransfer")
            {
                string transferID = e.CommandArgument.ToString();
                DeleteTransfer(transferID);
            }
        }

        private void LoadFilterCustomers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CariID, CariAdi FROM Cariler WHERE CariAdi IS NOT NULL ORDER BY CariAdi ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlFilterCustomer.DataSource = dt;
                ddlFilterCustomer.DataTextField = "CariAdi";
                ddlFilterCustomer.DataValueField = "CariID";
                ddlFilterCustomer.DataBind();
                ddlFilterCustomer.Items.Insert(0, new ListItem("Tüm Müşteriler", ""));
            }
        }

        private string GetCurrencySymbol(string kasaID)
        {
            string currencySymbol = "";

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
                if (result != null)
                {
                    currencySymbol = result.ToString();
                }
                conn.Close();
            }

            return currencySymbol;
        }

        private void LoadCustomers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CariID, CariAdi FROM Cariler WHERE SoftDelete = 0 ORDER BY CariAdi ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // 🟢 **Yeni Hareket Modalı İçin**
                ddlNewCustomer.DataSource = dt;
                ddlNewCustomer.DataTextField = "CariAdi";
                ddlNewCustomer.DataValueField = "CariID";
                ddlNewCustomer.DataBind();
                ddlNewCustomer.Items.Insert(0, new ListItem("Müşteri Seçin", ""));

                // 🔵 **Güncelleme Modalı İçin**
                ddlEditCustomer.DataSource = dt;
                ddlEditCustomer.DataTextField = "CariAdi";
                ddlEditCustomer.DataValueField = "CariID";
                ddlEditCustomer.DataBind();
                ddlEditCustomer.Items.Insert(0, new ListItem("Müşteri Seçin", ""));
            }
        }


        private void LoadCashAccounts()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT KasaID, KasaAdi + ' (' + ISNULL(d.DovizSembol, '') + ')' AS KasaGosterim
        FROM Kasalar k
        LEFT JOIN Dovizler d ON k.DovizID = d.DovizID
        WHERE k.SoftDelete = 0
        ORDER BY KasaAdi ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // 🟢 **Yeni Kasa Hareketi İçin Dropdown**
                ddlNewCashAccount.DataSource = dt;
                ddlNewCashAccount.DataTextField = "KasaGosterim";
                ddlNewCashAccount.DataValueField = "KasaID";
                ddlNewCashAccount.DataBind();
                ddlNewCashAccount.Items.Insert(0, new ListItem("Kasa Seçin", ""));

                // 🔵 **Güncelleme Modalı İçin Dropdown**
                ddlEditCashAccount.DataSource = dt;
                ddlEditCashAccount.DataTextField = "KasaGosterim";
                ddlEditCashAccount.DataValueField = "KasaID";
                ddlEditCashAccount.DataBind();
                ddlEditCashAccount.Items.Insert(0, new ListItem("Kasa Seçin", ""));

                // 🟢 **Transfer Modalı İçin Source Dropdown**
                ddlSourceCashAccount.DataSource = dt;
                ddlSourceCashAccount.DataTextField = "KasaGosterim";
                ddlSourceCashAccount.DataValueField = "KasaID";
                ddlSourceCashAccount.DataBind();
                ddlSourceCashAccount.Items.Insert(0, new ListItem("Kaynak Kasa Seçin", ""));

                // 🔵 **Transfer Modalı İçin Target Dropdown**
                ddlTargetCashAccount.DataSource = dt;
                ddlTargetCashAccount.DataTextField = "KasaGosterim";
                ddlTargetCashAccount.DataValueField = "KasaID";
                ddlTargetCashAccount.DataBind();
                ddlTargetCashAccount.Items.Insert(0, new ListItem("Hedef Kasa Seçin", ""));


                ddlEditSourceCashAccount.DataSource = dt;
                ddlEditSourceCashAccount.DataTextField = "KasaGosterim";
                ddlEditSourceCashAccount.DataValueField = "KasaID";
                ddlEditSourceCashAccount.DataBind();

                ddlEditSourceCashAccount.Items.Insert(0, new ListItem("Kaynak Kasa Seçin", ""));
            }
        }
    }
}