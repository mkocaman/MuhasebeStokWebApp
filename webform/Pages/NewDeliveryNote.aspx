<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewDeliveryNote.aspx.cs" Inherits="MuhasebeStokDB.Pages.NewDeliveryNote" MasterPageFile="~/Material.Master" EnableViewState="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Yeni İrsaliye Oluştur</h2>
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <asp:HiddenField ID="hfDeliveryNoteDetails" runat="server" />
    <asp:HiddenField ID="hfDeliveryNoteNumber" runat="server" />

    <style>
        .form-control {
            border: 2px solid #ced4da; /* Varsayılan Bootstrap sınır rengi */
            border-radius: 4px; /* Köşeleri yuvarlatır */
            padding: 10px;
            box-shadow: none; /* İstenmeyen gölge efektlerini kaldırır */
        }
    </style>

    <!-- Cari Seçimi -->
    <div class="input-group input-group-outline mb-3">
        <asp:UpdatePanel ID="upIrsaliyeNumarasi" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Label ID="lblIrsaliyeNumarasi" runat="server" Text="İrsaliye Numarası"></asp:Label>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlIrsaliyeTuru" EventName="SelectedIndexChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <div class="form-row mb-3">
        <div class="col">
            <label for="ddlIrsaliyeTuru" class="form-label">İrsaliye Türü</label>
            <asp:DropDownList ID="ddlIrsaliyeTuru" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlIrsaliyeTuru_SelectedIndexChanged"></asp:DropDownList>
        </div>
        <div class="col">
            <label for="ddlCari" class="form-label">Cari</label>
            <asp:DropDownList ID="ddlCari" runat="server" CssClass="form-control"></asp:DropDownList>
        </div>
    </div>

    <div class="form-group mb-3">
        <label for="chkResmi" class="form-label">Resmi İrsaliye</label>
        <asp:CheckBox ID="chkResmi" runat="server" CssClass="form-check-input" />
    </div>

    <div class="form-row mb-3">
        <div class="col">
            <label for="txtIrsaliyeTarihi" class="form-label">İrsaliye Tarihi</label>
            <asp:TextBox ID="txtIrsaliyeTarihi" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
    </div>

    <div class="form-group mb-3">
        <label for="txtAciklama" class="form-label">Açıklama</label>
        <asp:TextBox ID="txtAciklama" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
    </div>

    <!-- İrsaliye Detayları -->
    <div class="table-responsive">
        <table id="tblDeliveryNoteDetails" class="table table-striped table-bordered">
            <thead class="thead-dark">
                <tr>
                    <th>Ürün</th>
                    <th>Miktar</th>
                    <th>Birim</th>
                    <th>Birim Fiyat</th>
                    <th>Satır Toplamı</th>
                    <th>İşlem</th>
                </tr>
            </thead>
            <tbody id="deliveryNoteDetailsBody">
                <!-- JavaScript ile satırlar eklenecek -->
            </tbody>
        </table>
    </div>

    <!-- Satır Ekle -->
    <button type="button" id="btnAddRow" class="btn btn-primary">Satır Ekle</button>

    <hr />
    <div class="row">
        <div class="col">
            <p>Genel Toplam: <asp:Label ID="lblGenelToplam" runat="server" Text="0.00"></asp:Label></p>
        </div>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger"></asp:Label>
    <asp:Button ID="btnSaveDeliveryNote" runat="server" Text="Kaydet" CssClass="btn btn-success" OnClick="BtnSaveDeliveryNote_Click" />

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const tableBody = document.querySelector("#deliveryNoteDetailsBody");
            const lblGenelToplam = document.getElementById("<%= lblGenelToplam.ClientID %>");
            const btnAddRow = document.querySelector("#btnAddRow");
            const chkResmi = document.getElementById("<%= chkResmi.ClientID %>");
            const hfDeliveryNoteDetails = document.querySelector("#<%= hfDeliveryNoteDetails.ClientID %>");
            const hfDeliveryNoteNumber = document.querySelector("#<%= hfDeliveryNoteNumber.ClientID %>");

            function calculateTotals() {
                let genelToplam = 0;

                tableBody.querySelectorAll("tr").forEach(row => {
                    const miktarInput = row.querySelector(".miktar");
                    const birimFiyatInput = row.querySelector(".birimFiyat");
                    const satirToplamElement = row.querySelector(".satirToplam");

                    if (!miktarInput || !birimFiyatInput || !satirToplamElement) return;

                    const miktar = parseFloat(miktarInput.value) || 0;
                    const birimFiyat = parseFloat(birimFiyatInput.value) || 0;
                    const satirToplam = miktar * birimFiyat;

                    satirToplamElement.textContent = satirToplam.toFixed(2);
                    genelToplam += satirToplam;
                });

                lblGenelToplam.innerText = genelToplam.toFixed(2);
            }

            function addRow() {
                const newRow = document.createElement("tr");

                let productOptionsHtml = '';
                productOptions.forEach(product => {
                    productOptionsHtml += `<option value="${product.ProductID}">${product.ProductName}</option>`;
                });

                let unitOptionsHtml = '';
                unitOptions.forEach(unit => {
                    unitOptionsHtml += `<option value="${unit.UnitID}">${unit.UnitName}</option>`;
                });

                newRow.innerHTML = `
                    <td>
                        <select class="form-control urun">
                            ${productOptionsHtml}
                        </select>
                    </td>
                    <td><input type="number" class="form-control miktar" /></td>
                    <td>
                        <select class="form-control birim">
                            ${unitOptionsHtml}
                        </select>
                    </td>
                    <td><input type="number" class="form-control birimFiyat" /></td>
                    <td class="satirToplam">0.00</td>
                    <td><button type="button" class="btn btn-danger btnRemoveRow">Sil</button></td>
                `;

                tableBody.appendChild(newRow);
            }

            function getDeliveryNoteDetails() {
                const details = [];
                tableBody.querySelectorAll("tr").forEach(row => {
                    const urun = row.querySelector(".urun").value;
                    const miktar = row.querySelector(".miktar").value;
                    const birim = row.querySelector(".birim").value;
                    const birimFiyat = row.querySelector(".birimFiyat").value;
                    const satirToplam = row.querySelector(".satirToplam").textContent;

                    details.push({
                        ProductID: urun,
                        Quantity: miktar,
                        UnitID: birim,
                        UnitPrice: birimFiyat,
                        LineTotal: satirToplam
                    });
                });
                return details;
            }

            function saveDeliveryNoteDetails() {
                const details = getDeliveryNoteDetails();
                hfDeliveryNoteDetails.value = JSON.stringify(details);
            }

            btnAddRow.addEventListener("click", function () {
                addRow();
                calculateTotals();
                saveDeliveryNoteDetails();
            });

            tableBody.addEventListener("input", function () {
                calculateTotals();
                saveDeliveryNoteDetails();
            });

            tableBody.addEventListener("click", function (e) {
                if (e.target && e.target.matches(".btnRemoveRow")) {
                    e.target.closest("tr").remove();
                    calculateTotals();
                    saveDeliveryNoteDetails();
                }
            });

            document.addEventListener("keypress", function (e) {
                const target = e.target;
                if (target.matches(".miktar, .birimFiyat")) {
                    const key = e.key;
                    if (!/[0-9.]/.test(key) && key !== "Backspace" && key !== "Delete") {
                        e.preventDefault();
                    }
                }
            });

            calculateTotals();

            var productOptions = <%= ProductOptionsJson %>;
            var unitOptions = <%= UnitOptionsJson %>;
        });
    </script>
</asp:Content>