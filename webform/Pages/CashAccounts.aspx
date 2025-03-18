<%@ Page Title="Kasalar" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="CashAccounts.aspx.cs" Inherits="MaOaKApp.Pages.CashAccounts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:HiddenField ID="hfEditCashAccountId" runat="server" />
    <asp:HiddenField ID="hfDeleteCashAccountId" runat="server" />

    <div class="container mt-4">
        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h4 class="mb-0">Kasalar</h4>
                <button class="btn btn-primary" type="button" onclick="showAddCashAccountModal()">+ Yeni Kasa</button>
            </div>
            <div class="card-body">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <table class="table table-striped" id="basic-1">
                            <thead>
                                <tr>
                                    <th>Kasa Adı</th>
                                    <th>Bakiye</th>
                                    <th>Döviz Türü</th>
                                    <th>İşlemler</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptCashAccounts" runat="server" OnItemCommand="rptCashAccounts_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("KasaAdi") %></td>
                                            <td><%# Eval("Bakiye", "{0:N2}") %> <%# Eval("DovizSembol") %></td>
                                            <td><%# Eval("DovizSembol") %></td>
                                            <td>
                                                <asp:LinkButton runat="server" CssClass="btn btn-outline-info btn-sm"
                                                                CommandName="Edit" 
                                                                CommandArgument='<%# Eval("KasaID") %>'>
                                                    <i class="fa fa-edit"></i> Düzenle
                                                </asp:LinkButton>

                                                <asp:LinkButton runat="server" CssClass="btn btn-outline-primary btn-sm"
                                                                CommandName="Transactions" 
                                                                CommandArgument='<%# Eval("KasaID") %>'>
                                                    <i class="fa fa-list"></i> Kasa Hareketleri
                                                </asp:LinkButton>

                                                  <asp:LinkButton runat="server" CssClass="btn btn-outline-danger btn-sm"
                                                                CommandName="Delete"
                                                                CommandArgument='<%# Eval("KasaID") %>'
                                                                OnClientClick='<%# "return confirmDelete(\"" + Eval("KasaID") + "\");" %>'>
                                                    <i class="fa fa-trash"></i> Sil
                                                </asp:LinkButton>


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



    <!-- Kasa Güncelle Modalı -->
    <div class="modal fade" id="editCashAccountModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Kasa Güncelle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfEditKasaID" runat="server" />
                    <div class="mb-3">
                        <label>Kasa Adı</label>
                        <asp:TextBox ID="txtEditCashAccountName" runat="server" CssClass="form-control" />
                    </div>
                    <div class="mb-3">
                        <label>Bakiye</label>
                        <asp:TextBox ID="txtEditBalance" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event);" Enabled="false" />
                    </div>
                    <div class="mb-3">
                        <label>Döviz Türü</label>
                        <asp:DropDownList ID="ddlEditCurrency" runat="server" CssClass="form-control" Enabled="false"></asp:DropDownList>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnUpdateCashAccount" runat="server" CssClass="btn btn-success" Text="Güncelle" OnClick="btnUpdateCashAccount_Click" />
                </div>
            </div>
        </div>
    </div>


    <!-- Yeni Kasa Ekle Modalı -->
    <div class="modal fade" id="addCashAccountModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Yeni Kasa Ekle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanelAdd" runat="server">
                        <ContentTemplate>
                            <div class="mb-3">
                                <label>Kasa Adı</label>
                                <asp:TextBox ID="txtNewCashAccountName" runat="server" CssClass="form-control" />
                            </div>
                            <div class="mb-3">
                                <label>Bakiye</label>
                                <asp:TextBox ID="txtNewBalance" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event);" />
                            </div>
                            <div class="mb-3">
                                <label>Döviz Türü</label>
                                <asp:DropDownList ID="ddlNewCurrency" runat="server" CssClass="form-control"></asp:DropDownList>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnSaveNewCashAccount" runat="server" CssClass="btn btn-primary" Text="Kaydet" OnClick="btnSaveNewCashAccount_Click" />
                </div>
            </div>
        </div>
    </div>
  <!-- jQuery ve Bootstrap JavaScript Dosyaları -->
<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

<script>
    function showEditModal() {
        if (typeof $ === 'undefined') {
            console.error("jQuery yüklenmemiş!");
            return;
        }

        if (typeof bootstrap === 'undefined') {
            console.error("Bootstrap JS yüklenmemiş!");
            return;
        }

        setTimeout(function () {
            var myModal = new bootstrap.Modal(document.getElementById('editCashAccountModal'));
            myModal.show();
        }, 300);
    }

    function confirmDelete(kasaID) {
        Swal.fire({
            title: "Silmek istediğinize emin misiniz?",
            text: "Bu işlem geri alınamaz!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Evet, sil!",
            cancelButtonText: "İptal"
        }).then((result) => {
            if (result.isConfirmed) {
                __doPostBack('DeleteCashAccount', kasaID);
            }
        });
        return false; // Formun otomatik gitmesini engelle
    }

    function showAddCashAccountModal() {
        if (typeof $ === 'undefined') {
            console.error("jQuery yüklenmemiş!");
            return;
        }

        if (typeof bootstrap === 'undefined') {
            console.error("Bootstrap JS yüklenmemiş!");
            return;
        }

        setTimeout(function () {
            var myModal = new bootstrap.Modal(document.getElementById('addCashAccountModal'));
            myModal.show();
        }, 300);
    }
</script>

</asp:Content>