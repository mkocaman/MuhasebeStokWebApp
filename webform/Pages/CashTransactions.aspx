<%@ Page Title="Kasa Hareketleri" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="CashTransactions.aspx.cs" Inherits="MaOaKApp.Pages.CashTransactions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div class="container mt-4">
        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h4 class="mb-0">Kasa Hareketleri</h4>
                <div class="d-flex justify-content-between">
                    <button class="btn btn-primary" type="button" onclick="showAddTransactionModal()">+ Yeni Hareket</button>
                    <button class="btn btn-success" type="button" onclick="showTransferModal()">⇄ Kasalar Arası Transfer</button>
                </div>
            </div>
            <div class="card-body">
                                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <div>
                                <asp:DropDownList ID="ddlFilterCashAccount" runat="server" CssClass="form-control me-2"
                                    AutoPostBack="true" OnSelectedIndexChanged="ddlFilterCashAccount_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                            <div>
                                <asp:DropDownList ID="ddlFilterCustomer" runat="server" CssClass="form-control me-2"
                                    AutoPostBack="true" OnSelectedIndexChanged="ddlFilterCustomer_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                            <div class="d-flex">
                                <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control me-2" TextMode="Date"></asp:TextBox>
                                <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control me-2" TextMode="Date"></asp:TextBox>
                                <asp:Button ID="btnFilterByDate" runat="server" CssClass="btn btn-secondary" Text="Filtrele" OnClick="btnFilterByDate_Click" />
                            </div>
                        </div>

                        <table class="table table-striped" id="basic-1">
                            <thead>
                                <tr>
                                    <th>Fiş No</th>
                                    <th>Tarih</th>
                                    <th>Kasa Adı</th>
                                    <th>Cari Adı</th>
                                    <th>İşlem Türü</th>
                                    <th>Açıklama</th>
                                    <th>Tutar</th>
                                    <th>İşlemler</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptTransactions" runat="server" OnItemDataBound="rptTransactions_ItemDataBound" OnItemCommand="rptTransactions_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("HareketNo") %></td>
                                            <td><%# Eval("IslemTarihi", "{0:dd.MM.yyyy HH:mm}") %></td>
                                            <td><%# Eval("KasaGosterim") %></td>
                                            <td><%# Eval("CariAdi") %></td>
                                            <td><%# Eval("IslemTuru") %></td>
                                            <td><%# Eval("Aciklama") %></td>
                                            <td><%# Eval("Tutar", "{0:N2}") %> <%# Eval("DovizSembol") %></td>
                                            <td>
                                                <asp:PlaceHolder ID="phTransferControls" runat="server">
                                                    <asp:LinkButton runat="server" CssClass="btn btn-warning btn-sm"
                                                        CommandName="EditTransfer" CommandArgument='<%# Eval("TransferID") %>'>
                                                        <i class="fa fa-edit"></i> Transfer Düzenle
                                                    </asp:LinkButton>
                                                    <asp:LinkButton runat="server" CssClass="btn btn-danger btn-sm"
                                                        CommandName="DeleteTransfer" CommandArgument='<%# Eval("TransferID") %>'
                                                        OnClientClick="return confirmDelete();">
                                                        <i class="fa fa-trash"></i> Transfer Sil
                                                    </asp:LinkButton>
                                                </asp:PlaceHolder>
                                                <asp:PlaceHolder ID="phNormalControls" runat="server">
                                                    <asp:LinkButton runat="server" CssClass="btn btn-outline-warning btn-sm"
                                                        CommandName="Edit" CommandArgument='<%# Eval("HareketID") %>'>
                                                        <i class="fa fa-edit"></i> Düzenle
                                                    </asp:LinkButton>
                                                    <asp:LinkButton runat="server" CssClass="btn btn-outline-danger btn-sm"
                                                        CommandName="Delete" CommandArgument='<%# Eval("HareketID") %>'
                                                        OnClientClick="return confirmDelete();">
                                                        <i class="fa fa-trash"></i> Sil
                                                    </asp:LinkButton>
                                                </asp:PlaceHolder>
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

    <!-- Yeni Hareket Modalı -->
    <div class="modal fade" id="addTransactionModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Yeni Kasa Hareketi Ekle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label>Kasa Seç</label>
                        <asp:DropDownList ID="ddlNewCashAccount" runat="server" CssClass="form-control"></asp:DropDownList>
                    </div>

                    <div class="mb-3">
                        <label>İşlem Türü</label>
                        <asp:DropDownList ID="ddlNewTransactionType" runat="server" CssClass="form-control">
                            <asp:ListItem Value="Tahsilat">Tahsilat</asp:ListItem>
                            <asp:ListItem Value="Ödeme">Ödeme</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="mb-3">
                        <label>Müşteri Seç</label>
                        <asp:DropDownList ID="ddlNewCustomer" runat="server" CssClass="form-control"></asp:DropDownList>
                    </div>

                    <div class="mb-3">
                        <label>İşlem Tarihi</label>
                        <asp:TextBox ID="txtNewTransactionDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>

                    <div class="mb-3">
                        <label>Tutar</label>
                        <asp:TextBox ID="txtNewAmount" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>

                    <div class="mb-3">
                        <label>Açıklama</label>
                        <asp:TextBox ID="txtNewDescription" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnSaveTransaction" runat="server" CssClass="btn btn-primary" Text="Kaydet" OnClick="btnSaveTransaction_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- Kasa Hareketi Güncelle Modalı -->
    <div class="modal fade" id="editTransactionModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Kasa Hareketini Güncelle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfEditTransactionID" runat="server" />
                    <div class="mb-3">
                        <label>İşlem Türü</label>
                        <asp:DropDownList ID="ddlEditTransactionType" runat="server" CssClass="form-control">
                            <asp:ListItem Value="Tahsilat">Tahsilat</asp:ListItem>
                            <asp:ListItem Value="Ödeme">Ödeme</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="mb-3">
                        <label>Kasa Seç</label>
                        <asp:DropDownList ID="ddlEditCashAccount" runat="server" CssClass="form-control"></asp:DropDownList>
                    </div>
                    <div class="mb-3">
                        <label>Cari Seç</label>
                        <asp:DropDownList ID="ddlEditCustomer" runat="server" CssClass="form-control"></asp:DropDownList>
                    </div>
                    <div class="mb-3">
                        <label>İşlem Tarihi</label>
                        <asp:TextBox ID="txtEditTransactionDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label>Tutar</label>
                        <asp:TextBox ID="txtEditAmount" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event);"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label>Açıklama</label>
                        <asp:TextBox ID="txtEditDescription" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnUpdateTransaction" runat="server" CssClass="btn btn-success" Text="Güncelle" OnClick="btnUpdateTransaction_Click" />
                </div>
            </div>
        </div>
    </div>

 <!-- Transfer Hareketi Güncelle Modalı -->
