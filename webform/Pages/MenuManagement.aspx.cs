using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaOaKApp.Pages
{
    public partial class MenuManagement : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadParentMenus();
                LoadMenus();
            }

            // Silme onayı için __EVENTTARGET kontrolü
            string eventTarget = Request["__EVENTTARGET"];
            string eventArgument = Request["__EVENTARGUMENT"];
            if (!string.IsNullOrEmpty(eventTarget) && eventTarget == "DeleteMenu" && !string.IsNullOrEmpty(eventArgument))
            {
                int menuID = Convert.ToInt32(eventArgument);
                try
                {
                    DeleteMenu(menuID);
                    LoadMenus();
                    ShowSweetAlert("Başarılı!", "Menü başarıyla silindi.", "success");
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Hata!", "Menü silinirken hata oluştu: " + ex.Message, "error");
                }
            }
        }

        /// <summary>
        /// Tüm menüleri (SoftDelete=0) getirir.
        /// Üst menünün adını da JOIN ile çekiyoruz.
        /// </summary>
        private void LoadMenus()
        {
            string query = @"
                SELECT M.MenuID, M.MenuAdi, M.MenuUrl, M.ParentMenuID, M.Sira, M.Aktif, M.Icon, M.SoftDelete,
                       ISNULL(P.MenuAdi, '') AS ParentMenuAdi
                FROM Menuler M
                LEFT JOIN Menuler P ON M.ParentMenuID = P.MenuID
                WHERE M.SoftDelete = 0
                ORDER BY M.Sira ASC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                rptMenus.DataSource = dt;
                rptMenus.DataBind();
            }
        }

        /// <summary>
        /// Üst menü seçeneklerini doldurur.
        /// </summary>
        private void LoadParentMenus()
        {
            string query = @"
                SELECT MenuID, MenuAdi
                FROM Menuler
                WHERE ParentMenuID IS NULL AND SoftDelete = 0
                ORDER BY Sira ASC";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                ddlParentMenu.DataSource = dt;
                ddlParentMenu.DataTextField = "MenuAdi";
                ddlParentMenu.DataValueField = "MenuID";
                ddlParentMenu.DataBind();
                ddlParentMenu.Items.Insert(0, new ListItem("(Üst Menü Yok)", ""));

                ddlEditParentMenu.DataSource = dt;
                ddlEditParentMenu.DataTextField = "MenuAdi";
                ddlEditParentMenu.DataValueField = "MenuID";
                ddlEditParentMenu.DataBind();
                ddlEditParentMenu.Items.Insert(0, new ListItem("(Üst Menü Yok)", ""));
            }
        }

        /// <summary>
        /// Yeni menü ekler (Menuler tablosuna).
        /// SoftDelete=0 olarak ekler.
        /// </summary>
        protected void btnAddMenu_Click(object sender, EventArgs e)
        {
            try
            {
                string menuAdi = txtMenuAdi.Text.Trim();
                string menuUrl = txtMenuUrl.Text.Trim();
                int sira = 0;
                int.TryParse(txtSira.Text.Trim(), out sira);
                bool aktif = chkAktif.Checked;
                string icon = ddlIcon.SelectedValue;

                int? parentID = null;
                if (!string.IsNullOrEmpty(ddlParentMenu.SelectedValue))
                    parentID = Convert.ToInt32(ddlParentMenu.SelectedValue);

                string insertSql = @"
                    INSERT INTO Menuler(MenuAdi, MenuUrl, ParentMenuID, Sira, Aktif, Icon, SoftDelete)
                    VALUES(@MenuAdi, @MenuUrl, @ParentMenuID, @Sira, @Aktif, @Icon, 0)";
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@MenuAdi", menuAdi);
                    cmd.Parameters.AddWithValue("@MenuUrl", menuUrl);
                    cmd.Parameters.AddWithValue("@Sira", sira);
                    cmd.Parameters.AddWithValue("@Aktif", aktif);
                    cmd.Parameters.AddWithValue("@Icon", icon);
                    if (parentID.HasValue)
                        cmd.Parameters.AddWithValue("@ParentMenuID", parentID.Value);
                    else
                        cmd.Parameters.AddWithValue("@ParentMenuID", DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                ClearAddMenuForm();
                LoadMenus();
                ShowSweetAlert("Başarılı!", "Menü başarıyla eklendi.", "success");
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Hata!", "Menü eklenirken hata oluştu: " + ex.Message, "error");
            }
        }

        /// <summary>
        /// Repeater içindeki menü işlemlerini yakalar (Düzenle, Sil).
        /// </summary>
        protected void rptMenus_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "EditMenu")
            {
                int menuID = Convert.ToInt32(e.CommandArgument);
                LoadMenuDetails(menuID);
                // Bootstrap 5 için modalı açma komutu
                ScriptManager.RegisterStartupScript(this, GetType(), "openEditModal",
                    "var myModal = new bootstrap.Modal(document.getElementById('editMenuModal')); myModal.show();", true);
            }
            else if (e.CommandName == "DeleteMenu")
            {
                int menuID = Convert.ToInt32(e.CommandArgument);
                try
                {
                    DeleteMenu(menuID);
                    LoadMenus();
                    ShowSweetAlert("Başarılı!", "Menü başarıyla silindi.", "success");
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Hata!", "Menü silinirken hata oluştu: " + ex.Message, "error");
                }
            }
        }

        /// <summary>
        /// Seçili menünün bilgilerini düzenleme modalına doldurur.
        /// </summary>
        private void LoadMenuDetails(int menuID)
        {
            string query = @"
                SELECT MenuID, MenuAdi, MenuUrl, ParentMenuID, Sira, Aktif, Icon
                FROM Menuler
                WHERE MenuID=@MenuID AND SoftDelete=0";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MenuID", menuID);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        hfMenuID.Value = rdr["MenuID"].ToString();
                        txtEditMenuAdi.Text = rdr["MenuAdi"].ToString();
                        txtEditMenuUrl.Text = rdr["MenuUrl"].ToString();
                        txtEditSira.Text = rdr["Sira"].ToString();
                        chkEditAktif.Checked = Convert.ToBoolean(rdr["Aktif"]);
                        string icon = rdr["Icon"] == DBNull.Value ? "" : rdr["Icon"].ToString();
                        if (ddlEditIcon.Items.FindByValue(icon) != null)
                            ddlEditIcon.SelectedValue = icon;
                        else
                            ddlEditIcon.SelectedIndex = 0;
                        if (rdr["ParentMenuID"] != DBNull.Value)
                            ddlEditParentMenu.SelectedValue = rdr["ParentMenuID"].ToString();
                        else
                            ddlEditParentMenu.SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Menü güncelleme işlemi.
        /// </summary>
        protected void btnUpdateMenu_Click(object sender, EventArgs e)
        {
            try
            {
                int menuID = Convert.ToInt32(hfMenuID.Value);
                string menuAdi = txtEditMenuAdi.Text.Trim();
                string menuUrl = txtEditMenuUrl.Text.Trim();
                int sira = 0;
                int.TryParse(txtEditSira.Text, out sira);
                bool aktif = chkEditAktif.Checked;
                string icon = ddlEditIcon.SelectedValue;

                int? parentID = null;
                if (!string.IsNullOrEmpty(ddlEditParentMenu.SelectedValue))
                    parentID = Convert.ToInt32(ddlEditParentMenu.SelectedValue);

                string updateSql = @"
                    UPDATE Menuler
                    SET MenuAdi=@MenuAdi, MenuUrl=@MenuUrl, ParentMenuID=@ParentMenuID,
                        Sira=@Sira, Aktif=@Aktif, Icon=@Icon
                    WHERE MenuID=@MenuID";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("@MenuAdi", menuAdi);
                    cmd.Parameters.AddWithValue("@MenuUrl", menuUrl);
                    cmd.Parameters.AddWithValue("@Sira", sira);
                    cmd.Parameters.AddWithValue("@Aktif", aktif);
                    cmd.Parameters.AddWithValue("@Icon", icon);
                    cmd.Parameters.AddWithValue("@MenuID", menuID);
                    if (parentID.HasValue)
                        cmd.Parameters.AddWithValue("@ParentMenuID", parentID.Value);
                    else
                        cmd.Parameters.AddWithValue("@ParentMenuID", DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                ScriptManager.RegisterStartupScript(this, GetType(), "closeEditModal",
                    "var myModal = bootstrap.Modal.getInstance(document.getElementById('editMenuModal')); if(myModal){ myModal.hide(); }", true);
                LoadMenus();
                ShowSweetAlert("Başarılı!", "Menü başarıyla güncellendi.", "success");
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Hata!", "Menü güncellenirken hata oluştu: " + ex.Message, "error");
            }
        }

        /// <summary>
        /// Menü silme işlemi: İlişkili RolMenuler kayıtlarını silip, Menuler'da soft delete yapar.
        /// </summary>
        private void DeleteMenu(int menuID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        string delRM = "DELETE FROM RolMenuler WHERE MenuID=@MenuID";
                        SqlCommand cmdRM = new SqlCommand(delRM, conn, tran);
                        cmdRM.Parameters.AddWithValue("@MenuID", menuID);
                        cmdRM.ExecuteNonQuery();

                        string updMenu = "UPDATE Menuler SET SoftDelete=1 WHERE MenuID=@MenuID";
                        SqlCommand cmdMenu = new SqlCommand(updMenu, conn, tran);
                        cmdMenu.Parameters.AddWithValue("@MenuID", menuID);
                        cmdMenu.ExecuteNonQuery();

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Yeni Menü ekleme modalındaki formu temizler.
        /// </summary>
        private void ClearAddMenuForm()
        {
            txtMenuAdi.Text = "";
            txtMenuUrl.Text = "";
            txtSira.Text = "0";
            chkAktif.Checked = true;
            ddlParentMenu.SelectedIndex = 0;
            ddlIcon.SelectedIndex = 0;
        }

        /// <summary>
        /// SweetAlert ile mesaj gösterir.
        /// </summary>
        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
                Swal.fire({{
                    title: '{title}',
                    text: '{message}',
                    icon: '{icon}',
                    confirmButtonText: 'Tamam'
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, true);
        }
    }
}