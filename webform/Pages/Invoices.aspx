<%@ Page Title="Faturalar" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="Invoices.aspx.cs" Inherits="MuhasebeStokDB.Pages.Invoices" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Sayfanın üst kısmına sağa hizalanmış Yeni Fatura Oluştur butonu ekleniyor -->
        <div class="d-flex justify-content-between align-items-center">
            <h3 class="mt-4">Faturalar</h3>
            <a href="NewInvoice.aspx" class="btn btn-primary">Yeni Fatura Oluştur</a>
        </div>

        <div class="card mt-4">
            <div class="card-body">
                <div class="table-responsive">
                    <table class="table table-striped" id="basic-1">
                        <thead>
                            <tr>
                                <th>Fatura Numarası</th>
                                <th>Cari Adı</th>
                                <th>Fatura Tarihi</th>
                                <th>Genel Toplam</th>
                                <th>İşlemler</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptInvoices" runat="server" OnItemCommand="rptInvoices_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td><%# Eval("FaturaNumarasi") %></td>
                                        <td><%# Eval("CariAdi") %></td>
                                        <td><%# Eval("FaturaTarihi", "{0:dd.MM.yyyy}") %></td>
                                        <!-- GenelToplam sütunu, FormatTotal metoduyla 123.345,56 formatında ve döviz sembolü ile gösterilecek -->
                                        <td><%# FormatTotal(Eval("GenelToplam"), Eval("DovizTuru")) %></td>
                                        <td>
                                            <asp:Button ID="btnEdit" runat="server" CommandName="EditInvoice" CommandArgument='<%# Eval("FaturaID") %>' Text="Düzenle" CssClass="btn btn-outline-primary-2x btn-sm" />
                                            <asp:Button ID="btnView" runat="server" CommandName="ViewInvoice" CommandArgument='<%# Eval("FaturaID") %>' Text="Görüntüle" CssClass="btn btn-outline-info-2x btn-sm" />
                                            <asp:Button ID="btnDelete" runat="server" CommandName="DeleteInvoice" CommandArgument='<%# Eval("FaturaID") %>' Text="Sil" CssClass="btn btn-outline-danger-2x btn-sm" OnClientClick="return confirm('Bu faturayı silmek istediğinizden emin misiniz?');" />
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

    <script>
        $(document).ready(function () {
            $('#basic-1').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/tr.json'
                },
                paging: true,
                searching: true,
                info: true,
                responsive: true
            });
        });
    </script>
</asp:Content>
