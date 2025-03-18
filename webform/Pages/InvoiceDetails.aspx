<%@ Page Title="Fatura Detayları" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="InvoiceDetails.aspx.cs" Inherits="MuhasebeStokDB.Pages.InvoiceDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container invoice">
        <div class="row">
            <div class="col-sm-12">
                <div class="card">
                    <div class="card-body">
                        <div>
                            <div>
                                <div class="row invo-header">
                                    <div class="col-sm-6">
                                        <div class="media">
                                            <div class="media-left">
                                                <img class="media-object img-60" src="../images/logo.png" alt="">
                                            </div>
                                            <div class="media-body m-l-20">
                                                <h4 class="media-heading f-w-600">Firm Name</h4>
                                                <p>info@firm.com<br><span class="digits">123-456-7890</span></p>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-sm-6">
                                        <div class="text-md-end text-xs-center">
                                            <h3>Fatura #<span class="digits"><asp:Label ID="lblFaturaNumarasi" runat="server" /></span></h3>
                                            <p>Fatura Tarihi: <span class="digits"><asp:Label ID="lblFaturaTarihi" runat="server" /></span><br>
                                                Vade Tarihi: <span class="digits"><asp:Label ID="lblVadeTarihi" runat="server" /></span></p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="row invo-profile">
                                <div class="col-xl-4">
                                    <div class="media">
                                        <div class="media-body m-l-20">
                                            <h4 class="media-heading f-w-600">Müşteri Bilgileri</h4>
                                            <p><asp:Label ID="lblCariAdi" runat="server" /><br><asp:Label ID="lblCustomerAddress" runat="server" /></p>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-xl-8">
                                    <div class="text-xl-end" id="project">
                                        <h6>Fatura Notu</h6>
                                        <p><asp:Label ID="lblFaturaNotu" runat="server" /></p>
                                    </div>
                                </div>
                            </div>
                            <div>
                                <div class="table-responsive invoice-table" id="table">
                                    <table class="table table-bordered table-striped">
                                        <thead>
                                            <tr>
                                                <th>Ürün Adı</th>
                                                <th>Miktar</th>
                                                <th>Birim Fiyat</th>
                                                <th>KDV Toplam</th>
                                                <th>Satır Toplamı</th>


                                            </tr>
                                        </thead>
                                        <tbody>
                                            <asp:Repeater ID="rptInvoiceDetails" runat="server">
                                                <ItemTemplate>
                                                    <tr>
                                                        <td><asp:Label ID="lblUrunAdi" runat="server" Text='<%# Eval("UrunAdi") %>' /></td>
                                                        <td><asp:Label ID="lblMiktar" runat="server" Text='<%# Eval("Miktar") %>' /></td>
                                                        <td><asp:Label ID="lblBirimFiyat" runat="server" Text='<%# Eval("BirimFiyat", "{0:N2}") %>' /></td>
                                                        <td><asp:Label ID="LblKdv" runat="server" Text='<%# Eval("SatirKDV", "{0:N2}") %>' /></td>
                                                        <td><asp:Label ID="lblSatirToplam" runat="server" Text='<%# Eval("SatirToplam", "{0:N2}") %>' /></td>
                                                    </tr>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </tbody>
                                    </table>
                                </div>
                                <div class="row mt-3">
                                    
                                    <div class="col-md-4">
                                        <div class="text-md-end text-xs-center">
                                            <p>Ara Toplam: <asp:Label ID="lblAraToplam" runat="server" /></p>
                                            <p>KDV Toplam: <asp:Label ID="lblKdvToplam" runat="server" /></p>
                                            <p>Genel Toplam: <asp:Label ID="lblGenelToplam" runat="server" /></p>
                                            <p>Ödeme Türü: <asp:Label ID="lblOdemeTuru" runat="server" /></p>
                                            <p>Döviz Türü: <asp:Label ID="lblDovizTuru" runat="server" /></p>
                                            <p>Döviz Kuru: <asp:Label ID="lblDovizKuru" runat="server" /></p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-12 text-center mt-3">
                                <asp:Button ID="btnDownloadPDF" runat="server" Text="PDF İndir" OnClick="btnDownloadPDF_Click" CssClass="btn btn-primary me-2" />
                                <button class="btn btn-secondary" type="button" onclick="history.back()">Geri</button>
                            </div>
                            <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>