using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class Users : System.Web.UI.Page
    {
        // Web.config’deki bağlantı dizesi adı
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUsers();
                LoadRoles();
            }

            // Postback ile gelen event parametrelerini kontrol ediyoruz (ör. şifre sıfırlama, silme)
            string eventTarget = Request["__EVENTTARGET"];
            string eventArgument = Request["__EVENTARGUMENT"];
            if (!string.IsNullOrEmpty(eventTarget) && !string.IsNullOrEmpty(eventArgument))
            {
                if (eventTarget == "ResetPassword")
                {
                    ResetUserPassword(eventArgument);
                    ShowMessage("Şifre başarıyla sıfırlandı.", true);
                    LoadUsers();
                }
                else if (eventTarget == "DeleteUser")
                {
                    DeleteUser(eventArgument);
                    ShowMessage("Kullanıcı başarıyla silindi.", true);
                    LoadUsers();
                }
            }
        }

        #region Kullanıcı İşlemleri

        // Kullanıcı listesini yükler
        private void LoadUsers()
        {
            string query = @"
                SELECT 
                    K.KullaniciID,
                    K.KullaniciAdi,
                    K.Email,
                    R.RolAdi,
                    CASE WHEN K.Aktif = 1 THEN 'Aktif' ELSE 'Pasif' END AS Durum
                FROM 
                    Kullanicilar K
                INNER JOIN 
                    Roller R ON K.Rol = R.RolID
                WHERE 
                    K.SoftDelete = 0";
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptUsers.DataSource = dt;
                rptUsers.DataBind();
            }
        }

        // Roller listesini yükler
        private void LoadRoles()
        {
            string query = "SELECT RolID, RolAdi FROM Roller";
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                con.Open();
                DataTable dtRoles = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtRoles);

                // Güncelleme modalı için
                ddlEditRol.DataSource = dtRoles;
                ddlEditRol.DataTextField = "RolAdi";
                ddlEditRol.DataValueField = "RolID";
                ddlEditRol.DataBind();
                ddlEditRol.Items.Insert(0, new ListItem("Rol Seçiniz", "0"));

                // Yeni kullanıcı ekleme modalı için
                ddladdRoles.DataSource = dtRoles;
                ddladdRoles.DataTextField = "RolAdi";
                ddladdRoles.DataValueField = "RolID";
                ddladdRoles.DataBind();
                ddladdRoles.Items.Insert(0, new ListItem("Rol Seçiniz", "0"));
            }
        }

        // Yeni kullanıcı ekleme (şifreleme işlemi dahil)
        protected void btnAddUser_Click(object sender, EventArgs e)
        {
            string username = txtKullaniciAdi.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string role = ddladdRoles.SelectedValue;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowMessage("Tüm alanları doldurunuz.", false);
                return;
            }

            // Yeni kullanıcı için salt ve hash hesapla
            byte[] salt = GenerateSalt();
            byte[] hash = ComputeHash(password, salt);
            string saltString = Convert.ToBase64String(salt);
            Guid kullaniciID = Guid.NewGuid();

            string query = @"
                INSERT INTO Kullanicilar 
                (KullaniciID, KullaniciAdi, Email, Rol, SifreHash, Salt, Aktif, OlusturanKullaniciID, SoftDelete, OlusturmaTarihi)
                VALUES 
                (@KullaniciID, @KullaniciAdi, @Email, @Rol, @SifreHash, @Salt, 1, @OlusturanKullaniciID, 0, GETDATE())";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@KullaniciID", kullaniciID);
                    cmd.Parameters.AddWithValue("@KullaniciAdi", username);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Rol", role);
                    cmd.Parameters.Add("@SifreHash", SqlDbType.VarBinary, 64).Value = hash;
                    cmd.Parameters.AddWithValue("@Salt", saltString);
                    // Eğer oturumda KullaniciID yoksa varsayılan değer kullanılır.
                    string creator = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";
                    cmd.Parameters.AddWithValue("@OlusturanKullaniciID", creator);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                ShowMessage("Kullanıcı başarıyla eklendi.", true);
            }
            catch (Exception ex)
            {
                ShowMessage("Kullanıcı eklenirken hata oluştu: " + ex.Message, false);
            }
        }

        // Kullanıcı güncelleme işlemi
        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            string userId = hfEditUserId.Value;
            string username = txtEditKullaniciAdi.Text.Trim();
            string email = txtEditEmail.Text.Trim();
            string rol = ddlEditRol.SelectedValue;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || rol == "0")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error", "Swal.fire('Hata!', 'Tüm alanları doldurun.', 'error');", true);
                return;
            }

            // Güncelleme yapan kullanıcıyı oturumdan al; tanımlı değilse varsayılan değer kullanılır.
            string guncelleyenKullaniciID = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";

            string updateQuery = @"
                UPDATE Kullanicilar 
                SET 
                    KullaniciAdi = @KullaniciAdi,
                    Email = @Email,
                    Rol = @Rol,
                    SonGuncelleyenKullaniciID = @SonGuncelleyenKullaniciID,
                    GuncellemeTarihi = GETDATE()
                WHERE KullaniciID = @KullaniciID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@KullaniciID", userId);
                cmd.Parameters.AddWithValue("@KullaniciAdi", username);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Rol", rol);
                cmd.Parameters.AddWithValue("@SonGuncelleyenKullaniciID", guncelleyenKullaniciID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "success", @"
                Swal.fire({
                    title: 'Başarılı!',
                    text: 'Kullanıcı bilgileri başarıyla güncellendi.',
                    icon: 'success',
                    confirmButtonText: 'Tamam'
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.reload();
                    }
                });", true);
        }

        // Repeater içerisindeki kullanıcı işlemlerini (düzenle, şifre sıfırla, sil) yakalar
        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string userId = e.CommandArgument.ToString();
            if (e.CommandName == "EditUser")
            {
                hfEditUserId.Value = userId;
                LoadUserDetails(userId);
                ScriptManager.RegisterStartupScript(this, GetType(), "openEditModal", "$('#editUserModal').modal('show');", true);
            }
            else if (e.CommandName == "ResetPassword")
            {
                ResetUserPassword(userId);
                ShowMessage("Şifre başarıyla sıfırlandı.", true);
                LoadUsers();
            }
            else if (e.CommandName == "DeleteUser")
            {
                DeleteUser(userId);
                ShowMessage("Kullanıcı başarıyla silindi.", true);
                LoadUsers();
            }
        }

        // Güncelleme modalı için kullanıcı bilgilerini yükler
        private void LoadUserDetails(string userId)
        {
            string query = "SELECT KullaniciAdi, Email, Rol FROM Kullanicilar WHERE KullaniciID = @KullaniciID";
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@KullaniciID", userId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtEditKullaniciAdi.Text = reader["KullaniciAdi"].ToString();
                    txtEditEmail.Text = reader["Email"].ToString();
                    ddlEditRol.SelectedValue = reader["Rol"].ToString();
                }
            }
        }

        // Şifre sıfırlama işlemi
        private void ResetUserPassword(string userId)
        {
            string query = @"
                UPDATE Kullanicilar 
                SET SifreHash = @SifreHash, 
                    Salt = @Salt, 
                    SonGuncelleyenKullaniciID = @SonGuncelleyenKullaniciID, 
                    GuncellemeTarihi = GETDATE() 
                WHERE KullaniciID = @KullaniciID";

            // Varsayılan şifre "12345"
            byte[] salt = GenerateSalt();
            byte[] hash = ComputeHash("12345", salt);
            string saltString = Convert.ToBase64String(salt);

            // Eğer şifre sıfırlanan kullanıcı, güncelleme yapan kullanıcı ise FK çakışmasını önlemek için SonGuncelleyenKullaniciID'ni NULL yapıyoruz.
            string currentUser = Session["KullaniciID"]?.ToString();
            object sonGuncelleyenParam = (!string.IsNullOrEmpty(currentUser) && currentUser != userId) ? (object)currentUser : DBNull.Value;

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@KullaniciID", userId);
                cmd.Parameters.Add("@SifreHash", SqlDbType.VarBinary, 64).Value = hash;
                cmd.Parameters.AddWithValue("@Salt", saltString);
                cmd.Parameters.AddWithValue("@SonGuncelleyenKullaniciID", sonGuncelleyenParam);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Kullanıcı silme (soft delete)
        private void DeleteUser(string userId)
        {
            string updaterId = Session["KullaniciID"]?.ToString() ?? "A2AA6637-0732-4B63-88C4-D32B285B5E1B";
            string query = @"
                UPDATE Kullanicilar 
                SET SoftDelete = 1, 
                    SonGuncelleyenKullaniciID = @SonGuncelleyenKullaniciID, 
                    GuncellemeTarihi = GETDATE() 
                WHERE KullaniciID = @KullaniciID";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@KullaniciID", userId);
                cmd.Parameters.AddWithValue("@SonGuncelleyenKullaniciID", updaterId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Yardımcı Metotlar

        /// <summary>
        /// Şifre ve salt'ı kullanarak SHA256 hash hesaplar.
        /// </summary>
        private byte[] ComputeHash(string password, byte[] salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] pwdBytes = Encoding.UTF8.GetBytes(password);
                byte[] combined = salt.Concat(pwdBytes).ToArray();
                return sha256.ComputeHash(combined);
            }
        }

        /// <summary>
        /// Rastgele 16 baytlık salt üretir.
        /// </summary>
        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Türkçe formatlı string değeri decimal'a çevirir (örn. "1.234,56" -> 1234.56)
        /// </summary>
        private decimal ParseTurkishDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            value = value.Replace(".", "").Replace(",", ".");
            decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal result);
            return result;
        }

        /// <summary>
        /// Kullanıcıya SweetAlert ile mesaj gösterir ve (varsa) sayfayı yeniler.
        /// </summary>
        private void ShowMessage(string message, bool success)
        {
            string icon = success ? "success" : "error";
            ScriptManager.RegisterStartupScript(this, GetType(), "showMessage", $@"
                Swal.fire({{
                    title: '{(success ? "Başarılı!" : "Hata!")}',
                    text: '{message}',
                    icon: '{icon}',
                    confirmButtonText: 'Tamam'
                }}).then((result) => {{
                    if(result.isConfirmed) {{
                        window.location.reload();
                    }}
                }});", true);
        }

        #endregion
    }
}
