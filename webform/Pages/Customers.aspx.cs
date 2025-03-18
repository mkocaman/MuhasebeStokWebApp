using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class Customers : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCustomers();
            }
        }

        private void LoadCustomers(string sortExpression = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CariID, CariAdi, Telefon, Email FROM Cariler WHERE Aktif = 1 AND SoftDelete = 0";
                if (!string.IsNullOrEmpty(sortExpression))
                {
                    query += " ORDER BY " + sortExpression;
                }
                else
                {
                    query += " ORDER BY CariAdi";
                }
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptCustomers.DataSource = dt;
                rptCustomers.DataBind();
            }
        }

        protected void rptCustomers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string cariID = e.CommandArgument.ToString();

            if (e.CommandName == "Details")
            {
                ShowCustomerDetails(cariID);
            }
            else if (e.CommandName == "Edit")
            {
                LoadCustomerForEdit(cariID);
            }
            else if (e.CommandName == "Delete")
            {
                hfDeleteCustomerId.Value = cariID;
                ScriptManager.RegisterStartupScript(this, this.GetType(), "openDeleteModal", "$('#deleteModal').modal('show');", true);
                UpdatePanelDelete.Update(); // Update the panel to ensure the hidden field value is set
            }
            else if (e.CommandName == "Statement")
            {
                // Yönlendirme: Cari hesap ekstresini gösteren, örneğin "CustomerStatement.aspx" sayfasına
                Response.Redirect("CustomerStatement.aspx?CariID=" + cariID);
            }
        }

        private void ShowCustomerDetails(string cariID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Cariler WHERE CariID = @CariID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CariID", cariID);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lblCustomerDetails.Text = $"<strong>Müşteri Adı:</strong> {reader["CariAdi"]}<br/>" +
                                              $"<strong>Vergi No:</strong> {reader["VergiNo"]}<br/>" +
                                              $"<strong>Firma Yetkilisi:</strong> {reader["Yetkili"]}<br/>" +
                                              $"<strong>Telefon:</strong> {reader["Telefon"]}<br/>" +
                                              $"<strong>E-posta:</strong> {reader["Email"]}<br/>" +
                                              $"<strong>Adres:</strong> {reader["Adres"]}<br/>" +
                                              $"<strong>Açıklama:</strong> {reader["Aciklama"]}";
                }
                conn.Close();
            }
            ScriptManager.RegisterStartupScript(this, UpdatePanelDetails.GetType(), "showCustomerDetailsModal", "$('#customerDetailsModal').modal('show');", true);
            UpdatePanelDetails.Update();
        }

        private void LoadCustomerForEdit(string cariID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Cariler WHERE CariID = @CariID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CariID", cariID);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtCustomerName.Text = reader["CariAdi"].ToString();
                    txtVergiNo.Text = reader["VergiNo"].ToString();
                    txtYetkili.Text = reader["Yetkili"].ToString();
                    txtTelefon.Text = reader["Telefon"].ToString();
                    txtEmail.Text = reader["Email"].ToString();
                    txtAdres.Text = reader["Adres"].ToString();
                    txtAciklama.Text = reader["Aciklama"].ToString();
                    ViewState["EditCariID"] = cariID;
                }
                conn.Close();
            }
            ScriptManager.RegisterStartupScript(this, UpdatePanelAddEdit.GetType(), "showEditCustomerModal", "$('#addCustomerModal').modal('show');", true);
            UpdatePanelAddEdit.Update();
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            string cariID = hfDeleteCustomerId.Value;

            if (!string.IsNullOrEmpty(cariID))
            {
                try
                {
                    SoftDeleteCustomer(cariID);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "deleteSuccess", "Swal.fire('Başarılı!', 'Müşteri başarıyla pasif hale getirildi.', 'success').then(() => { location.reload(); });", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "deleteError", $"Swal.fire('Hata!', 'Hata: {ex.Message}', 'error');", true);
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "invalidCustomerId", "Swal.fire('Hata!', 'Geçersiz müşteri ID.', 'error');", true);
            }
        }

        private void SoftDeleteCustomer(string cariID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Cariler SET SoftDelete = 1 WHERE CariID = @CariID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CariID", cariID);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        protected void btnSaveCustomer_Click(object sender, EventArgs e)
        {
            string cariAdi = txtCustomerName.Text;
            string vergiNo = txtVergiNo.Text;
            string yetkili = txtYetkili.Text;
            string telefon = txtTelefon.Text;
            string email = txtEmail.Text;
            string adres = txtAdres.Text;
            string aciklama = txtAciklama.Text;

            string userID = HttpContext.Current.Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query;
                if (ViewState["EditCariID"] != null)
                {
                    query = @"
                UPDATE Cariler
                SET CariAdi = @CariAdi, VergiNo = @VergiNo, Yetkili = @Yetkili, Telefon = @Telefon, Email = @Email, Adres = @Adres, Aciklama = @Aciklama, GuncellemeTarihi = GETDATE(), SonGuncelleyenKullaniciID = @UserID
                WHERE CariID = @CariID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CariAdi", cariAdi);
                    cmd.Parameters.AddWithValue("@VergiNo", (object)vergiNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Yetkili", (object)yetkili ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefon", (object)telefon ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Adres", (object)adres ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Aciklama", (object)aciklama ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@CariID", ViewState["EditCariID"]);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    ViewState["EditCariID"] = null;
                }
                else
                {
                    query = @"
                INSERT INTO Cariler (CariID, CariAdi, VergiNo, Yetkili, Telefon, Email, Adres, Aciklama, Aktif, SoftDelete, OlusturanKullaniciID, OlusturmaTarihi)
                VALUES (NEWID(), @CariAdi, @VergiNo, @Yetkili, @Telefon, @Email, @Adres, @Aciklama, 1, 0, @UserID, GETDATE())";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CariAdi", cariAdi);
                    cmd.Parameters.AddWithValue("@VergiNo", (object)vergiNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Yetkili", (object)yetkili ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefon", (object)telefon ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Adres", (object)adres ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Aciklama", (object)aciklama ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                LoadCustomers(ViewState["SortExpression"]?.ToString());
            }

            ScriptManager.RegisterStartupScript(this, UpdatePanelAddEdit.GetType(), "hideAddCustomerModal", "$('#addCustomerModal').modal('hide');", true);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "showalert", "Swal.fire('Başarılı!', 'Müşteri başarıyla eklendi.', 'success').then(() => { location.reload(); });", true);
        }
    }
}