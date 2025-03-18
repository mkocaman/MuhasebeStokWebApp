using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace MuhasebeStokDB.Pages
{
    public partial class DeliveryNotes : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDeliveryNotes();
            }
        }

        private void LoadDeliveryNotes()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT DN.IrsaliyeID, DN.IrsaliyeNumarasi, C.CariAdi, DN.IrsaliyeTarihi, DN.FaturaID
                FROM Irsaliyeler DN
                LEFT JOIN Faturalar F ON DN.FaturaID = F.FaturaID
                LEFT JOIN Cariler C ON DN.CariID = C.CariID order by DN.IrsaliyeNumarasi desc";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptDeliveryNotes.DataSource = dt;
                rptDeliveryNotes.DataBind();
            }
        }

        protected void rptDeliveryNotes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string deliveryNoteID = e.CommandArgument.ToString();

            if (e.CommandName == "CreateInvoice")
            {
                Response.Redirect($"NewInvoice.aspx?DeliveryNoteID={deliveryNoteID}");
            }
            else if (e.CommandName == "EditDeliveryNote")
            {
                Response.Redirect($"EditDeliveryNote.aspx?DeliveryNoteID={deliveryNoteID}");
            }
            else if (e.CommandName == "ViewDeliveryNote")
            {
                Response.Redirect($"DeliveryNoteDetails.aspx?DeliveryNoteID={deliveryNoteID}");
            }
            else if (e.CommandName == "DeleteDeliveryNote")
            {
                DeleteDeliveryNote(deliveryNoteID);
                LoadDeliveryNotes(); // Repeater'ı yenile
            }
        }

        private void DeleteDeliveryNote(string deliveryNoteID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Irsaliyeler WHERE IrsaliyeID = @IrsaliyeID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@IrsaliyeID", deliveryNoteID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}