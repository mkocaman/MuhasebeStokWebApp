<%@ Page Title="Silinmiş Müşteriler" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="ArchivedCustomers.aspx.cs" Inherits="MaOaKApp.Pages.ArchivedCustomers" %>
<%@ Import Namespace="System.Web.Services" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>

    <!-- Gerekli CSS dosyaları -->
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet">

    <div class="container mt-5">
        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h4 class="mb-0">Silinmiş Müşteriler</h4>
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
                                <asp:Repeater ID="rptArchivedCustomers" runat="server" OnItemCommand="rptArchivedCustomers_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("CariAdi") %></td>
                                            <td><%# Eval("Telefon") %></td>
                                            <td><%# Eval("Email") %></td>
                                            <td>
                                                <asp:Button runat="server" CssClass="btn btn-outline-success btn-sm" 
                                                            Text="Geri Al" 
                                                            CommandName="Restore" 
                                                            CommandArgument='<%# Eval("CariID") %>' />
                                                <asp:Button runat="server" CssClass="btn btn-outline-danger btn-sm" 
                                                            Text="Kalıcı Sil" 
                                                            CommandName="PermanentDelete" 
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

    <!-- Gerekli JS dosyaları -->
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/@popperjs/core@2.5.4/dist/umd/popper.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@10"></script>

    <script>
        function showAlert(title, text, icon) {
            Swal.fire({
                title: title,
                text: text,
                icon: icon,
                confirmButtonText: 'Tamam'
            });
        }
    </script>
</asp:Content>