<%@ Page Title="Müşteriler" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="Customers.aspx.cs" Inherits="MaOaKApp.Pages.Customers" %>
<%@ Import Namespace="System.Web.Services" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <!-- Gerekli CSS dosyaları -->
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet">

    <div class="container mt-5">
        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h4 class="mb-0">Müşteriler</h4>
                <div>
                    <button class="btn btn-primary" type="button" onclick="showAddCustomerModal()">Müşteri Ekle</button>
                    <a href="ArchivedCustomers.aspx" class="btn btn-secondary" role="button">Silinmiş Müşteriler</a>
                </div>
            </div>
            <div class="card-body">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <table class="table table-striped" id="basic-1">
                            <thead>
                                <tr>
                                    <th>Müşteri Adı</th>
                                    <th>Telefon</th>
                                    <th>E-posta</th>
                                    <th>İşlemler</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptCustomers" runat="server" OnItemCommand="rptCustomers_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("CariAdi") %></td>
                                            <td><%# Eval("Telefon") %></td>
                                            <td><%# Eval("Email") %></td>
                                            <td>
                                                <asp:Button runat="server" CssClass="btn btn-outline-primary btn-sm" 
                                                            Text="Detaylar" 
                                                            CommandName="Details" 
                                                            CommandArgument='<%# Eval("CariID") %>' />
                                                <asp:Button runat="server" CssClass="btn btn-outline-secondary btn-sm" 
                                                            Text="Düzenle" 
                                                            CommandName="Edit" 
                                                            CommandArgument='<%# Eval("CariID") %>' />
                                                <asp:Button runat="server" CssClass="btn btn-outline-info btn-sm" 
                                                            Text="Cari Ekstre" 
                                                            CommandName="Statement" 
                                                            CommandArgument='<%# Eval("CariID") %>' />
                                                <asp:Button runat="server" CssClass="btn btn-outline-danger btn-sm" 
                                                            Text="Sil" 
                                                            CommandName="Delete" 
                                                            CommandArgument='<%# Eval("CariID") %>' />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <!-- Müşteri Detayları Modalı -->
    <asp:UpdatePanel ID="UpdatePanelDetails" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="modal fade" id="customerDetailsModal" tabindex="-1" aria-labelledby="customerDetailsModalLabel" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="customerDetailsModalLabel">Müşteri Detayları</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <asp:Label ID="lblCustomerDetails" runat="server" />
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Müşteri Silme Modalı -->
    <asp:UpdatePanel ID="UpdatePanelDelete" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="modal fade" id="deleteModal" tabindex="-1" aria-labelledby="deleteModalLabel" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="deleteModalLabel">Silme Onayı</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <p id="deleteMessage">Bu müşteriyi silmek istediğinizden emin misiniz?</p>
                            <asp:HiddenField ID="hfDeleteCustomerId" runat="server" />
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnConfirmDelete" runat="server" Text="Evet, Sil" CssClass="btn btn-danger"
                                        OnClick="btnConfirmDelete_Click" />
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Vazgeç</button>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Müşteri Ekle/Düzenle Modalı -->
    <asp:UpdatePanel ID="UpdatePanelAddEdit" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="modal fade" id="addCustomerModal" tabindex="-1" aria-labelledby="addCustomerModalLabel" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="addCustomerModalLabel">Müşteri Ekle/Düzenle</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label for="txtCustomerName" class="form-label">Müşteri Adı</label>
                                <asp:TextBox ID="txtCustomerName" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="txtVergiNo" class="form-label">Vergi No</label>
                                <asp:TextBox ID="txtVergiNo" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="txtYetkili" class="form-label">Firma Yetkilisi</label>
                                <asp:TextBox ID="txtYetkili" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="txtTelefon" class="form-label">Telefon</label>
                                <asp:TextBox ID="txtTelefon" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="txtEmail" class="form-label">E-posta</label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="txtAdres" class="form-label">Adres</label>
                                <asp:TextBox ID="txtAdres" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label for="txtAciklama" class="form-label">Açıklama</label>
                                <asp:TextBox ID="txtAciklama" runat="server" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnSaveCustomer" runat="server" Text="Kaydet" CssClass="btn btn-primary"
                                        OnClick="btnSaveCustomer_Click" />
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Vazgeç</button>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Gerekli JS dosyaları -->
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/@popperjs/core@2.5.4/dist/umd/popper.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>
    
    <!-- DataTables Türkçe Dil Ayarı için JS -->
    <script src="https://cdn.datatables.net/1.11.5/js/jquery.dataTables.min.js"></script>
    <script>
        $(document).ready(function () {
            $('#basic-1').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/tr.json'
                }
            });
        });

        function showDeleteModal(customerName, customerId) {
            document.getElementById('deleteMessage').innerText =
                customerName + " isimli müşteriyi silmek istediğinizden emin misiniz?";
            document.getElementById('<%= hfDeleteCustomerId.ClientID %>').value = customerId;
            $('#deleteModal').modal('show');
        }

        function showAddCustomerModal() {
            $('#addCustomerModal').modal('show');
        }
    </script>
</asp:Content>