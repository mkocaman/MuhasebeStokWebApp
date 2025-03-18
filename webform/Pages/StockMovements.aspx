<%@ Page Title="Stok Hareketleri" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="StockMovements.aspx.cs" Inherits="MuhasebeStokDB.Pages.StockMovements" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h3 class="mt-4">Stok Hareketleri</h3>
        <div class="card mt-4">
            <div class="card-header d-flex justify-content-between">
                <h5>Stok Hareket Listesi</h5>
                <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#yeniHareketModal">Yeni Hareket</button>
            </div>
            <div class="card-body">
                <div class="input-group input-group-outline mb-3">
                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" Placeholder="Ürün veya Depo Ara"></asp:TextBox>
                    </div>
    <asp:Button ID="btnSearch" runat="server" Text="Ara" CssClass="btn btn-primary" OnClick="btnSearch_Click" />

   <asp:GridView ID="gvStockMovements" runat="server" CssClass="table table-striped" AutoGenerateColumns="false">
    <Columns>
        <asp:BoundField DataField="UrunAdi" HeaderText="Ürün Adı" />
        <asp:BoundField DataField="Miktar" HeaderText="Miktar" />
        <asp:BoundField DataField="HareketTarihi" HeaderText="Tarih" DataFormatString="{0:dd.MM.yyyy}" />
        <asp:BoundField DataField="HareketTuru" HeaderText="Hareket Türü" />
        <asp:BoundField DataField="BirimFiyat" HeaderText="Birim Fiyat" SortExpression="BirimFiyat" />

        <asp:TemplateField HeaderText="Bağlantılar">
            <ItemTemplate>
                <%# Eval("FaturaID") != null ? 
                    $"<a href='InvoiceDetails.aspx?InvoiceID={Eval("FaturaID")}' class='btn btn-primary btn-sm'>Faturaya Git</a>" : 
                    "" %>
                <%# Eval("IrsaliyeID") != null ? 
                    $"<a href='DeliveryNoteDetails.aspx?DeliveryNoteID={Eval("IrsaliyeID")}' class='btn btn-secondary btn-sm'>İrsaliyeye Git</a>" : 
                    "" %>
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>

            </div>
        </div>
    </div>

    <!-- Yeni Hareket Modal -->
    <div class="modal fade" id="yeniHareketModal" tabindex="-1" aria-labelledby="yeniHareketModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="yeniHareketModalLabel">Yeni Stok Hareketi</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:DropDownList ID="ddlHareketTuru" runat="server" CssClass="form-select mb-3">
                        <asp:ListItem Text="Giriş" Value="Giriş"></asp:ListItem>
                        <asp:ListItem Text="Çıkış" Value="Çıkış"></asp:ListItem>
                    </asp:DropDownList>
                    <asp:TextBox ID="txtUrunAdi" runat="server" CssClass="form-control mb-3" Placeholder="Ürün Adı"></asp:TextBox>
                    <asp:TextBox ID="txtMiktar" runat="server" CssClass="form-control mb-3" Placeholder="Miktar"></asp:TextBox>
                    <asp:Button ID="btnHareketKaydet" runat="server" CssClass="btn btn-primary w-100" Text="Kaydet" OnClick="btnHareketKaydet_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
