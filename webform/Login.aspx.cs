using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace MaOaKApp
{
    public partial class Login : System.Web.UI.Page
    {
        // Web.config’de tanımlı bağlantı dizesi
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Kullanıcı adı ve şifre boş bırakılamaz.";
                lblMessage.Visible = true;
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Aktif ve SoftDelete kontrolü dahil
                string query = "SELECT KullaniciID, SifreHash, Salt, Aktif, Rol FROM Kullanicilar WHERE KullaniciAdi=@username AND SoftDelete=0";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        bool aktif = Convert.ToBoolean(reader["Aktif"]);
                        if (!aktif)
                        {
                            lblMessage.Text = "Hesabınız devre dışı bırakılmış.";
                        }
                        else
                        {
                            // Veritabanından gelen hash ve salt değerlerini alıyoruz
                            byte[] dbHash = (byte[])reader["SifreHash"];  // VarBinary olarak saklandığından direk alıyoruz.
                            string saltBase64 = reader["Salt"].ToString();
                            byte[] salt = Convert.FromBase64String(saltBase64);

                            byte[] inputHash = ComputeHash(password, salt);
                            if (dbHash.SequenceEqual(inputHash))
                            {
                                Session["KullaniciID"] = reader["KullaniciID"].ToString();
                                Session["KullaniciAdi"] = username;
                                Session["Rol"] = reader["Rol"].ToString();
                                Response.Redirect("/Pages/Dashboard.aspx");
                            }
                            else
                            {
                                lblMessage.Text = "Hatalı kullanıcı adı veya şifre.";
                            }
                        }
                    }
                    else
                    {
                        lblMessage.Text = "Hatalı kullanıcı adı veya şifre.";
                    }
                    lblMessage.Visible = true;
                }
            }
        }

        /// <summary>
        /// Verilen şifreyi, verilen salt ile birleştirip SHA256 hash hesaplar.
        /// </summary>
        private byte[] ComputeHash(string password, byte[] salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] pwdBytes = Encoding.UTF8.GetBytes(password);
                // Salt + şifre sırasıyla birleştiriyoruz
                byte[] combined = salt.Concat(pwdBytes).ToArray();
                return sha256.ComputeHash(combined);
            }
        }

        // Aşağıdaki metotlar (TryConvertFromBase64OrHex, HexStringToByteArray) örnek olarak verilmiştir.
        // Veritabanında hash/salt VarBinary olarak saklanıyorsa, bu metotlara gerek olmayabilir.
        private byte[] TryConvertFromBase64OrHex(string value)
        {
            try
            {
                return Convert.FromBase64String(value);
            }
            catch (FormatException)
            {
                return HexStringToByteArray(value);
            }
        }

        private byte[] HexStringToByteArray(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return new byte[0];

            hex = Regex.Replace(hex, @"\s+", "");
            hex = Regex.Replace(hex, @"[^0-9A-Fa-f]", "");

            if (hex.Length % 2 != 0)
                hex = "0" + hex;

            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