<!-- Transfer Hareketi Güncelle Modalı -->
<div class="modal fade" id="editTransferModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Transfer Hareketini Güncelle</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <asp:HiddenField ID="hfEditTransferID" runat="server" />
                <asp:HiddenField ID="hfEditTransferDirection" runat="server" />
                <div class="mb-3">
                    <label>Kaynak Kasa</label>
                    <asp:DropDownList ID="ddlEditSourceCashAccount" runat="server" CssClass="form-control"></asp:DropDownList>
                </div>
                <div class="mb-3">
                    <label>Hedef Kasa</label>
                    <asp:DropDownList ID="ddlEditTargetCashAccount" runat="server" CssClass="form-control"></asp:DropDownList>
                </div>
                <div class="mb-3">
                    <label>İşlem Tarihi</label>
                    <asp:TextBox ID="txtEditTransferDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                </div>
                <div class="mb-3">
                    <label>Tutar</label>
<asp:TextBox ID="txtEditTransferAmount" runat="server" CssClass="form-control" OnTextChanged="txtEditTransferAmount_TextChanged"></asp:TextBox>              

                </div>
                <div class="mb-3">
                    <label>Kur Bilgisi</label>
                    <div class="row">
                        <asp:UpdatePanel ID="updEditExchangeRate" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="row">
                                    <div class="col">
                                        <asp:TextBox ID="txtEditExchangeRate" runat="server" CssClass="form-control"
                                            AutoPostBack="true" OnTextChanged="txtEditExchangeRate_TextChanged"></asp:TextBox>
                                    </div>
                                    <div class="col">
                                        <asp:TextBox ID="txtEditConvertedAmount" runat="server" CssClass="form-control"
                                            AutoPostBack="true" OnTextChanged="txtEditConvertedAmount_TextChanged"></asp:TextBox>
                                    </div>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
                <div class="mb-3">
                    <label>Kaynak Açıklama</label>
                    <asp:TextBox ID="txtEditSourceDescription" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="mb-3">
                    <label>Hedef Açıklama</label>
                    <asp:TextBox ID="txtEditTargetDescription" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnUpdateTransfer" runat="server" CssClass="btn btn-success" Text="Güncelle" OnClick="btnUpdateTransfer_Click" />
            </div>
        </div>
    </div>
