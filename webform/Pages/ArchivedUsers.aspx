<%@ Page Title="Arşivlenmiş Kullanıcılar" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="ArchivedUsers.aspx.cs" Inherits="MaOaKApp.Pages.ArchivedUsers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-5">
        <div class="card">
            <div class="card-header">
                <h4 class="mb-0">Arşivlenmiş Kullanıcılar</h4>
            </div>
            <div class="card-body">
                <table class="table table-striped" id="basic-1">
                    <thead>
                        <tr>
                            <th>Kullanıcı Adı</th>
                            <th>E-posta</th>
                            <th>Rol</th>
                            <th>Oluşturma Tarihi</th>
                            <th>İşlemler</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptArchivedUsers" runat="server" OnItemCommand="rptArchivedUsers_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("KullaniciAdi") %></td>
                                    <td><%# Eval("Email") %></td>
                                    <td><%# Eval("RolAdi") %></td>
                                    <td><%# Eval("OlusturmaTarihi", "{0:dd/MM/yyyy}") %></td>
                                    <td>
                                        <asp:Button runat="server" CssClass="btn btn-outline-primary-2x" 
                                                    Text="Geri Yükle" 
                                                    CommandName="RestoreUser" 
                                                    CommandArgument='<%# Eval("KullaniciID") %>' />
                                        <asp:Button runat="server" CssClass="btn btn-outline-danger-2x" 
                                                    Text="Kalıcı Sil" 
                                                    CommandName="PermanentDeleteUser" 
                                                    CommandArgument='<%# Eval("KullaniciID") %>' 
                                                    OnClientClick='<%# "showPermanentDeleteModal(\"" + Eval("KullaniciAdi") + "\", \"" + Eval("KullaniciID") + "\"); return false;" %>' />
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- Kalıcı Silme Modalı -->
    <div class="modal fade" id="permanentDeleteModal" tabindex="-1" aria-labelledby="permanentDeleteModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="permanentDeleteModalLabel">Kalıcı Silme Onayı</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p id="permanentDeleteMessage">Bu kullanıcıyı kalıcı olarak silmek istediğinizden emin misiniz?</p>
                    <asp:HiddenField ID="hfPermanentDeleteUserId" runat="server" />
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnConfirmPermanentDelete" runat="server" Text="Evet, Sil" CssClass="btn btn-danger"
    OnClick="btnConfirmPermanentDelete_Click" />

                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Vazgeç</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Başarı Mesajı Modalı -->
    <div class="modal fade" id="successModal" tabindex="-1" aria-labelledby="successModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="successModalLabel">Başarılı!</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p id="successMessage">İşlem başarıyla tamamlandı.</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Tamam</button>
                </div>
            </div>
        </div>
    </div>

    <script>
        function showPermanentDeleteModal(userName, userId) {
            document.getElementById('permanentDeleteMessage').innerText =
                userName + " isimli kullanıcıyı kalıcı olarak silmek istediğinizden emin misiniz?";
            document.getElementById('<%= hfPermanentDeleteUserId.ClientID %>').value = userId;
            $('#permanentDeleteModal').modal('show');
        }

        function showSuccessModal(message) {
            document.getElementById('successMessage').innerText = message;
            $('#successModal').modal('show');
        }
    </script>
</asp:Content>
