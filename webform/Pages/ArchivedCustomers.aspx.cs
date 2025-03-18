using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class ArchivedCustomers : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadArchivedCustomers();
            }
        }

        private void LoadArchivedCustomers(string sortExpression = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CariID, CariAdi, Telefon, Email FROM Cariler WHERE SoftDelete = 1";
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
                rptArchivedCustomers.DataSource = dt;
                rptArchivedCustomers.DataBind();
            }
        }

        [WebMethod]
        public static string PermanentDeleteCustomer(string cariID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM CariHareketler WHERE CariID = @CariID";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@CariID", cariID);
                conn.Open();
                int count = (int)checkCmd.ExecuteScalar();
                conn.Close();

                if (count > 0)
                {
                    // Cari hareketi varsa silinemez
                    return "İşlem yapılmış bir cariyi kalıcı olarak silemezsiniz.";
                }
                else
                {
                    // Cari hareketi yoksa silinebilir
                    string deleteQuery = "DELETE FROM Cariler WHERE CariID = @CariID";
                    SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                    deleteCmd.Parameters.AddWithValue("@CariID", cariID);
                    conn.Open();
                    deleteCmd.ExecuteNonQuery();
                    conn.Close();
                    return "Müşteri başarıyla kalıcı olarak silindi.";
                }
            }
        }

        protected void rptArchivedCustomers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string cariID = e.CommandArgument.ToString();

            if (e.CommandName == "Restore")
            {
                RestoreCustomer(cariID);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "restoreSuccess", "showAlert('Başarılı!', 'Müşteri başarıyla geri alındı.', 'success');", true);
                LoadArchivedCustomers();
            }
            else if (e.CommandName == "PermanentDelete")
            {
                string result = PermanentDeleteCustomer(cariID);
                string alertType = result.Contains("başarıyla") ? "success" : "error";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "permanentDeleteResult", $"showAlert('Sonuç', '{result}', '{alertType}');", true);
                LoadArchivedCustomers();
            }
        }

        [WebMethod]
        public static void RestoreCustomer(string cariID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Cariler SET SoftDelete = 0 WHERE CariID = @CariID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CariID", cariID);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}