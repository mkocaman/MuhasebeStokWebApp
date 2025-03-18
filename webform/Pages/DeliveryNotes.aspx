<%@ Page Title="İrsaliyeler" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="DeliveryNotes.aspx.cs" Inherits="MuhasebeStokDB.Pages.DeliveryNotes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h3 class="mt-4">İrsaliyeler</h3>
        <div class="card mt-4">
            <div class="card-body">
                <div class="table-responsive">
                    <table class="table table-striped" id="basic-1">
                        <thead>
                            <tr>
                                
                           
                                <th>Cari Adı</th>
                                <th>İrsaliye Numarası</th>
                                <th>İrsaliye Tarihi</th>
                             
                                <th>İşlemler</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptDeliveryNotes" runat="server" OnItemCommand="rptDeliveryNotes_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td><%# Eval("CariAdi") %></td>
                                        <td><%# Eval("IrsaliyeNumarasi") %></td>
    
                                        <td><%# Eval("IrsaliyeTarihi", "{0:dd.MM.yyyy}") %></td>
                                        <td>
                                            <asp:Button ID="btnEdit" runat="server" CommandName="EditDeliveryNote" CommandArgument='<%# Eval("IrsaliyeID") %>' Text="Düzenle" CssClass="btn btn-outline-primary-2x btn-sm" />
                                            <asp:Button ID="btnView" runat="server" CommandName="ViewDeliveryNote" CommandArgument='<%# Eval("IrsaliyeID") %>' Text="Görüntüle" CssClass="btn btn-outline-info-2x btn-sm" />
                                            <asp:Button ID="btnDelete" runat="server" CommandName="DeleteDeliveryNote" CommandArgument='<%# Eval("IrsaliyeID") %>' Text="Sil" CssClass="btn btn-outline-danger-2x btn-sm" OnClientClick="return confirm('Bu irsaliyeyi silmek istediğinizden emin misiniz?');" />
                                            <asp:Button ID="btnCreateInvoice" runat="server" Text="Fatura" CommandName="CreateInvoice" CommandArgument='<%# Eval("IrsaliyeID") %>' Visible='<%# Eval("FaturaID") == DBNull.Value %>' CssClass="btn btn-success btn-sm" />
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
            $('#deliveryNotesTable').DataTable({
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