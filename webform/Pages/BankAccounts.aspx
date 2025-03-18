<%@ Page Title="Bankalar ve Hesapları" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="BankAccounts.aspx.cs" Inherits="MaOaKApp.Pages.BankAccounts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    
    <asp:HiddenField ID="hfEditBankAccountID" runat="server" />
    <asp:HiddenField ID="hfDeleteBankID" runat="server" />
<asp:HiddenField ID="hfDeleteBankAccountID" runat="server" />
    <div class="container mt-4">
        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h4 class="mb-0">Bankalar ve Hesapları</h4>
                <div>
                    <button class="btn btn-primary" type="button" onclick="showAddBankModal()">+ Yeni Banka</button>
                    <button class="btn btn-success" type="button" onclick="showAddBankAccountModal()">+ Banka Hesabı Ekle</button>
                </div>
            </div>
            <div class="card-body">
                <asp:UpdatePanel ID="UpdatePanelBanks" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="list-group">
                            <asp:Repeater ID="rptBanks" runat="server" OnItemDataBound="rptBanks_ItemDataBound">
                                <ItemTemplate>
                                            <div class="list-group-item mb-3">
                                            <div class="d-flex justify-content-between align-items-center">
                                                <h5><i class="fa fa-university"></i> <%# Eval("BankaAdi") %></h5>
                                                <div>
                                                    <button class="btn btn-outline-primary btn-sm me-2" onclick="showEditBankModal('<%# Eval("BankaID") %>', '<%# Eval("BankaAdi") %>')"><i class="fa fa-pencil-alt"></i> Düzenle</button>
                                                    <button class="btn btn-outline-danger btn-sm" onclick="confirmDeleteBank('<%# Eval("BankaID") %>')"><i class="fa fa-trash"></i>Sil</button>
                                                </div>
                                            </div>
                                        <ul class="list-group">
                                        <asp:Repeater ID="rptBankAccounts" runat="server">
                                            <ItemTemplate>
                                                        <li class="list-group-item d-flex justify-content-between align-items-center">
                                                            <div>
                                                                <strong><%# Eval("DovizSembol") %> Hesabı</strong> - <%# Eval("HesapAdi") %> 
                                                                <br /><small>IBAN: <%# Eval("IBAN") %></small>
                                                            </div>
                                                            <div class="d-flex align-items-center">
                                                                <span class="badge bg-success me-2 p-2">Bakiye: <%# Eval("Bakiye", "{0:N2}") %> <%# Eval("DovizSembol") %></span>

                                                                <!-- Hesap Hareketleri Butonu -->
                                                            <asp:LinkButton ID="lnkBankTransactions" runat="server" 
                                                                CssClass="btn btn-outline-info btn-sm"
                                                                CommandArgument='<%# Eval("BankaHesapID") %>' 
                                                                OnClick="lnkBankTransactions_Click">
                                                                <i class="fa fa-list"></i> Hareketler
                                                            </asp:LinkButton>

                                                                <!-- Düzenle Butonu -->
                                                                <asp:LinkButton ID="btnEditBankAccount" runat="server" CssClass="btn btn-outline-warning btn-sm"
                                                                OnClientClick='<%# "showEditBankAccountModal(\"" + Eval("BankaHesapID") + "\", \"" + Eval("HesapNo") + "\", \"" + Eval("HesapAdi") + "\", \"" + Eval("IBAN") + "\", \"" + Eval("SubeAdi") + "\", \"" + Eval("DovizID") + "\"); return false;" %>'>
                                                                <i class="fa fa-pencil-alt"></i> Düzenle
                                                            </asp:LinkButton>

                                                                <!-- Sil Butonu -->
                                                                 <asp:LinkButton ID="btnDeleteBankAccount" runat="server" CssClass="btn btn-outline-danger btn-sm"
                                                                    OnClientClick='<%# "confirmDeleteBankAccount(\"" + Eval("BankaHesapID") + "\"); return false;" %>'>
                                                                    <i class="fa fa-trash"></i> Sil
                                                                </asp:LinkButton>
                                                            </div>
                                                        </li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        </ul>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <!-- Banka Hesabı Güncelleme Modalı -->
<div class="modal fade" id="editBankAccountModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Banka Hesabını Güncelle</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <asp:UpdatePanel ID="UpdatePanelEditBankAccount" runat="server">
                    <ContentTemplate>
                        <asp:HiddenField ID="hfBankAccountID" runat="server" />
                        <div class="mb-3">
                            <label>Hesap Numarası</label>
                            <asp:TextBox ID="txtEditAccountNumber" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label>Hesap Adı</label>
                            <asp:TextBox ID="txtEditAccountName" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label>IBAN</label>
                            <asp:TextBox ID="txtEditIBAN" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label>Şube Adı</label>
                            <asp:TextBox ID="txtEditBranch" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label>Döviz Türü</label>
                            <asp:DropDownList ID="ddlEditCurrency" runat="server" CssClass="form-control"></asp:DropDownList>
                        </div>
                        <asp:HiddenField ID="hfSonGuncelleyenKullaniciID" runat="server" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnUpdateBankAccount" runat="server" CssClass="btn btn-warning" Text="Güncelle"
                            OnClick="btnUpdateBankAccount_Click" UseSubmitBehavior="false" />
            </div>
        </div>
    </div>