</div>

    <!-- Kasalar Arası Transfer Modalı -->
    <div class="modal fade" id="transferModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Kasalar Arası Transfer</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanelTransfer" runat="server">
                        <ContentTemplate>
                            <div class="mb-3">
                                <label>Kaynak Kasa</label>
                                <asp:DropDownList ID="ddlSourceCashAccount" runat="server" CssClass="form-control"
                                    AutoPostBack="true" OnSelectedIndexChanged="ddlSourceCashAccount_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>

                            <div class="mb-3">
                                <label>Hedef Kasa</label>
                                <asp:DropDownList ID="ddlTargetCashAccount" runat="server" CssClass="form-control"
                                    AutoPostBack="true" OnSelectedIndexChanged="ddlTargetCashAccount_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>

                            <div class="mb-3">
                                <label>İşlem Tarihi</label>
                                <asp:TextBox ID="txtTransferDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                            </div>

                            <div class="mb-3">
                                <label>Tutar</label>
                                <asp:TextBox ID="txtTransferAmount" runat="server" CssClass="form-control"
                                    AutoPostBack="true" OnTextChanged="txtTransferAmount_TextChanged"></asp:TextBox>
                            </div>

                            <!-- Döviz Kur Bilgisi -->
                            <div class="mb-3">
                                <label>Kur Bilgisi</label>
                                <div class="row">
                                    <asp:UpdatePanel ID="updExchangeRate" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <div class="row">
                                                <div class="col">
                                                    <asp:TextBox ID="txtExchangeRate" runat="server" CssClass="form-control"
                                                        AutoPostBack="true" OnTextChanged="txtExchangeRate_TextChanged"></asp:TextBox>
                                                </div>
                                                <div class="col">
                                                    <asp:TextBox ID="txtConvertedAmount" runat="server" CssClass="form-control"
                                                        AutoPostBack="true" OnTextChanged="txtConvertedAmount_TextChanged"></asp:TextBox>
                                                </div>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>

                            <div class="mb-3">
                                <label>Açıklama</label>
                                <asp:TextBox ID="txtTransferDescription" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnSaveTransfer" runat="server" CssClass="btn btn-success" Text="Transfer Yap" OnClick="btnSaveTransfer_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- JS -->

