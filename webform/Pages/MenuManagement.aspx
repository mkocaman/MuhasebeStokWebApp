<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MenuManagement.aspx.cs" Inherits="MaOaKApp.Pages.MenuManagement" MasterPageFile="~/Material.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- SweetAlert2 script (CDN üzerinden) -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/js/bootstrap.bundle.min.js"></script>
    <h2>Menü Yönetimi</h2>

    <!-- "Yeni Menü Ekle" butonu, sayfanın sağ üstünde -->
    <div style="text-align:right; margin-bottom:10px;">
        <asp:Button ID="btnShowAddModal" runat="server" Text="Yeni Menü Ekle" CssClass="btn btn-success" 
                    OnClientClick="$('#addMenuModal').modal('show'); return false;" />
    </div>

    <!-- Repeater ile Menü Listesi -->
    <asp:Repeater ID="rptMenus" runat="server" OnItemCommand="rptMenus_ItemCommand">
        <HeaderTemplate>
            <table class="table table-bordered" id="basic-1">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Menü Adı</th>
                        <th>URL</th>
                        <th>Üst Menü</th>
                        <th>Sıra</th>
                        <th>Aktif</th>
                        <th>İkon</th>
                        <th>İşlemler</th>
                    </tr>
                </thead>
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td><%# Eval("MenuID") %></td>
                <td><%# Eval("MenuAdi") %></td>
                <td><%# Eval("MenuUrl") %></td>
                <td><%# Eval("ParentMenuAdi") %></td>
                <td><%# Eval("Sira") %></td>
                <td>
                    <asp:CheckBox ID="chkAktifDisplay" runat="server" 
                        Checked='<%# Convert.ToBoolean(Eval("Aktif")) %>' Enabled="false" />
                </td>
                <td>
                    <i class='<%# Eval("Icon") %>'></i> 
                </td>
                <td>
                    <asp:LinkButton ID="lnkEdit" runat="server" Text="Düzenle" CssClass="btn btn-warning btn-sm"
                        CommandName="EditMenu" CommandArgument='<%# Eval("MenuID") %>' />
                    <asp:LinkButton ID="lnkDelete" runat="server" Text="Sil" CssClass="btn btn-danger btn-sm"
                        CommandName="DeleteMenu" CommandArgument='<%# Eval("MenuID") %>'
                        OnClientClick='<%# "return confirmDeleteMenu(" + Eval("MenuID") + ");" %>' />
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
                </tbody>
            </table>
        </FooterTemplate>
    </asp:Repeater>

    <!-- Add Menu Modal -->
    <div class="modal fade" id="addMenuModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Yeni Menü Ekle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Kapat"></button>
                </div>
                <div class="modal-body">
                    <asp:Panel ID="pnlAddMenu" runat="server">
                        <div class="form-group">
                            <label>Menü Adı:</label>
                            <asp:TextBox ID="txtMenuAdi" runat="server" CssClass="form-control" />
                        </div>
                        <div class="form-group">
                            <label>Menü URL:</label>
                            <asp:TextBox ID="txtMenuUrl" runat="server" CssClass="form-control" />
                        </div>
                        <div class="form-group">
                            <label>Üst Menü:</label>
                            <asp:DropDownList ID="ddlParentMenu" runat="server" CssClass="form-control">
                                <asp:ListItem Text="(Üst Menü Yok)" Value="" />
                            </asp:DropDownList>
                        </div>
                        <div class="form-group">
                            <label>Sıra:</label>
                            <asp:TextBox ID="txtSira" runat="server" CssClass="form-control" Text="0" />
                        </div>
                        <div class="form-group">
                            <label>Aktif:</label>
                            <asp:CheckBox ID="chkAktif" runat="server" Checked="true" />
                        </div>
                        <div class="form-group">
                            <label>İkon Seçimi:</label>
                            <asp:DropDownList ID="ddlIcon" runat="server" CssClass="form-control">
                              
                                <asp:ListItem Text="Gelir" Value="fa fa-download" />
                                <asp:ListItem Text="Gider" Value="fa fa-upload" />
                                <asp:ListItem Text="Nakit" Value="fa fa-money-bill" />
                                <asp:ListItem Text="Stok" Value="fa fa-cubes" />
                                <asp:ListItem Text="Ayarlar" Value="fa fa-cog" />
                                <asp:ListItem Text="Kullanıcı" Value="fa fa-user" />
                                <asp:ListItem Text="Ev" Value="fa fa-home" />
                                <asp:ListItem Text="Grafik" Value="fa fa-chart-bar" />
                                <asp:ListItem Text="Bilgi" Value="fa fa-info-circle" />
                                <asp:ListItem Text="Mesaj" Value="fa fa-envelope" />
                                <asp:ListItem Text="Bildirim" Value="fa fa-bell" />
                                <asp:ListItem Text="Ayarlar 2" Value="fa fa-sliders-h" />
                                <asp:ListItem Text="Zaman" Value="fa fa-clock" />
                                <asp:ListItem Text="Arama" Value="fa fa-search" />
                                <asp:ListItem Text="Favori" Value="fa fa-star" />
                                <asp:ListItem Text="Sil" Value="fa fa-trash" />
                                <asp:ListItem Text="Düzenle" Value="fa fa-edit" />
                                <asp:ListItem Text="Dosya" Value="fa fa-file" />
                                <asp:ListItem Text="Klasör" Value="fa fa-folder" />
                            </asp:DropDownList>
                        </div>
                    </asp:Panel>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnAddMenu" runat="server" Text="Kaydet" CssClass="btn btn-success"
                        OnClick="btnAddMenu_Click" />
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Edit Menu Modal -->
    <div class="modal fade" id="editMenuModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Menü Düzenle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Kapat"></button>
                </div>
                <div class="modal-body">
                    <asp:Panel ID="pnlEditMenu" runat="server">
                        <asp:HiddenField ID="hfMenuID" runat="server" />
                        <div class="form-group">
                            <label>Menü Adı:</label>
                            <asp:TextBox ID="txtEditMenuAdi" runat="server" CssClass="form-control" />
                        </div>
                        <div class="form-group">
                            <label>Menü URL:</label>
                            <asp:TextBox ID="txtEditMenuUrl" runat="server" CssClass="form-control" />
                        </div>
                        <div class="form-group">
                            <label>Üst Menü:</label>
                            <asp:DropDownList ID="ddlEditParentMenu" runat="server" CssClass="form-control">
                                <asp:ListItem Text="(Üst Menü Yok)" Value="" />
                            </asp:DropDownList>
                        </div>
                        <div class="form-group">
                            <label>Sıra:</label>
                            <asp:TextBox ID="txtEditSira" runat="server" CssClass="form-control" />
                        </div>
                        <div class="form-group">
                            <label>Aktif:</label>
                            <asp:CheckBox ID="chkEditAktif" runat="server" />
                        </div>
                        <div class="form-group">
                            <label>İkon Seçimi:</label>
                            <asp:DropDownList ID="ddlEditIcon" runat="server" CssClass="form-control">
                                <asp:ListItem Text="Gelir" Value="fa fa-download" />
                                <asp:ListItem Text="Gider" Value="fa fa-upload" />
                                <asp:ListItem Text="Nakit" Value="fa fa-money-bill" />
                                <asp:ListItem Text="Stok" Value="fa fa-cubes" />
                                <asp:ListItem Text="Ayarlar" Value="fa fa-cog" />
                                <asp:ListItem Text="Kullanıcı" Value="fa fa-user" />
                                <asp:ListItem Text="Ev" Value="fa fa-home" />
                                <asp:ListItem Text="Grafik" Value="fa fa-chart-bar" />
                                <asp:ListItem Text="Bilgi" Value="fa fa-info-circle" />
                                <asp:ListItem Text="Mesaj" Value="fa fa-envelope" />
                                <asp:ListItem Text="Bildirim" Value="fa fa-bell" />
                                <asp:ListItem Text="Ayarlar 2" Value="fa fa-sliders-h" />
                                <asp:ListItem Text="Zaman" Value="fa fa-clock" />
                                <asp:ListItem Text="Arama" Value="fa fa-search" />
                                <asp:ListItem Text="Favori" Value="fa fa-star" />
                                <asp:ListItem Text="Sil" Value="fa fa-trash" />
                                <asp:ListItem Text="Düzenle" Value="fa fa-edit" />
                                <asp:ListItem Text="Dosya" Value="fa fa-file" />
                                <asp:ListItem Text="Klasör" Value="fa fa-folder" />
                            </asp:DropDownList>
                        </div>
                    </asp:Panel>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnUpdateMenu" runat="server" Text="Güncelle" CssClass="btn btn-warning"
                        OnClick="btnUpdateMenu_Click" />
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
                </div>
            </div>
        </div>
    </div>

    <!-- JavaScript: Silme onayı için SweetAlert -->
    <script type="text/javascript">
        function confirmDeleteMenu(menuID) {
            Swal.fire({
                title: 'Emin misiniz?',
                text: 'Bu menüyü silmek istediğinize emin misiniz?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Evet, sil!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('DeleteMenu', menuID);
                }
            });
            return false;
        }

        function showEditMenuModal() {
            var $modal = $('#editMenuModal');
            if ($modal.length) {
                $modal.modal('show');
            }
        }
    </script>
</asp:Content>