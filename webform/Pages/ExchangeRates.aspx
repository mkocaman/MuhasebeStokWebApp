<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExchangeRates.aspx.cs" Inherits="MaOaKApp.Pages.ExchangeRates" MasterPageFile="~/Material.Master"  Async="true"%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Döviz Kurları</h2>
    <asp:GridView ID="gvExchangeRates" runat="server" AutoGenerateColumns="false" CssClass="table table-striped">
        <Columns>
            <asp:BoundField DataField="CurrencyCode" HeaderText="Döviz Kodu" />
            <asp:BoundField DataField="CurrencyName" HeaderText="Döviz Adı" />
            <asp:TemplateField HeaderText="Alış Kuru">
                <ItemTemplate>
                    <asp:Label ID="lblForexBuying" runat="server" Text='<%# Eval("ForexBuying", "{0:N2}") + " " + Eval("Unit") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Satış Kuru">
                <ItemTemplate>
                    <asp:Label ID="lblForexSelling" runat="server" Text='<%# Eval("ForexSelling", "{0:N2}") + " " + Eval("Unit") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <h2>Geçmiş Döviz Kurları</h2>
    <asp:Calendar ID="calFilterDate" runat="server" CssClass="form-control" OnDayRender="calFilterDate_DayRender" />
    <asp:Button ID="btnFilter" runat="server" Text="Filtrele" OnClick="btnFilter_Click" CssClass="btn btn-primary" />
    <asp:GridView ID="gvHistoricalExchangeRates" runat="server" AutoGenerateColumns="false" CssClass="table table-striped" AllowPaging="true" PageSize="10" AllowSorting="true" OnSorting="gvHistoricalExchangeRates_Sorting" OnPageIndexChanging="gvHistoricalExchangeRates_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="CurrencyCode" HeaderText="Döviz Kodu" SortExpression="CurrencyCode" />
            <asp:BoundField DataField="CurrencyName" HeaderText="Döviz Adı" SortExpression="CurrencyName" />
            <asp:TemplateField HeaderText="Alış Kuru">
                <ItemTemplate>
                    <asp:Label ID="lblHistoricalForexBuying" runat="server" Text='<%# Eval("ForexBuying", "{0:N2}") + " " + Eval("Unit") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Satış Kuru">
                <ItemTemplate>
                    <asp:Label ID="lblHistoricalForexSelling" runat="server" Text='<%# Eval("ForexSelling", "{0:N2}") + " " + Eval("Unit") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Date" HeaderText="Tarih" DataFormatString="{0:yyyy-MM-dd}" SortExpression="Date" />
        </Columns>
    </asp:GridView>
</asp:Content>

