<script>
    function showAddTransactionModal() {
        $('#addTransactionModal').modal('show');
    }

    function showTransferModal() {
        $('#transferModal').modal('show');
    }

    function confirmDelete() {
        return confirm('Bu işlemi silmek istediğinize emin misiniz?');
    }

    function showEditTransactionModal() {
        setTimeout(function () {
            var myModal = new bootstrap.Modal(document.getElementById('editTransactionModal'));
            myModal.show();
        }, 300);
    }

    function showEditTransferModal() {
        setTimeout(function () {
            var myModal = new bootstrap.Modal(document.getElementById('editTransferModal'));
            myModal.show();
        }, 300);
    }

    $(document).ready(function () {
        console.log("jQuery Yüklendi.");

        $('#addTransactionModal').on('hidden.bs.modal', function () {
            $(this).find('input, textarea').val('');
            $(this).find('select').prop('selectedIndex', 0);
        });

        $('#editTransactionModal').on('hidden.bs.modal', function () {
            $(this).find('input, textarea').val('');
            $(this).find('select').prop('selectedIndex', 0);
        });

        $('#editTransferModal').on('hidden.bs.modal', function () {
            $(this).find('input, textarea').val('');
            $(this).find('select').prop('selectedIndex', 0);
        });

        $('#txtTransferAmount').on('input', function () {
            var amount = parseFloat($(this).val()) || 0;
            var rate = parseFloat($('#txtExchangeRate').val()) || 1;
            $('#txtConvertedAmount').val((amount * rate).toFixed(2));
        });

        $('#txtExchangeRate').on('input', function () {
            var amount = parseFloat($('#txtTransferAmount').val()) || 0;
            var rate = parseFloat($(this).val()) || 1;
            $('#txtConvertedAmount').val((amount * rate).toFixed(2));
        });

        $('#transferModal').on('hidden.bs.modal', function () {
            $(this).find('input, select').val('');
            $('#txtTransferDate').val(new Date().toISOString().split('T')[0]); // Bugünün tarihi
        });
    });

    function updateExchangeRate() {
        var sourceKasa = $('#<%= ddlSourceCashAccount.ClientID %>').val();
        var targetKasa = $('#<%= ddlTargetCashAccount.ClientID %>').val();

        $.ajax({
            type: "POST",
            url: "CashTransactions.aspx/GetExchangeRate",
            data: JSON.stringify({ sourceKasaID: sourceKasa, targetKasaID: targetKasa }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                $('#<%= txtExchangeRate.ClientID %>').val(response.d);
            },
            error: function (error) {
                console.log("Kur bilgisi alınamadı!", error);
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        const amountInput = document.getElementById('<%= txtTransferAmount.ClientID %>');
        const exchangeRateInput = document.getElementById('<%= txtExchangeRate.ClientID %>');
        const convertedAmountInput = document.getElementById('<%= txtConvertedAmount.ClientID %>');

        function formatCurrency(input) {
            let value = input.value.replace(/\./g, '').replace(',', '.'); // Nokta ve virgül düzenleme
            if (!isNaN(value) && value !== "") {
                let formattedValue = parseFloat(value).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 6 });
                input.value = formattedValue;
            }
        }

        amountInput.addEventListener("blur", function () {
            formatCurrency(amountInput);
        });

        exchangeRateInput.addEventListener("blur", function () {
            formatCurrency(exchangeRateInput);
        });

        convertedAmountInput.addEventListener("blur", function () {
            formatCurrency(convertedAmountInput);
        });

        formatCurrency(amountInput);
        formatCurrency(exchangeRateInput);
        formatCurrency(convertedAmountInput);
    });


    $(document).ready(function () {
        console.log("🔄 Hesaplama Script Yüklendi!");

        $('#<%= txtEditTransferAmount.ClientID %>').on('input', function () {
            calculateConvertedAmount();
        });

        $('#<%= txtEditExchangeRate.ClientID %>').on('input', function () {
            calculateConvertedAmount();
        });

        $('#<%= txtEditConvertedAmount.ClientID %>').on('input', function () {
            calculateOriginalAmount();
        });

        function calculateConvertedAmount() {
            let amount = parseFloat($('#<%= txtEditTransferAmount.ClientID %>').val().replace(',', '.') || 0);
        let exchangeRate = parseFloat($('#<%= txtEditExchangeRate.ClientID %>').val().replace(',', '.') || 1);
        
        if (amount > 0 && exchangeRate > 0) {
            let convertedAmount = (amount * exchangeRate).toFixed(2);
            $('#<%= txtEditConvertedAmount.ClientID %>').val(convertedAmount);
        }
    }

    function calculateOriginalAmount() {
        let convertedAmount = parseFloat($('#<%= txtEditConvertedAmount.ClientID %>').val().replace(',', '.') || 0);
        let exchangeRate = parseFloat($('#<%= txtEditExchangeRate.ClientID %>').val().replace(',', '.') || 1);
        
        if (convertedAmount > 0 && exchangeRate > 0) {
            let originalAmount = (convertedAmount / exchangeRate).toFixed(2);
                $('#<%= txtEditTransferAmount.ClientID %>').val(originalAmount);
            }
        }
    });
</script>

</asp:Content>