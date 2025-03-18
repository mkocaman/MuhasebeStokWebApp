using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace MaOaKApp
{
    public partial class Material : System.Web.UI.MasterPage
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadMainMenus();
            }
        }

        /// <summary>
        /// Üst menüleri getirir ve rptMainMenu'ya bağlar.
        /// </summary>
        private void LoadMainMenus()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Örnek: Üst Menü = ParentMenuID IS NULL AND Aktif=1 ve soft-delete yapılmamış menüler
                string query = @"
                    SELECT MenuID, MenuAdi, MenuUrl, ParentMenuID, Sira, Aktif, Icon
                    FROM Menuler
                    WHERE ParentMenuID IS NULL AND Aktif = 1 AND SoftDelete = 0
                    ORDER BY Sira ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                rptMainMenu.DataSource = dt;
                rptMainMenu.DataBind();
            }
        }

        /// <summary>
        /// rptMainMenu_ItemDataBound ile alt menüleri doldurur.
        /// </summary>
        protected void rptMainMenu_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Ana menü satırındaki veriyi çek
                var drv = (DataRowView)e.Item.DataItem;
                int menuID = Convert.ToInt32(drv["MenuID"]);

                // Alt menü repeater'ını bul
                Repeater rptSubMenu = (Repeater)e.Item.FindControl("rptSubMenu");
                if (rptSubMenu != null)
                {
                    DataTable dtSub = GetSubMenus(menuID);
                    if (dtSub.Rows.Count > 0)
                    {
                        rptSubMenu.DataSource = dtSub;
                        rptSubMenu.DataBind();
                    }
                    else
                    {
                        // Alt menü yoksa repeater'ı gizle
                        rptSubMenu.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Belirtilen parentID'ye ve oturumdaki RolID'ye ait alt menüleri çeker.
        /// </summary>
        private DataTable GetSubMenus(int parentID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Rol filtresi kaldırıldı, tüm alt menüler getiriliyor.
                string query = @"
            SELECT M.MenuID, M.MenuAdi, M.MenuUrl, M.Icon
            FROM Menuler M
            WHERE M.ParentMenuID = @ParentID AND M.SoftDelete = 0 AND M.Aktif = 1
            ORDER BY M.Sira ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ParentID", parentID);
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                return dt;
            }
        }
    }
}