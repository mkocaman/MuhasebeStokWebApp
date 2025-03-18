<%@ Page Title="Ürün Yönetimi" Language="C#" MasterPageFile="~/Material.Master" AutoEventWireup="true" CodeBehind="Products.aspx.cs" Inherits="MaOaKApp.Pages.Products" %>



<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div class="container mt-4">
        <h3>Ürün Yönetimi</h3>
            <div class="text-end mb-3">
        <asp:Button ID="btnAddProduct" runat="server" CssClass="btn btn-primary" Text="Yeni Ürün Ekle"
            OnClientClick="$('#addProductModal').modal('show'); return false;" />
    </div>
        <div class="input-group input-group-outline mb-3">
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" Placeholder="Ürün Ara..."
                AutoPostBack="true" OnTextChanged="txtSearch_TextChanged" />
        </div>

        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:GridView ID="gvProducts" runat="server" CssClass="table table-striped table-hover"
    AutoGenerateColumns="False" AllowPaging="True" PageSize="10"
    OnPageIndexChanging="gvProducts_PageIndexChanging" OnRowCommand="gvProducts_RowCommand">
    <Columns>
        <asp:BoundField DataField="UrunAdi" HeaderText="Ürün Adı" />
        <asp:BoundField DataField="StokMiktar" HeaderText="Mevcut Stok" />
        <asp:BoundField DataField="ListeFiyati" HeaderText="Liste Fiyatı" DataFormatString="{0:C2}" />
        <asp:BoundField DataField="MaliyetFiyati" HeaderText="Maliyet Fiyatı" DataFormatString="{0:C2}" />
        <asp:BoundField DataField="SatisFiyati" HeaderText="Satış Fiyatı" DataFormatString="{0:C2}" />
        <asp:TemplateField>
            <ItemTemplate>
                <asp:Button ID="btnEdit" runat="server" CssClass="btn btn-warning btn-sm"
                    Text="Düzenle" CommandName="EditProduct" CommandArgument='<%# Eval("UrunID") %>' />
                <asp:Button ID="btnDelete" runat="server" CssClass="btn btn-danger btn-sm"
                    Text="Sil" CommandName="DeleteProduct" CommandArgument='<%# Eval("UrunID") %>' />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>

                <asp:Label ID="lblNoRecords" runat="server" CssClass="text-danger" Visible="false"></asp:Label>

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="txtSearch" EventName="TextChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <!-- Ürün Ekle Modal -->
    <asp:UpdatePanel ID="UpdatePanelAddProduct" runat="server">
        <ContentTemplate>
            <div class="modal fade" id="addProductModal" tabindex="-1" role="dialog" aria-labelledby="addProductModalLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="addProductModalLabel">Yeni Ürün Ekle</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="input-group input-group-outline mb-3">
                                <asp:TextBox ID="txtProductName" runat="server" CssClass="form-control" Placeholder="Ürün Adı"></asp:TextBox>
                            </div>
                            <div class="input-group input-group-outline mb-3">
                                <asp:TextBox ID="txtListeFiyati" runat="server" CssClass="form-control" Placeholder="Liste Fiyatı"></asp:TextBox>
                            </div>
                            <div class="input-group input-group-outline mb-3">
                                <asp:TextBox ID="txtMaliyetFiyati" runat="server" CssClass="form-control" Placeholder="Maliyet Fiyatı"></asp:TextBox>
                            </div>
                            <div class="input-group input-group-outline mb-3">
                                <asp:TextBox ID="txtSatisFiyati" runat="server" CssClass="form-control" Placeholder="Satış Fiyatı"></asp:TextBox>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnSaveProduct" runat="server" CssClass="btn btn-success" Text="Kaydet" OnClick="btnSaveProduct_Click" />
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Ürün Düzenle Modal -->
    <asp:UpdatePanel ID="UpdatePanelEditProduct" runat="server">
        <ContentTemplate>
            <div class="modal fade" id="editProductModal" tabindex="-1" role="dialog" aria-labelledby="editProductModalLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="editProductModalLabel">Ürün Düzenle</h5>
                               <asp:Label ID="lblError" runat="server" CssClass="text-danger"></asp:Label>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <asp:HiddenField ID="hfEditProductId" runat="server" />
                            <div class="input-group input-group-outline mb-3">
                                <asp:TextBox ID="txtEditProductName" runat="server" CssClass="form-control" Placeholder="Ürün Adı"></asp:TextBox>
                            </div>
                            <div class="input-group input-group-outline mb-3">
                                <asp:TextBox ID="txtEditListeFiyati" runat="server" CssClass="form-control" Placeholder="Liste Fiyatı"></asp:TextBox>
                            </div>
                            <div class="input-group input-group-outline mb-3">
                                <asp:TextBox ID="txtEditMaliyetFiyati" runat="server" CssClass="form-control" Placeholder="Maliyet Fiyatı"></asp:TextBox>
                            </div>
                            <div class="input-group input-group-outline mb-3">
                                <asp:TextBox ID="txtEditSatisFiyati" runat="server" CssClass="form-control" Placeholder="Satış Fiyatı"></asp:TextBox>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnUpdateProduct" runat="server" CssClass="btn btn-success" Text="Güncelle" OnClick="btnUpdateProduct_Click" />
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Silme Onayı Modal -->
    <asp:UpdatePanel ID="UpdatePanelDeleteConfirm" runat="server">
        <ContentTemplate>
            <div class="modal fade" id="deleteConfirmModal" tabindex="-1" role="dialog" aria-labelledby="deleteConfirmModalLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="deleteConfirmModalLabel">Silme Onayı</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            Bu ürünü silmek istediğinizden emin misiniz?
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnConfirmDelete" runat="server" CssClass="btn btn-danger" Text="Sil" OnClick="btnConfirmDelete_Click" />
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <script>

        function hideModal('#deleteConfirmModal') {
            $('#deleteConfirmModal').modal('hide');
            $('body').removeClass('modal-open');
            $('.modal-backdrop').remove();
        }

        function hideModal('#editProductModal') {
            $('#editProductModal').modal('hide');
            $('body').removeClass('modal-open');
            $('.modal-backdrop').remove();
        }
       
        // Postback olmadan arama yapma
        document.getElementById('<%= txtSearch.ClientID %>').addEventListener('input', function () {
            const searchText = this.value;
            __doPostBack('txtSearch', searchText);
        });


    </script>

</asp:Content>
