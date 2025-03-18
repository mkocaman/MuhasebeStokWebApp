<%@ Page Title="Customer Statement" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="CustomerStatement.aspx.cs" Inherits="MaOaKApp.Pages.CustomerStatement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <!-- Gerekli CSS dosyaları -->
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.datatables.net/1.11.5/css/jquery.dataTables.min.css" rel="stylesheet" />

    <div class="container mt-5">
        <!-- Müşterinin adı, cari hesap ekstresi başlığında gösterilecek -->
        <asp:Label ID="lblCustomerName" runat="server" CssClass="h3 mb-3"></asp:Label>

        <asp:UpdatePanel ID="UpdatePanelStatement" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <table class="table table-striped" id="statementTable">
                    <thead>
                        <tr>
                            <th>Tarih</th>
                            <th>İşlem Türü</th>
                            <th>Tutar</th>
                            <th>Açıklama</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptStatement" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("IslemTarihi", "{0:dd.MM.yyyy}") %></td>
                                    <td><%# Eval("IslemTuru") %></td>
                                    <td><%# Eval("Tutar", "{0:N2}") %></td>
                                    <td><%# Eval("Aciklama") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <!-- Gerekli JS dosyaları -->
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <script src="https://cdn.datatables.net/1.11.5/js/jquery.dataTables.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>
    <script>
        $(document).ready(function () {
            $('#statementTable').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/tr.json'
                }
            });
        });
    </script>
</asp:Content>