</div>
    <!-- Yeni Banka Ekleme Modalı -->
    <div class="modal fade" id="addBankModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Yeni Banka Ekle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanelAddBank" runat="server">
                        <ContentTemplate>
                            <div class="mb-3">
                                <label>Banka Adı</label>
                                <asp:TextBox ID="txtBankName" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnSaveBank" runat="server" CssClass="btn btn-primary" Text="Kaydet"
                                OnClick="btnSaveBank_Click" UseSubmitBehavior="false" />
                </div>
            </div>
        </div>
    </div>

    <!-- Banka Güncelleme Modalı -->
<div class="modal fade" id="editBankModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Banka Güncelle</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <asp:UpdatePanel ID="UpdatePanelEditBank" runat="server">
                    <ContentTemplate>
                        <!-- Banka ID (Gizli Alan) -->
                        <asp:HiddenField ID="hfEditBankID" runat="server" />

                        <div class="mb-3">
                            <label>Banka Adı</label>
                            <asp:TextBox ID="txtEditBankName" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnUpdateBank" runat="server" CssClass="btn btn-warning" Text="Güncelle"
                            OnClick="btnUpdateBank_Click" UseSubmitBehavior="false" />
            </div>
        </div>
    </div>
</div>

<!-- Yeni Banka Hesabı Ekleme Modalı -->
<div class="modal fade" id="addBankAccountModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Yeni Banka Hesabı Ekle</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <asp:UpdatePanel ID="UpdatePanelAddBankAccount" runat="server">
                    <ContentTemplate>
                        <div class="mb-3">
                            <label>Banka Seç</label>
                            <asp:DropDownList ID="ddlSelectBank" runat="server" CssClass="form-control"></asp:DropDownList>
                        </div>
                        <asp:HiddenField ID="hfBankID" runat="server" />
                        <div class="mb-3">
                            <label>Hesap Numarası</label>
                            <asp:TextBox ID="txtAccountNumber" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label>Hesap Adı</label>
                            <asp:TextBox ID="txtAccountName" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label>IBAN</label>
                            <asp:TextBox ID="txtIBAN" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label>Şube Adı</label>
                            <asp:TextBox ID="txtBranch" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label>Döviz Türü</label>
                            <asp:DropDownList ID="ddlCurrency" runat="server" CssClass="form-control"></asp:DropDownList>
                        </div>
                        <asp:HiddenField ID="hfOlusturanKullaniciID" runat="server" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnSaveBankAccount" runat="server" CssClass="btn btn-success" Text="Kaydet"
                            OnClick="btnSaveBankAccount_Click" UseSubmitBehavior="false" />
            </div>
        </div>
    </div>
</div>

    <script>
        function showAddBankModal() {
            $('#addBankModal').modal('show');
        }

        function showAddBankAccountModal() {
            $('#addBankAccountModal').modal('show');
        }

        function showEditBankAccountModal(bankAccountID, accountNumber, accountName, iban, branch, currencyID) {
            $('#<%= hfEditBankAccountID.ClientID %>').val(bankAccountID);
            $('#<%= txtEditAccountNumber.ClientID %>').val(accountNumber);
            $('#<%= txtEditAccountName.ClientID %>').val(accountName);
    $('#<%= txtEditIBAN.ClientID %>').val(iban);
    $('#<%= txtEditBranch.ClientID %>').val(branch);
            $('#<%= ddlEditCurrency.ClientID %>').val(currencyID);

            $('#editBankAccountModal').modal('show');
        }

        function showEditBankAccountModal(bankAccountID, accountNumber, accountName, iban, branch, currencyID) {
            document.getElementById('<%= hfEditBankAccountID.ClientID %>').value = bankAccountID;
    document.getElementById('<%= txtEditAccountNumber.ClientID %>').value = accountNumber;
    document.getElementById('<%= txtEditAccountName.ClientID %>').value = accountName;
    document.getElementById('<%= txtEditIBAN.ClientID %>').value = iban;
    document.getElementById('<%= txtEditBranch.ClientID %>').value = branch;
    document.getElementById('<%= ddlEditCurrency.ClientID %>').value = currencyID;

    $('#editBankAccountModal').modal('show');
}

        function confirmDeleteBank(bankID) {
            Swal.fire({
                title: "Emin misiniz?",
                text: "Bu bankayı silmek istediğinizden emin misiniz?",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#d33",
                cancelButtonColor: "#3085d6",
                confirmButtonText: "Evet, sil!",
                cancelButtonText: "İptal"
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfDeleteBankID.ClientID %>').value = bankID;
            __doPostBack('DeleteBank', ''); // Postback ile silme işlemini başlat
        }
    });
        }

        function confirmDeleteBankAccount(bankAccountID) {
            Swal.fire({
                title: "Emin misiniz?",
                text: "Bu banka hesabını silmek istediğinizden emin misiniz?",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#d33",
                cancelButtonColor: "#3085d6",
                confirmButtonText: "Evet, sil!",
                cancelButtonText: "İptal"
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfDeleteBankAccountID.ClientID %>').value = bankAccountID;
            __doPostBack('DeleteBankAccount', ''); // Postback ile silme işlemini başlat
        }
    });
        }
    </script>
</asp:Content>