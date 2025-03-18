using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;



namespace MuhasebeStokDB.Pages
{
    public partial class StockMovements : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadStockMovements();
            }
        }

        private void LoadStockMovements(string search = "")
        {
            string query = @"
        SELECT 
            U.UrunAdi,  
            SH.Miktar, 
            SH.HareketTarihi, 
            SH.HareketTuru,
            SH.FaturaID, 
            SH.IrsaliyeID,
            SH.BirimFiyat,
            D.DepoAdi
        FROM StokHareketleri SH
        INNER JOIN Urunler U ON SH.UrunID = U.UrunID
        LEFT JOIN Depolar D ON SH.DepoID = D.DepoID
        WHERE SH.SoftDelete = 0 order by SH.HareketTarihi desc";

            if (!string.IsNullOrEmpty(search))
            {
                query += " AND (U.UrunAdi LIKE @Search OR D.DepoAdi LIKE @Search)";
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                if (!string.IsNullOrEmpty(search))
                {
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                }

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvStockMovements.DataSource = dt;
                gvStockMovements.DataBind();
            }
        }


        protected void gvStockMovements_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvStockMovements.PageIndex = e.NewPageIndex;
            LoadStockMovements(txtSearch.Text.Trim());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadStockMovements(txtSearch.Text.Trim());
        }
        protected void btnHareketKaydet_Click(object sender, EventArgs e)
        {
            string hareketTuru = ddlHareketTuru.SelectedValue;
            string urunAdi = txtUrunAdi.Text;
            decimal miktar = Convert.ToDecimal(txtMiktar.Text);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = @"
                    INSERT INTO StokHareketleri (UrunID, Miktar, HareketTuru, HareketTarihi, SoftDelete)
                    SELECT UrunID, @Miktar, @HareketTuru, GETDATE(), 0
                    FROM Urunler WHERE UrunAdi = @UrunAdi";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@HareketTuru", hareketTuru);
                cmd.Parameters.AddWithValue("@UrunAdi", urunAdi);
                cmd.Parameters.AddWithValue("@Miktar", miktar);

                cmd.ExecuteNonQuery();
            }

            LoadStockMovements();
        }
    }
}

