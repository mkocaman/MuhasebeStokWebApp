<%@ Page Title="Silinmiş Ürünler" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="ProductsRestore.aspx.cs" Inherits="MaOaKApp.Pages.ProductsRestore" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div class="container mt-4">
        <h3>Silinmiş Ürünler</h3>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:GridView ID="gvDeletedProducts" runat="server" CssClass="table table-striped table-hover"
                    AutoGenerateColumns="False" AllowPaging="True" PageSize="10"
                    OnRowCommand="gvDeletedProducts_RowCommand">
                    <PagerStyle CssClass="pagination" />
                    <Columns>
                        <asp:BoundField DataField="UrunAdi" HeaderText="Ürün Adı" />
                        <asp:BoundField DataField="StokMiktar" HeaderText="Mevcut Stok" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:Button ID="btnRestore" runat="server" CssClass="btn btn-success btn-sm"
                                    Text="Geri Yükle" CommandName="RestoreProduct" CommandArgument='<%# Eval("UrunID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:Label ID="lblNoRecords" runat="server" CssClass="text-danger" Visible="false"></asp:Label>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
