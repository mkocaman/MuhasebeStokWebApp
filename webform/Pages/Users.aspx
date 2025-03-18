<%@ Page Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="MaOaKApp.Pages.Users" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <div class="container mt-5">
        <div class="card">
            <div class="card-header d-flex justify-content-between">
                <h4 class="mb-0">Kullanıcılar</h4>
                <!-- Yeni kullanıcı ekleme butonu -->
                <asp:Button ID="btnAddUser" runat="server" Text="Yeni Kullanıcı" CssClass="btn btn-primary" OnClientClick="$('#AddUserModal').modal('show'); return false;" />
            </div>
            <div class="card-body">
                <div class="table-responsive">
                    <table class="table table-striped" id="basic-1">
                        <thead>
                            <tr>
                                <th>Kullanıcı Adı</th>
                                <th>E-posta</th>
                                <th>Rol</th>
                                <th>Durum</th>
                                <th>İşlemler</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptUsers" runat="server" OnItemCommand="rptUsers_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td><%# Eval("KullaniciAdi") %></td>
                                        <td><%# Eval("Email") %></td>
                                        <td><%# Eval("RolAdi") %></td>
                                        <td><%# Eval("Durum") %></td>
                                        <td>
                                            <asp:Button ID="btnEditUser" runat="server" Text="Düzenle" CssClass="btn btn-outline-primary" CommandName="EditUser" CommandArgument='<%# Eval("KullaniciID") %>' />
                                            <asp:Button ID="btnResetPassword" runat="server" Text="Şifre Sıfırla" CssClass="btn btn-outline-secondary" CommandName="ResetPassword" CommandArgument='<%# Eval("KullaniciID") %>' />
                                            <asp:Button ID="btnDeleteUser" runat="server" Text="Sil" CssClass="btn btn-outline-danger" CommandName="DeleteUser" CommandArgument='<%# Eval("KullaniciID") %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <!-- Kullanıcı Ekleme Modalı -->
    <asp:UpdatePanel ID="upAddUserModal" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="modal fade" id="AddUserModal" tabindex="-1" role="dialog" aria-labelledby="AddUserModalLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="AddUserModalLabel">Yeni Kullanıcı Ekle</h5>
                            <button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Kapat"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label for="txtKullaniciAdi" class="form-label">Kullanıcı Adı</label>
                                <asp:TextBox ID="txtKullaniciAdi" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="txtEmail" class="form-label">E-posta</label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="txtPassword" class="form-label">Şifre</label>
                                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="ddladdRoles" class="form-label">Rol</label>
                                <asp:DropDownList ID="ddladdRoles" runat="server" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button class="btn btn-primary" type="button" data-bs-dismiss="modal">Kapat</button>
                            <asp:Button ID="btnSaveUser" runat="server" Text="Kaydet" CssClass="btn btn-success" OnClick="btnAddUser_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Kullanıcı Güncelleme Modalı -->
    <asp:UpdatePanel ID="upEditUserModal" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField ID="hfEditUserId" runat="server" />
            <div class="modal fade" id="editUserModal" tabindex="-1" role="dialog" aria-labelledby="editUserModalLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="editUserModalLabel">Kullanıcı Güncelle</h5>
                            <button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Kapat"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label for="txtEditKullaniciAdi" class="form-label">Kullanıcı Adı</label>
                                <asp:TextBox ID="txtEditKullaniciAdi" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="txtEditEmail" class="form-label">E-posta</label>
                                <asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="ddlEditRol" class="form-label">Rol</label>
                                <asp:DropDownList ID="ddlEditRol" runat="server" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button class="btn btn-danger" type="button" data-bs-dismiss="modal">Kapat</button>
                            <asp:Button ID="btnSaveChanges" runat="server" Text="Kaydet" CssClass="btn btn-success" OnClick="btnSaveChanges_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="rptUsers" EventName="ItemCommand" />
        </Triggers>
    </asp:UpdatePanel>

    <!-- İlgili JavaScript kodları -->
    <script type="text/javascript">
        $(document).ready(function () {
            $('#basic-1').DataTable({
                "order": [[0, "asc"]],
                "language": { "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Turkish.json" }
            });
        });
    </script>
</asp:Content>