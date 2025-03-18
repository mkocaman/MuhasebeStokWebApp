<%@ Page Title="Banka Hareketleri" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" 
    CodeBehind="BankTransactions.aspx.cs" Inherits="MaOaKApp.Pages.BankTransactions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- ScriptManager ve stil tanımlamaları -->
    <!-- ScriptManager güncelleme -->
<asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" EnablePageMethods="true" ScriptMode="Release">
    <Scripts>
    </Scripts>
</asp:ScriptManager>

    <style>
        #filterPanel { display: none; }
        .custom-invalid-feedback {
            display: block;
            width: 100%;
            margin-top: 0.1rem;
            font-size: 0.7rem;
            color: #dc3545;
        }
    </style>
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>

    <!-- Ana İçerik (Filtreleme, liste, repeater vb.) -->
    <div class="container mt-4">
        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h4 class="mb-0">Banka Hareketleri</h4>
                <div class="d-flex">
                    <button class="btn btn-primary me-2" type="button" onclick="showAddTransactionModal()">+ Yeni Hareket</button>
                    <button class="btn btn-success" type="button" onclick="showTransferModal()">⇄ Bankalar Arası Transfer</button>
                </div>
            </div>
            <div class="card-body">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <!-- Filtreleme Alanı -->
                        <asp:HiddenField ID="hfFilterPanelState" runat="server" />
                        <div class="container mt-3">
                            <div class="d-flex justify-content-between align-items-center">
                                <h4 class="mb-0"></h4>
                                <button id="btnToggleFilter" class="btn btn-info" type="button" onclick="toggleFilterPanel()">
                                    <i class="fa fa-filter"></i> Filtrele
                                </button>
                            </div>
                            <div id="filterPanel" class="card mt-3 p-3">
                                <div class="row">
                                    <div class="col-md-3">
                                        <label>Banka Hesabı</label>
                                        <asp:DropDownList ID="ddlBankAccountFilter" runat="server" CssClass="form-control"
                                            AutoPostBack="true" OnSelectedIndexChanged="ddlBankAccountFilter_SelectedIndexChanged">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-3">
                                        <label>Cari Hesabı</label>
                                        <asp:DropDownList ID="ddlCustomerFilter" runat="server" CssClass="form-control"
                                            AutoPostBack="true" OnSelectedIndexChanged="ddlCustomerFilter_SelectedIndexChanged">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-2">
                                        <label>Yıl</label>
                                        <asp:DropDownList ID="ddlYearFilter" runat="server" CssClass="form-control"
                                            AutoPostBack="true" OnSelectedIndexChanged="ddlYearFilter_SelectedIndexChanged">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-2">
                                        <label>Ay</label>
                                        <asp:DropDownList ID="ddlMonthFilter" runat="server" CssClass="form-control"
                                            AutoPostBack="true" OnSelectedIndexChanged="ddlMonthFilter_SelectedIndexChanged">
                                            <asp:ListItem Value="">Tüm Aylar</asp:ListItem>
                                            <asp:ListItem Value="1">Ocak</asp:ListItem>
                                            <asp:ListItem Value="2">Şubat</asp:ListItem>
                                            <asp:ListItem Value="3">Mart</asp:ListItem>
                                            <asp:ListItem Value="4">Nisan</asp:ListItem>
                                            <asp:ListItem Value="5">Mayıs</asp:ListItem>
                                            <asp:ListItem Value="6">Haziran</asp:ListItem>
                                            <asp:ListItem Value="7">Temmuz</asp:ListItem>
                                            <asp:ListItem Value="8">Ağustos</asp:ListItem>
                                            <asp:ListItem Value="9">Eylül</asp:ListItem>
                                            <asp:ListItem Value="10">Ekim</asp:ListItem>
                                            <asp:ListItem Value="11">Kasım</asp:ListItem>
                                            <asp:ListItem Value="12">Aralık</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="row mt-3">
                                    <div class="col-md-3">
                                        <label>Başlangıç Tarihi</label>
                                        <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                    </div>
                                    <div class="col-md-3">
                                        <label>Bitiş Tarihi</label>
                                        <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                    </div>
                                    <div class="col-md-6 d-flex align-items-end justify-content-end">
                                        <asp:Button ID="btnApplyFilter" runat="server" CssClass="btn btn-primary me-2"
                                            Text="Filtrele" OnClick="btnApplyFilter_Click" />
                                        <button id="btnResetFilter" class="btn btn-secondary" type="button" onclick="resetFilters()">
                                            <i class="fa fa-times"></i> Temizle
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- Banka Hareketleri Tablosu -->
                        <table class="table table-striped" id="basic-1">
                            <thead>
                                <tr>
                                    <th></th>
                                    <th>Fiş No</th>
                                    <th>Tarih</th>
                                    <th>Banka Adı</th>
                                    <th>Cari Adı</th>
                                    <th>Tutar</th>
                                    <th>İşlemler</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptBankTransactions" runat="server" OnItemCommand="rptBankTransactions_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td>
                                                <%# Eval("HareketTuru").ToString() == "Ödeme" 
                                                    ? "<i class='fa fa-arrow-circle-up text-danger'></i>" 
                                                    : "<i class='fa fa-arrow-circle-down text-success'></i>" %>
                                            </td>
                                            <td><%# Eval("HareketNo") %></td>
                                            <td><%# Eval("IslemTarihi", "{0:dd.MM.yyyy HH:mm}") %></td>
                                            <td><%# Eval("BankaGosterim") %></td>
                                            <td><%# Eval("CariAdi") %></td>
                                            <td><%# Eval("Tutar", "{0:N2}") %> <%# Eval("DovizSembol") %></td>
                                            <td style="white-space: nowrap;">
                                                    <asp:LinkButton ID="btnShowDetails" runat="server" 
                                                        CommandName="ShowDetails" 
                                                        CommandArgument='<%# Eval("HareketID") %>' 
                                                        CssClass="btn btn-outline-info btn-sm">
                                                        <i class="fa fa-info-circle"></i> Detay
                                                    </asp:LinkButton>
                                                <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-outline-success btn-sm"
                                                    CommandName="Edit" Visible='<%# Eval("TransferID") == DBNull.Value %>'
                                                    CommandArgument='<%# Eval("HareketID") %>'>
                                                    <i class="fa fa-edit"></i> Düzenle
                                                </asp:LinkButton>
                                              
                                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-outline-danger btn-sm"
                                                    CommandName="Delete" CommandArgument='<%# Eval("HareketID") %>'
                                                    Visible='<%# Eval("TransferID") == DBNull.Value %>'
                                                    OnClientClick='<%# "return confirmDelete(\"Delete\", \"" + Eval("HareketID") + "\");" %>'>
                                                    <i class="fa fa-trash"></i> Sil
                                                </asp:LinkButton>

                                                <asp:LinkButton ID="btnEditTransfer" runat="server" CssClass="btn btn-success btn-sm"
                                                    CommandName="EditTransfer" CommandArgument='<%# Eval("TransferID") %>'
                                                    Visible='<%# Eval("TransferID") != DBNull.Value %>'>
                                                    <i class="fa fa-edit"></i> Düzenle
                                                </asp:LinkButton>

                                                <asp:LinkButton ID="LinkButton1" runat="server" CssClass="btn btn-danger btn-sm"
                                                    CommandName="DeleteTransfer" CommandArgument='<%# Eval("TransferID") %>'
                                                    Visible='<%# Eval("TransferID") != DBNull.Value %>'
                                                    OnClientClick='<%# "return confirmDelete(\"DeleteTransfer\", \"" + Eval("TransferID") + "\");" %>'>
                                                    <i class="fa fa-trash"></i> Sil
                                                </asp:LinkButton>
                                                
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                        <div class="text-center">
                            <asp:Label ID="lblNoRecords" runat="server" CssClass="alert alert-danger mt-3 text-center w-100" Visible="false">
                                Aradığınız kriterlerde kayıt bulunamadı.
                            </asp:Label>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <!-- Modallar -->
    <div>
        <!-- Add Transaction Modal -->
        <div class="modal fade" id="addTransactionModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Yeni Banka İşlemi Ekle</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <form id="BankaHareket" novalidate>
                                    <div class="mb-3">
                                        <label for="ddlTransactionBank" class="form-label">Banka Seç</label>
                                        <asp:DropDownList ID="ddlTransactionBank" runat="server" CssClass="form-control"
                                            AutoPostBack="true" OnSelectedIndexChanged="ddlTransactionBank_SelectedIndexChanged">
                                        </asp:DropDownList>
                                        <div class="invalid-feedback">Lütfen bir banka seçin.</div>
                                    </div>
                                    <div class="mb-3">
                                        <label for="ddlTransactionBankAccount" class="form-label">Banka Hesabı Seç</label>
                                        <asp:DropDownList ID="ddlTransactionBankAccount" runat="server" CssClass="form-control"
                                            AutoPostBack="true" OnSelectedIndexChanged="ddlTransactionBankAccount_SelectedIndexChanged">
                                        </asp:DropDownList>
                                        <div class="invalid-feedback">Lütfen bir banka hesabı seçin.</div>
                                        <asp:Label ID="lblBankBalance" runat="server" CssClass="custom-invalid-feedback balance-label" Visible="false"></asp:Label>
                                    </div>
                                    <div class="mb-3">
                                        <label for="ddlTransactionType" class="form-label">İşlem Türü</label>
                                        <asp:DropDownList ID="ddlTransactionType" runat="server" CssClass="form-control">
                                            <asp:ListItem Value="Tahsilat">Tahsilat</asp:ListItem>
                                            <asp:ListItem Value="Ödeme">Ödeme</asp:ListItem>
                                        </asp:DropDownList>
                                        <div class="invalid-feedback">Lütfen bir işlem türü seçin.</div>
                                    </div>
                                    <div class="mb-3">
                                        <label for="ddlTransactionCustomer" class="form-label">Müşteri Seç</label>
                                        <asp:DropDownList ID="ddlTransactionCustomer" runat="server" CssClass="form-control">
                                        </asp:DropDownList>
                                        <div class="invalid-feedback">Lütfen bir müşteri seçin.</div>
                                    </div>
                                    <div class="mb-3">
                                        <label for="txtTransactionDate" class="form-label">İşlem Tarihi</label>
                                        <asp:TextBox ID="txtTransactionDate" runat="server" CssClass="form-control" TextMode="Date">
                                        </asp:TextBox>
                                        <div class="invalid-feedback">Lütfen geçerli bir tarih girin.</div>
                                    </div>
                                    <div class="mb-3">
                                        <label for="txtTransactionAmount" class="form-label">Tutar</label>
                                        <asp:TextBox ID="txtTransactionAmount" runat="server" CssClass="form-control">
                                        </asp:TextBox>
                                        <div class="invalid-feedback">Lütfen geçerli bir tutar girin.</div>
                                    </div>
                                    <div class="mb-3">
                                        <label for="txtTransactionDescription" class="form-label">Açıklama</label>
                                        <asp:TextBox ID="txtTransactionDescription" runat="server" CssClass="form-control">
                                        </asp:TextBox>
                                    </div>
                                </form>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ddlTransactionBank" EventName="SelectedIndexChanged" />
                                <asp:AsyncPostBackTrigger ControlID="ddlTransactionBankAccount" EventName="SelectedIndexChanged" />
                                <asp:AsyncPostBackTrigger ControlID="btnSaveTransaction" EventName="Click" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="btnSaveTransaction" runat="server" CssClass="btn btn-primary" Text="Kaydet" 
                            OnClick="btnSaveTransaction_Click" UseSubmitBehavior="false" />
                    </div>
                </div>
            </div>
        </div>
        
        <!-- Transfer Modal -->
        <div class="modal fade" id="transferModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Bankalar Arası Transfer</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <asp:UpdatePanel ID="UpdatePanelTransfer" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Panel ID="pnlTransferForm" runat="server">
                                    <form id="BankaTransfer" novalidate>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <label>Kaynak Banka</label>
                                                <asp:DropDownList ID="ddlSourceBank" runat="server" CssClass="form-control"
                                                    AutoPostBack="true" OnSelectedIndexChanged="ddlSourceBank_SelectedIndexChanged">
                                                </asp:DropDownList>
                                                <div class="invalid-feedback">Lütfen bir kaynak banka seçin.</div>
                                            </div>
                                            <div class="col-md-6">
                                                <label>Kaynak Hesap</label>
                                                <asp:DropDownList ID="ddlSourceBankAccount" runat="server" CssClass="form-control"
                                                    AutoPostBack="true" OnSelectedIndexChanged="ddlSourceBankAccount_SelectedIndexChanged">
                                                </asp:DropDownList>
                                                <div class="invalid-feedback">Lütfen bir kaynak hesap seçin.</div>
                                            </div>
                                        </div>
                                        <asp:Label ID="lblSourceBankBalance" runat="server" CssClass="custom-invalid-feedback balance-label" Visible="false"></asp:Label>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <label>Hedef Banka</label>
                                                <asp:DropDownList ID="ddlTargetBank" runat="server" CssClass="form-control"
                                                    AutoPostBack="true" OnSelectedIndexChanged="ddlTargetBank_SelectedIndexChanged">
                                                </asp:DropDownList>
                                                <div class="invalid-feedback">Lütfen bir hedef banka seçin.</div>
                                            </div>
                                            <div class="col-md-6">
                                                <label>Hedef Hesap</label>
                                                <asp:DropDownList ID="ddlTargetBankAccount" runat="server" CssClass="form-control"
                                                    AutoPostBack="true" OnSelectedIndexChanged="ddlTargetBankAccount_SelectedIndexChanged">
                                                </asp:DropDownList>
                                                <div class="invalid-feedback">Lütfen bir hedef hesap seçin.</div>
                                            </div>
                                        </div>
                                        <asp:Label ID="lblTargetBankBalance" runat="server" CssClass="custom-invalid-feedback balance-label" Visible="false"></asp:Label>
                                        <div class="row mt-3">
                                            <div class="col-md-12">
                                                <label>İşlem Tarihi</label>
                                                <asp:TextBox ID="txtTransferDate" runat="server" CssClass="form-control" TextMode="Date">
                                                </asp:TextBox>
                                                <div class="invalid-feedback">Lütfen geçerli bir tarih girin.</div>
                                            </div>
                                        </div>
                                        <div class="row mt-3">
                                            <div class="col-md-12">
                                                <label>Tutar</label>
                                                <asp:TextBox ID="txtTransferAmount" runat="server" CssClass="form-control"
                                                    AutoPostBack="true" OnTextChanged="txtTransferAmount_TextChanged">
                                                </asp:TextBox>
                                                <div class="invalid-feedback">Lütfen geçerli bir tutar girin.</div>
                                            </div>
                                        </div>
                                        <div class="row mt-3">
                                            <div class="col-md-6">
                                                <label>Kur Bilgisi</label>
                                                <asp:TextBox ID="txtExchangeRate" runat="server" CssClass="form-control"
                                                    AutoPostBack="true" OnTextChanged="txtExchangeRate_TextChanged">
                                                </asp:TextBox>
                                                <div class="invalid-feedback">Lütfen geçerli bir kur bilgisi girin.</div>
                                            </div>
                                            <div class="col-md-6">
                                                <label>Giden Tutar</label>
                                                <asp:TextBox ID="txtConvertedAmount" runat="server" CssClass="form-control"
                                                    AutoPostBack="true" OnTextChanged="txtConvertedAmount_TextChanged">
                                                </asp:TextBox>
                                                <div class="invalid-feedback">Lütfen geçerli bir giden tutar girin.</div>
                                            </div>
                                        </div>
                                        <div class="row mt-3">
                                            <div class="col-md-12">
                                                <label>Açıklama</label>
                                                <asp:TextBox ID="txtTransferDescription" runat="server" CssClass="form-control">
                                                </asp:TextBox>
                                            </div>
                                        </div>
                                    </form>
                                </asp:Panel>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="txtTransferAmount" EventName="TextChanged" />
                                <asp:AsyncPostBackTrigger ControlID="txtExchangeRate" EventName="TextChanged" />
                                <asp:AsyncPostBackTrigger ControlID="txtConvertedAmount" EventName="TextChanged" />
                                <asp:AsyncPostBackTrigger ControlID="btnSaveTransfer" EventName="Click" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="btnSaveTransfer" runat="server" CssClass="btn btn-success" Text="Transfer Yap" 
                            OnClick="btnSaveTransfer_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Edit Transaction Modal -->
        <!-- Edit Transaction Modal -->
        <div class="modal fade" id="editTransactionModal" tabindex="-1">
          <div class="modal-dialog">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Banka Hareketini Düzenle</h5>
                <!-- Bootstrap 5 modal close button -->
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
          <span aria-hidden="true">&times;</span>
        </button>
              </div>
              <div class="modal-body">
                <asp:UpdatePanel ID="UpdatePanelEditTransaction" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                  <ContentTemplate>
                    <!-- HiddenField to store the transaction ID -->
                    <asp:HiddenField ID="hfEditTransactionID" runat="server" />
            
                    <!-- Bank selection -->
                    <div class="mb-3">
                      <label for="ddlEditTransactionBank" class="form-label">Banka Seç</label>
                      <asp:DropDownList ID="ddlEditTransactionBank" runat="server" CssClass="form-control"
                          AutoPostBack="true" OnSelectedIndexChanged="ddlEditTransactionBank_SelectedIndexChanged" ClientIDMode="Static">
                      </asp:DropDownList>
                      <div class="invalid-feedback">Lütfen bir banka seçin.</div>
                    </div>
            
                    <!-- Bank account selection -->
                    <div class="mb-3">
                      <label for="ddlEditTransactionBankAccount" class="form-label">Banka Hesabı Seç</label>
                      <asp:DropDownList ID="ddlEditTransactionBankAccount" runat="server" CssClass="form-control"
                          AutoPostBack="true" ClientIDMode="Static">
                      </asp:DropDownList>
                      <div class="invalid-feedback">Lütfen bir banka hesabı seçin.</div>
                    </div>
            
                    <!-- Transaction type -->
                    <div class="mb-3">
                      <label for="ddlEditTransactionType" class="form-label">İşlem Türü</label>
                      <asp:DropDownList ID="ddlEditTransactionType" runat="server" CssClass="form-control" ClientIDMode="Static">
                        <asp:ListItem Value="Tahsilat">Tahsilat</asp:ListItem>
                        <asp:ListItem Value="Ödeme">Ödeme</asp:ListItem>
                      </asp:DropDownList>
                      <div class="invalid-feedback">Lütfen bir işlem türü seçin.</div>
                    </div>
            
                    <!-- Customer selection -->
                    <div class="mb-3">
                      <label for="ddlEditTransactionCustomer" class="form-label">Müşteri Seç</label>
                      <asp:DropDownList ID="ddlEditTransactionCustomer" runat="server" CssClass="form-control" ClientIDMode="Static">
                      </asp:DropDownList>
                      <div class="invalid-feedback">Lütfen bir müşteri seçin.</div>
                    </div>
            
                    <!-- Transaction date -->
                    <div class="mb-3">
                      <label for="txtEditTransactionDate" class="form-label">İşlem Tarihi</label>
                      <asp:TextBox ID="txtEditTransactionDate" runat="server" CssClass="form-control" TextMode="Date" ClientIDMode="Static">
                      </asp:TextBox>
                      <div class="invalid-feedback">Lütfen geçerli bir tarih girin.</div>
                    </div>
            
                    <!-- Transaction amount -->
                    <div class="mb-3">
                      <label for="txtEditTransactionAmount" class="form-label">Tutar</label>
                      <asp:TextBox ID="txtEditTransactionAmount" runat="server" CssClass="form-control" ClientIDMode="Static">
                      </asp:TextBox>
                      <div class="invalid-feedback">Lütfen geçerli bir tutar girin.</div>
                    </div>
            
                    <!-- Transaction description -->
                    <div class="mb-3">
                      <label for="txtEditTransactionDescription" class="form-label">Açıklama</label>
                      <asp:TextBox ID="txtEditTransactionDescription" runat="server" CssClass="form-control" ClientIDMode="Static">
                      </asp:TextBox>
                    </div>
                    <!-- These triggers ensure that partial postbacks occur for the specified controls -->

                  </ContentTemplate>
          
                    <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="ddlEditTransactionBank" EventName="SelectedIndexChanged" />
                    <asp:AsyncPostBackTrigger ControlID="ddlEditTransactionBankAccount" EventName="SelectedIndexChanged" />
                    <asp:AsyncPostBackTrigger ControlID="btnUpdateTransaction" EventName="Click" />
                  </Triggers>
                </asp:UpdatePanel>
              </div>
              <div class="modal-footer">
                <!-- Update button; UseSubmitBehavior set to false to prevent full page postback -->
               <asp:Button ID="btnUpdateTransaction" runat="server" CssClass="btn btn-warning" 
            Text="Güncelle" OnClick="btnUpdateTransaction_Click" UseSubmitBehavior="false" />
                  <button type="button" class="btn btn-secondary" data-dismiss="modal">Kapat</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Edit Transfer Modal -->
        <!-- Edit Transfer Modal -->
        <!-- Edit Transfer Modal -->
        <div class="modal fade" id="editTransferModal" tabindex="-1" role="dialog" aria-labelledby="editTransferModalLabel" aria-hidden="true">
          <div class="modal-dialog" role="document">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title" id="editTransferModalLabel">Transfer Hareketini Düzenle</h5>
                <!-- Bootstrap 4 kapatma butonu -->
                <button type="button" class="close" onclick="closeEditTransferModal()" aria-label="Kapat">
                  <span aria-hidden="true">&times;</span>
                </button>
              </div>
              <div class="modal-body">
                <asp:UpdatePanel ID="UpdatePanelEditTransfer" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                  <ContentTemplate>
                    <asp:HiddenField ID="hfEditTransferID" runat="server" />
                    <!-- Kaynak Banka ve Hesap -->
                    <div class="form-group row">
                      <div class="col-md-6">
                        <label>Kaynak Banka</label>
                        <asp:DropDownList ID="ddlEditSourceBank" runat="server" CssClass="form-control"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlEditSourceBank_SelectedIndexChanged">
                        </asp:DropDownList>
                      </div>
                      <div class="col-md-6">
                        <label>Kaynak Hesap</label>
                        <asp:DropDownList ID="ddlEditSourceBankAccount" runat="server" CssClass="form-control"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlEditSourceBankAccount_SelectedIndexChanged">
                        </asp:DropDownList>
                      </div>
                    </div>
                    <!-- Hedef Banka ve Hesap -->
                    <div class="form-group row">
                      <div class="col-md-6">
                        <label>Hedef Banka</label>
                        <asp:DropDownList ID="ddlEditTargetBank" runat="server" CssClass="form-control"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlEditTargetBank_SelectedIndexChanged">
                        </asp:DropDownList>
                      </div>
                      <div class="col-md-6">
                        <label>Hedef Hesap</label>
                        <asp:DropDownList ID="ddlEditTargetBankAccount" runat="server" CssClass="form-control"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlEditTargetBankAccount_SelectedIndexChanged">
                        </asp:DropDownList>
                      </div>
                    </div>
                    <!-- İşlem Tarihi -->
                    <div class="form-group">
                      <label>İşlem Tarihi</label>
                      <asp:TextBox ID="txtEditTransferDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                    <!-- Transfer Tutarı -->
                    <div class="form-group">
                      <label>Tutar</label>
                      <asp:TextBox ID="txtEditTransferAmount" runat="server" CssClass="form-control" 
                          AutoPostBack="true" OnTextChanged="txtEditTransferAmount_TextChanged"></asp:TextBox>
                    </div>
                    <!-- Döviz Kuru ve Çevrilmiş Tutar -->
                    <div class="form-group row">
                      <div class="col-md-6">
                        <label>Kur Bilgisi</label>
                        <asp:TextBox ID="txtEditExchangeRate" runat="server" CssClass="form-control" 
                            AutoPostBack="true" OnTextChanged="txtEditExchangeRate_TextChanged"></asp:TextBox>
                      </div>
                      <div class="col-md-6">
                        <label>Giden Tutar</label>
                        <asp:TextBox ID="txtEditConvertedAmount" runat="server" CssClass="form-control" 
                            AutoPostBack="true" OnTextChanged="txtEditConvertedAmount_TextChanged"></asp:TextBox>
                      </div>
                    </div>
                    <!-- Tek Açıklama Alanı -->
                    <div class="form-group">
                      <label>Açıklama</label>
                      <asp:TextBox ID="txtEditTransferDescription" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                  </ContentTemplate>
                  <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="txtEditTransferAmount" EventName="TextChanged" />
                    <asp:AsyncPostBackTrigger ControlID="txtEditExchangeRate" EventName="TextChanged" />
                    <asp:AsyncPostBackTrigger ControlID="txtEditConvertedAmount" EventName="TextChanged" />
                    <asp:AsyncPostBackTrigger ControlID="btnUpdateTransfer" EventName="Click" />
                  </Triggers>
                </asp:UpdatePanel>
              </div>
              <div class="modal-footer">
                <asp:Button ID="btnUpdateTransfer" runat="server" CssClass="btn btn-warning" Text="Güncelle" 
                    OnClick="btnUpdateTransfer_Click" UseSubmitBehavior="false" />
                <button type="button" class="btn btn-secondary" onclick="closeEditTransferModal();">Kapat</button>
              </div>
            </div>
          </div>
        </div>



        <!--   Detay Görüntüle Modalı -->

        <!-- Hareket Detay Modalı -->
        <div class="modal fade" id="detailModal" tabindex="-1" role="dialog" aria-labelledby="detailModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-lg" role="document"> 
            <!-- modal-lg => geniş modal; isteğe bağlı -->
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title" id="detailModalLabel">Hareket Detayları</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Kapat">
                  <span aria-hidden="true">&times;</span>
                </button>
              </div>
              <div class="modal-body">
                <!-- Card tasarımı -->
                <div class="card">
                  <div class="card-body">
                    <h5 class="card-title mb-3" style="font-weight:600;">
                      <!-- Örneğin fiş no veya özel bir başlık: -->
                      Fiş No: 
                      <asp:Label ID="lblDetailHareketNo" runat="server" CssClass="text-primary" />
                    </h5>

                    <!-- SATIR 1 -->
                    <div class="row mb-2">
                      <div class="col-md-6">
                        <p class="mb-1 text-muted">Tarih</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailTarih" runat="server" />
                        </h6>
                      </div>
                      <div class="col-md-6">
                        <p class="mb-1 text-muted">Banka</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailBanka" runat="server" />
                        </h6>
                      </div>
                    </div>

                    <!-- SATIR 2 -->
                    <div class="row mb-2">
                      <div class="col-md-6">
                        <p class="mb-1 text-muted">Tutar</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailTutar" runat="server" />
                        </h6>
                      </div>
                      <div class="col-md-6">
                        <p class="mb-1 text-muted">Hareket Türü</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailHareketTuru" runat="server" />
                        </h6>
                      </div>
                    </div>

                    <!-- SATIR 3 -->
                    <div class="row mb-2">
                      <div class="col-md-6">
                        <p class="mb-1 text-muted">Cari Adı (veya ID)</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailCariAdi" runat="server" />
                        </h6>
                      </div>
                      <div class="col-md-6">
                        <p class="mb-1 text-muted">Açıklama</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailAciklama" runat="server" />
                        </h6>
                      </div>
                    </div>

                    <hr/>

                    <!-- SATIR 4 - Daha fazla alan: TransferID, TransferYonu vb. -->
                    <div class="row mb-2">
                      <div class="col-md-6">
                        <p class="mb-1 text-muted">Transfer ID</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailTransferID" runat="server" />
                        </h6>
                      </div>
                      <div class="col-md-6">
                        <p class="mb-1 text-muted">Transfer Yönü</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailTransferYonu" runat="server" />
                        </h6>
                      </div>
                    </div>

                    <hr/>

                    <!-- SATIR 5 - Kullanıcı bilgileri, Oluşturma tarihi vb. -->
                    <div class="row">
                      <div class="col-md-4">
                        <p class="mb-1 text-muted">Oluşturan Kullanıcı ID</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailOlusturanKullaniciID" runat="server" />
                        </h6>
                      </div>
                      <div class="col-md-4">
                        <p class="mb-1 text-muted">Son Günc. Kullanıcı ID</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailSonGuncelleyenKullaniciID" runat="server" />
                        </h6>
                      </div>
                      <div class="col-md-4">
                        <p class="mb-1 text-muted">Güncelleme Tarihi</p>
                        <h6 class="mb-0">
                          <asp:Label ID="lblDetailGuncellemeTarihi" runat="server" />
                        </h6>
                      </div>
                    </div>

                  </div> <!-- end card-body -->
                </div> <!-- end card -->
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Kapat</button>
              </div>
            </div>
          </div>
        </div>

         <!--   Detay Görüntüle Modalı -->

    </div>

    <!-- JavaScript Kodu (TL/UZS -> USD, USD -> TL/UZS vb.) -->
    <script type="text/javascript">
        // Sunucu tarafından render edilen ClientID değerlerini değişkenlere atayalım
        var ddlTransactionBankClientID = '<%= ddlTransactionBank.ClientID %>';
        var ddlSourceBankClientID = '<%= ddlSourceBank.ClientID %>';
        var ddlSourceBankAccountClientID = '<%= ddlSourceBankAccount.ClientID %>';
        var ddlTargetBankAccountClientID = '<%= ddlTargetBankAccount.ClientID %>';
        var txtExchangeRateClientID = '<%= txtExchangeRate.ClientID %>';
        var txtTransferAmountClientID = '<%= txtTransferAmount.ClientID %>';
        var txtConvertedAmountClientID = '<%= txtConvertedAmount.ClientID %>';
    var btnSaveTransactionClientID = '<%= btnSaveTransaction.ClientID %>';
    var btnSaveTransferClientID = '<%= btnSaveTransfer.ClientID %>';


        function closeEditTransferModal() {
            $('#editTransferModal').modal('hide');
            // Modal kapandıktan sonra aşağıdaki kodu ekleyebilirsiniz:
            $('#editTransferModal').on('hidden.bs.modal', function () {
                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();
            });
        }


    // Global validasyon fonksiyonu (edit modal için)
    window.validateEditForm = function () {
        let isValid = true;
        $('#editTransactionModal').find('select[required], input[required]').each(function () {
            if (!$(this).val() || $(this).val().trim() === "") {
                $(this).addClass('is-invalid');
                if (isValid) { $(this).focus(); }
                isValid = false;
            } else {
                $(this).removeClass('is-invalid');
            }
        });
        if (!isValid) {
            Swal.fire({
                title: 'Hata!',
                text: 'Lütfen tüm zorunlu alanları doldurun!',
                icon: 'error'
            });
        }
        return isValid;
    };

    // Modal yardımcı nesnesi
    const ModalHelper = {
        show: function (modalId, triggerId) {
            const $modal = $('#' + modalId);
            if ($modal.length) {
                $modal.modal('show');
                if (triggerId) {
                    $modal.find('#' + triggerId).trigger('change');
                }
            }
        },
        hide: function (modalId) {
            const $modal = $('#' + modalId);
            if ($modal.length) {
                $modal.modal('hide');
            }
        },
        clear: function (modalId) {
            const $modal = $('#' + modalId);
            if ($modal.length) {
                $modal.find('input:not([type="hidden"]), select, textarea').val('');
                $modal.find('.is-invalid').removeClass('is-invalid');
                $modal.find('.balance-label').text('').hide();
            }
        }
    };

    // Modal açma fonksiyonları
    function showAddTransactionModal() {
        ModalHelper.show('addTransactionModal', ddlTransactionBankClientID);
    }

    function showTransferModal() {
        ModalHelper.show('transferModal', ddlSourceBankClientID);
    }

    function showEditTransactionModal() {
        ModalHelper.show('editTransactionModal');
    }

        function showDetailModal() {
            $('#detailModal').modal('show');
        }

        function showEditTransferModal() {
            var $modal = $('#editTransferModal');
            if ($modal.length) {
                $modal.modal('show');
            }
        }

    // Modal kapatma ve temizleme
    function closeEditTransactionModal() {
        ModalHelper.hide('editTransactionModal');
        setTimeout(() => ModalHelper.clear('editTransactionModal'), 100);
    }

    // Filtre paneli fonksiyonları
    function toggleFilterPanel() {
        var filterPanel = document.getElementById("filterPanel");
        if (!filterPanel) return;
        var isOpen = (filterPanel.style.display === "block");
        filterPanel.style.display = isOpen ? "none" : "block";
        var btn = document.getElementById("btnToggleFilter");
        if (btn) {
            btn.innerHTML = isOpen ? '<i class="fa fa-filter"></i> Filtrele' : '<i class="fa fa-filter"></i> Filtreyi Kapat';
        }
    }

    function resetFilters() {
        $('#filterPanel select, #filterPanel input').val('');
        $('#filterPanel').hide();
        $('#btnToggleFilter').html('<i class="fa fa-filter"></i> Filtrele');
        __doPostBack('ResetFilters', '');
    }

    // Döviz kuru hesaplama
    async function checkExchangeRate() {
        try {
            var sourceID = $('#' + ddlSourceBankAccountClientID).val();
            var targetID = $('#' + ddlTargetBankAccountClientID).val();
            if (!sourceID || !targetID) return;
            const symbolsResponse = await AjaxHelper.call('BankTransactions.aspx/GetCurrencySymbols', {
                sourceBankAccountID: sourceID,
                targetBankAccountID: targetID
            });
            const { sourceCurrency, targetCurrency } = symbolsResponse.d;
            if (sourceCurrency === targetCurrency) {
                $('#' + txtExchangeRateClientID).val('1').prop('disabled', true);
                var amt = $('#' + txtTransferAmountClientID).val() || '0';
                $('#' + txtConvertedAmountClientID).val(amt).prop('disabled', true);
                return;
            }
            $('#' + txtExchangeRateClientID + ', #' + txtConvertedAmountClientID).prop('disabled', false);
            const rateResponse = await AjaxHelper.call('BankTransactions.aspx/GetExchangeRate', {
                sourceCurrency: sourceCurrency,
                targetCurrency: targetCurrency
            });
            const currentRate = rateResponse.d;
            let rate, displayRate;
            if ((sourceCurrency === 'TL' || sourceCurrency === 'UZS') && targetCurrency === 'USD') {
                rate = 1 / currentRate;
                displayRate = '1/' + CurrencyHelper.format(currentRate, 8);
            } else {
                rate = currentRate;
                displayRate = CurrencyHelper.format(currentRate, 8);
            }
            $('#' + txtExchangeRateClientID).val(displayRate);
            var transferAmount = CurrencyHelper.parse($('#' + txtTransferAmountClientID).val());
            $('#' + txtConvertedAmountClientID).val(CurrencyHelper.calculateConverted(transferAmount, rate));
        } catch (ex) {
            console.error('Exchange rate error:', ex);
            Swal.fire({
                title: 'Hata!',
                text: 'Kur bilgisi alınamadı.',
                icon: 'error'
            });
        }
    }

    // Form validasyonu
    function validateForm($form) {
        let isValid = true;
        $form.find('select[required], input[required]').each(function () {
            if (!$(this).val()?.trim()) {
                $(this).addClass('is-invalid');
                if (isValid) $(this).focus();
                isValid = false;
            } else {
                $(this).removeClass('is-invalid');
            }
        });
        if (!isValid) {
            Swal.fire({ title: 'Hata!', text: 'Lütfen tüm zorunlu alanları doldurun!', icon: 'error' });
        }
        return isValid;
    }

    // Document Ready
    $(document).ready(function () {
        try {
            $('#basic-1').DataTable({
                "order": [[1, "desc"]],
                "language": { "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Turkish.json" }
            });
            $('.modal').on({
                'show.bs.modal': function () {
                    $(this).find('.is-invalid').removeClass('is-invalid');
                },
                'hidden.bs.modal': function () {
                    ModalHelper.clear(this.id);
                }
            });
            // Yeni işlem kaydet
            $('#' + btnSaveTransactionClientID).on('click', function (e) {
                e.preventDefault();
                const $modal = $('#addTransactionModal');
                if (!validateForm($modal)) return;
                var amt = CurrencyHelper.parse($('#<%= txtTransactionAmount.ClientID %>').val());
                if (amt <= 0) {
                    $('#<%= txtTransactionAmount.ClientID %>').addClass('is-invalid');
                    Swal.fire({ title: 'Hata!', text: 'Lütfen geçerli bir tutar girin!', icon: 'error' });
                    return;
                }
                __doPostBack('<%= btnSaveTransaction.UniqueID %>', '');
            });
            // Transfer kaydet
            $('#' + btnSaveTransferClientID).on('click', function (e) {
                e.preventDefault();
                if (validateForm($('#transferModal'))) {
                    __doPostBack('<%= btnSaveTransfer.UniqueID %>', '');
                }
            });
            // Transfer hesaplamaları
            $('#' + txtTransferAmountClientID + ', #' + txtExchangeRateClientID).on('input', function () {
                var amt = CurrencyHelper.parse($('#' + txtTransferAmountClientID).val());
                var rate = CurrencyHelper.parse($('#' + txtExchangeRateClientID).val());
                $('#' + txtConvertedAmountClientID).val(CurrencyHelper.calculateConverted(amt, rate));
            });
            $('#' + txtConvertedAmountClientID).on('input', function () {
                var convAmt = CurrencyHelper.parse($(this).val());
                var rate = CurrencyHelper.parse($('#' + txtExchangeRateClientID).val());
                if (rate !== 0) {
                    $('#' + txtTransferAmountClientID).val(CurrencyHelper.format(convAmt / rate));
                }
            });
            // Prevent Enter key form submit
            $('form').on('keypress', function (e) {
                return e.which !== 13;
            });
        } catch (ex) {
            console.error('Document ready error:', ex);
        }
    });
    </script>



</asp:Content>