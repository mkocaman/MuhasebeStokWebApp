using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class ArchivedUsers : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadArchivedUsers();
            }
        }

        private void LoadArchivedUsers()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;
            string query = @"
                SELECT 
                    K.KullaniciID,
                    K.KullaniciAdi,
                    K.Email,
                    R.RolAdi,
                    K.OlusturmaTarihi
                FROM 
                    Kullanicilar K
                INNER JOIN 
                    Roller R ON K.Rol = R.RolID
                WHERE 
                    K.SoftDelete = 1";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptArchivedUsers.DataSource = dt;
                rptArchivedUsers.DataBind();
            }
        }

        protected void rptArchivedUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string userId = e.CommandArgument.ToString();

            if (e.CommandName == "RestoreUser")
            {
                RestoreUser(userId);
            }
            else if (e.CommandName == "PermanentDeleteUser")
            {
                hfPermanentDeleteUserId.Value = userId;
                ScriptManager.RegisterStartupScript(this, this.GetType(), "openPermanentDeleteModal", "$('#permanentDeleteModal').modal('show');", true);
            }
        }

        private void RestoreUser(string userId)
        {
            string query = "UPDATE Kullanicilar SET SoftDelete = 0 WHERE KullaniciID = @KullaniciID";
            ExecuteQuery(query, new SqlParameter("@KullaniciID", userId));
            LoadArchivedUsers();

            ScriptManager.RegisterStartupScript(this, GetType(), "restoreSuccess", "showSuccessModal('Kullanıcı başarıyla geri yüklendi.');", true);
        }

        protected void btnConfirmPermanentDelete_Click(object sender, EventArgs e)
        {
            // HiddenField'den kullanıcı ID'sini al
            string userId = hfPermanentDeleteUserId.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    // Kullanıcıyı kalıcı olarak sil
                    DeleteUserPermanently(userId);

                    // Listeyi yenile
                    LoadArchivedUsers();

                    // Başarı mesajı
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "deleteSuccess", "showSuccessModal('Kullanıcı başarıyla kalıcı olarak silindi.');", true);
                }
                catch (Exception ex)
                {
                    // Hata durumunda mesaj
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "deleteError", $"alert('Hata: {ex.Message}');", true);
                }
            }
            else
            {
                // Geçersiz kullanıcı ID'si için hata mesajı
                ScriptManager.RegisterStartupScript(this, this.GetType(), "invalidUserId", "alert('Geçersiz kullanıcı ID.');", true);
            }
        }

        private void DeleteUserPermanently(string userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

            string deleteRolesQuery = "DELETE FROM KullaniciRolleri WHERE KullaniciID = @KullaniciID";
            string deleteUserQuery = "DELETE FROM Kullanicilar WHERE KullaniciID = @KullaniciID";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(deleteRolesQuery, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@KullaniciID", userId);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(deleteUserQuery, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@KullaniciID", userId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void ExecuteQuery(string query, SqlParameter parameter)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add(parameter);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
