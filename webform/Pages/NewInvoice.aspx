<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewInvoice.aspx.cs" Inherits="MaOaKApp.Pages.NewInvoice" MasterPageFile="~/Material.Master" EnableViewState="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><asp:Label ID="lblBaslik" runat="server"></asp:Label></h2>
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
    <asp:HiddenField ID="hfInvoiceDetails" runat="server" />
    <asp:HiddenField ID="hfInvoiceNumber" runat="server" />
    <!-- Müşteri Ekle Modal -->
    <asp:UpdatePanel ID="UpdatePanelAddCustomer" runat="server">

        <ContentTemplate>
    <asp:HiddenField ID="hfHareketTuru" runat="server" />

            <div class="modal fade" id="addCustomerModal" tabindex="-1" role="dialog" aria-labelledby="addCustomerModalLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="addCustomerModalLabel">Yeni Müşteri Ekle</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <asp:Panel ID="pnlAddCustomer" runat="server">
                                <div class="input-group input-group-outline mb-3">
                                    <asp:TextBox ID="txtCustomerName" runat="server" CssClass="form-control" Placeholder="Müşteri Adı"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvCustomerName" runat="server" ControlToValidate="txtCustomerName" ErrorMessage="Müşteri Adı zorunludur." CssClass="text-danger" Display="Dynamic" ValidationGroup="AddCustomer" />
                                </div>
                                <div class="input-group input-group-outline mb-3">
                                    <asp:TextBox ID="txtVergiNo" runat="server" CssClass="form-control" Placeholder="Vergi No"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="revVergiNo" runat="server" ControlToValidate="txtVergiNo" ErrorMessage="Vergi No sadece rakam olmalıdır." ValidationExpression="^\d+$" CssClass="text-danger" Display="Dynamic" ValidationGroup="AddCustomer" />
                                </div>
                                <div class="input-group input-group-outline mb-3">
                                    <asp:TextBox ID="txtTelefon" runat="server" CssClass="form-control" Placeholder="Telefon"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="revTelefon" runat="server" ControlToValidate="txtTelefon" ErrorMessage="Geçerli bir telefon numarası giriniz." ValidationExpression="^\+?\d{10,15}$" CssClass="text-danger" Display="Dynamic" ValidationGroup="AddCustomer" />
                                </div>
                                <div class="input-group input-group-outline mb-3">
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" Placeholder="E-posta"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Geçerli bir e-posta adresi giriniz." ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" CssClass="text-danger" Display="Dynamic" ValidationGroup="AddCustomer" />
                                </div>
                                <div class="input-group input-group-outline mb-3">
                                    <asp:TextBox ID="txtAdres" runat="server" CssClass="form-control" Placeholder="Adres"></asp:TextBox>
                                </div>
                                <div class="input-group input-group-outline mb-3">
                                    <asp:TextBox ID="txtAciklama" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" Placeholder="Açıklama"></asp:TextBox>
                                </div>
                            </asp:Panel>
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnSaveCustomer" runat="server" CssClass="btn btn-success" Text="Kaydet" OnClick="btnSaveCustomer_Click" ValidationGroup="AddCustomer" />
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Cari Seçimi -->
    <div class="input-group input-group-outline mb-3">
        <asp:UpdatePanel ID="upFaturaNumarasi" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Label ID="lblFaturaNumarasi" runat="server" Text="Fatura Numarası"></asp:Label>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlFaturaTuru" EventName="SelectedIndexChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <div class="form-row mb-3">
        <div class="col">
            <div class="input-group input-group-outline">
                <asp:DropDownList ID="ddlFaturaTuru" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlFaturaTuru_SelectedIndexChanged">
                    <asp:ListItem Text="Fatura Türü Seçin" Value="" />
                </asp:DropDownList>
            </div>
        </div>
        <div class="col">
            <div class="input-group input-group-outline">
                <asp:DropDownList ID="ddlCari" runat="server" CssClass="form-control" AppendDataBoundItems="true">
                    <asp:ListItem Text="Müşteri Seçiniz" Value="" />
                </asp:DropDownList>
                <asp:Button ID="btnAddCustomer" runat="server" Text="Müşteri Ekle" CssClass="btn btn-secondary" OnClientClick="openAddCustomerModal(); return false;" />
            </div>
        </div>
    </div>

    <div class="form-group mb-3">
        <div class="form-check">
            <asp:CheckBox ID="chkResmi" runat="server" CssClass="form-check-input" />
            <label class="form-check-label" for="chkResmi">Resmi Fatura</label>
        </div>
    </div>

    <div class="form-row mb-3">
        <div class="col">
            <div class="input-group input-group-outline">
                <asp:TextBox ID="txtFaturaTarihi" runat="server" CssClass="form-control" TextMode="Date" placeholder="Fatura Tarihi" />
            </div>
        </div>
        <div class="col">
            <div class="input-group input-group-outline">
                <asp:TextBox ID="txtVadeTarihi" runat="server" CssClass="form-control" TextMode="Date" placeholder="Vade Tarihi" />
            </div>
        </div>
    </div>

    <div class="form-row mb-3">
        <div class="col">
            <div class="input-group input-group-outline">
                <asp:DropDownList ID="ddlOdemeTuru" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Ödeme Türü Seçin" Value="" />
                </asp:DropDownList>
            </div>
        </div>
        <asp:UpdatePanel ID="upDoviz" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="col">
                    <div class="input-group input-group-outline">
                        <asp:DropDownList ID="ddlDovizTuru" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlDovizTuru_SelectedIndexChanged">
                            <asp:ListItem Text="Döviz Türü Seçin" Value="" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col">
                    <div class="input-group input-group-outline">
                        <asp:TextBox ID="txtDovizKuru" runat="server" CssClass="form-control" placeholder="Döviz Kuru" />
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlDovizTuru" EventName="SelectedIndexChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <div class="form-group mb-3">
        <div class="input-group input-group-outline">
            <asp:TextBox ID="txtFaturaNotu" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8" placeholder="Fatura Notu" />
        </div>
    </div>

    <!-- Fatura Detayları -->
    <div class="table-responsive">
        <table id="tblInvoiceDetails" class="table table-striped table-bordered">
            <thead class="thead-dark">
                <tr>
                    <th>Ürün</th>
                    <th>Miktar</th>
                    <th>Birim</th>
                    <th>Birim Fiyat</th>
                    <th>KDV Tutarı</th>
                    <th>Satır Toplamı</th>
                    <th>İşlem</th>
                </tr>
            </thead>
            <tbody id="invoiceDetailsBody">
                <!-- JavaScript ile satırlar eklenecek -->
            </tbody>
        </table>
    </div>

    <!-- Satır Ekle -->
    <button type="button" id="btnAddRow" class="btn btn-primary">Satır Ekle</button>

    <hr />
    <div class="row">
        <div class="col">
            <p>Ara Toplam: <asp:Label ID="lblAraToplam" runat="server" Text="0.00"></asp:Label></p>
            <p>KDV Toplam: <asp:Label ID="lblKdvToplam" runat="server" Text="0.00"></asp:Label></p>
            <p>Genel Toplam: <asp:Label ID="lblGenelToplam" runat="server" Text="0.00"></asp:Label></p>
        </div>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger"></asp:Label>
    <asp:Panel ID="pnlBtnSave" runat="server">
    <asp:Button ID="btnSaveInvoice" runat="server" Text="Kaydet" CssClass="btn btn-success" OnClick="BtnSaveInvoice_Click" />
    </asp:Panel>
    <asp:Panel ID="pnlBtnUpdate" runat="server">    
    <asp:Button ID="btnUpdateInvoice" runat="server" Text="Güncelle" CssClass="btn btn-success" OnClick="BtnUpdateInvoice_Click" />
    </asp:Panel>    


    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script> <!-- SweetAlert Kütüphanesini ekleyin -->

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const tableBody = document.querySelector("#invoiceDetailsBody");
            const lblAraToplam = document.getElementById("<%= lblAraToplam.ClientID %>");
            const lblKdvToplam = document.getElementById("<%= lblKdvToplam.ClientID %>");
            const lblGenelToplam = document.getElementById("<%= lblGenelToplam.ClientID %>");
            const btnAddRow = document.querySelector("#btnAddRow");
            const chkResmi = document.getElementById("<%= chkResmi.ClientID %>");
            const hfInvoiceDetails = document.querySelector("#<%= hfInvoiceDetails.ClientID %>");
            const hfInvoiceNumber = document.querySelector("#<%= hfInvoiceNumber.ClientID %>");

            var productOptions = <%= ProductOptionsJson %>; // JSON verisini doğru şekilde ekleyin
            var unitOptions = <%= UnitOptionsJson %>;

            function getKdvOrani() {
                return chkResmi.checked ? 0.12 : 0.12; // %12 KDV
            }

            function calculateTotals() {
                let araToplam = 0;
                let kdvToplam = 0;
                const kdvOrani = getKdvOrani();

                tableBody.querySelectorAll("tr").forEach(row => {
                    const miktarInput = row.querySelector(".miktar");
                    const birimFiyatInput = row.querySelector(".birimFiyat");
                    const satirKdvElement = row.querySelector(".satirKdv");
                    const satirToplamElement = row.querySelector(".satirToplam");

                    if (!miktarInput || !birimFiyatInput || !satirKdvElement || !satirToplamElement) return;

                    const miktar = parseFloat(miktarInput.value) || 0;
                    const birimFiyat = parseFloat(birimFiyatInput.value) || 0;
                    const satirKdv = miktar * birimFiyat * kdvOrani;
                    const satirToplam = miktar * birimFiyat + satirKdv;

                    satirKdvElement.textContent = satirKdv.toFixed(2);
                    satirToplamElement.textContent = satirToplam.toFixed(2);
                    araToplam += miktar * birimFiyat;
                    kdvToplam += satirKdv;
                });

                const genelToplam = araToplam + kdvToplam;

                lblAraToplam.innerText = araToplam.toFixed(2);
                lblKdvToplam.innerText = kdvToplam.toFixed(2);
                lblGenelToplam.innerText = genelToplam.toFixed(2);
            }

            function addRow(productID, productName, quantity, unitID, unitPrice, lineVatTotal, lineTotal) {
                const newRow = document.createElement("tr");

                let productOptionsHtml = '';
                productOptions.forEach(product => {
                    productOptionsHtml += `<option value="${product.ProductID}" ${product.ProductID === productID ? 'selected' : ''}>${product.ProductName}</option>`;
                });

                let unitOptionsHtml = '';
                unitOptions.forEach(unit => {
                    unitOptionsHtml += `<option value="${unit.UnitID}" ${unit.UnitID === unitID ? 'selected' : ''}>${unit.UnitName}</option>`;
                });

                newRow.innerHTML = `
                    <td>
                        <select class="form-control urun">
                            ${productOptionsHtml}
                        </select>
                    </td>
                    <td><input type="number" class="form-control miktar" value="${quantity}" /></td>
                    <td>
                        <select class="form-control birim">
                            ${unitOptionsHtml}
                        </select>
                    </td>
                    <td><input type="number" class="form-control birimFiyat" value="${unitPrice}" /></td>
                    <td class="satirKdv">${lineVatTotal}</td>
                    <td class="satirToplam">${lineTotal}</td>
                    <td><button type="button" class="btn btn-danger btnRemoveRow">Sil</button></td>
                `;

                tableBody.appendChild(newRow);
            }

            function loadInvoiceDetails() {
                const details = JSON.parse(hfInvoiceDetails.value);
                details.forEach(detail => {
                    addRow(detail.ProductID, detail.ProductName, detail.Quantity, detail.UnitID, detail.UnitPrice, detail.LineVatTotal, detail.LineTotal);
                });
                calculateTotals();
            }

            function getInvoiceDetails() {
                const details = [];
                tableBody.querySelectorAll("tr").forEach(row => {
                    const urun = row.querySelector(".urun").value;
                    const miktar = row.querySelector(".miktar").value;
                    const birim = row.querySelector(".birim").value;
                    const birimFiyat = row.querySelector(".birimFiyat").value;
                    const satirKdv = row.querySelector(".satirKdv").textContent;
                    const satirToplam = row.querySelector(".satirToplam").textContent;

                    details.push({
                        ProductID: urun,
                        Quantity: miktar,
                        UnitID: birim,
                        UnitPrice: birimFiyat,
                        LineVatTotal: satirKdv,
                        LineTotal: satirToplam
                    });
                });
                return details;
            }

            function saveInvoiceDetails() {
                const details = getInvoiceDetails();
                hfInvoiceDetails.value = JSON.stringify(details);
            }

            btnAddRow.addEventListener("click", function () {
                addRow();
                calculateTotals();
                saveInvoiceDetails();
            });

            tableBody.addEventListener("input", function () {
                calculateTotals();
                saveInvoiceDetails();
            });

            tableBody.addEventListener("click", function (e) {
                if (e.target && e.target.matches(".btnRemoveRow")) {
                    e.target.closest("tr").remove();
                    calculateTotals();
                    saveInvoiceDetails();
                }
            });

            chkResmi.addEventListener("change", calculateTotals);
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

            if (hfInvoiceDetails.value) {
                loadInvoiceDetails(); // Fatura detaylarını yükle
            }

            var activeLink = $('a.nav-link.menu-title.active');
            if (activeLink.length) {
                $('.custom-scrollbar').animate({
                    scrollTop: activeLink.offset().top - 500
                }, 1000);
            }

            $(".toggle-nav").click(function () {
                $('.nav-menu').css("left", "0px");
            });
            $(".mobile-back").click(function () {
                $('.nav-menu').css("left", "-410px");
            });

            $(".page-wrapper").attr("class", "page-wrapper " + localStorage.getItem("page-wrapper"));
            $(".page-body-wrapper").attr("class", "page-body-wrapper " + localStorage.getItem("page-body-wrapper"));

            if (localStorage.getItem("page-wrapper") === null) {
                $(".page-wrapper").addClass("compact-wrapper");
            }

            // left sidebar and horizontal menu
            if ($('#pageWrapper').hasClass('compact-wrapper')) {
                jQuery('.submenu-title').append('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                jQuery('.submenu-title').click(function () {
                    jQuery('.submenu-title').removeClass('active');
                    jQuery('.submenu-title').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                    jQuery('.submenu-content').slideUp('normal');
                    if (jQuery(this).next().is(':hidden') == true) {
                        jQuery(this).addClass('active');
                        jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                        jQuery(this).next().slideDown('normal');
                    } else {
                        jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                    }
                });
                jQuery('.submenu-content').hide();

                jQuery('.menu-title').append('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                jQuery('.menu-title').click(function () {
                    jQuery('.menu-title').removeClass('active');
                    jQuery('.menu-title').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                    jQuery('.menu-content').slideUp('normal');
                    if (jQuery(this).next().is(':hidden') == true) {
                        jQuery(this).addClass('active');
                        jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                        jQuery(this).next().slideDown('normal');
                    } else {
                        jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                    }
                });
                jQuery('.menu-content').hide();
            } else if ($('#pageWrapper').hasClass('horizontal-wrapper')) {
                var contentWidth = jQuery(window).width();
                if ((contentWidth) < '992') {
                    $('#pageWrapper').removeClass('horizontal-wrapper').addClass('compact-wrapper');
                    $('.page-body-wrapper').removeClass('horizontal-menu').addClass('sidebar-icon');
                    jQuery('.submenu-title').append('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                    jQuery('.submenu-title').click(function () {
                        jQuery('.submenu-title').removeClass('active');
                        jQuery('.submenu-title').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                        jQuery('.submenu-content').slideUp('normal');
                        if (jQuery(this).next().is(':hidden') == true) {
                            jQuery(this).addClass('active');
                            jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                            jQuery(this).next().slideDown('normal');
                        } else {
                            jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                        }
                    });
                    jQuery('.submenu-content').hide();

                    jQuery('.menu-title').append('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                    jQuery('.menu-title').click(function () {
                        jQuery('.menu-title').removeClass('active');
                        jQuery('.menu-title').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                        jQuery('.menu-content').slideUp('normal');
                        if (jQuery(this).next().is(':hidden') == true) {
                            jQuery(this).addClass('active');
                            jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                            jQuery(this).next().slideDown('normal');
                        } else {
                            jQuery(this).find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-right"></i></div>');
                        }
                    });
                    jQuery('.menu-content').hide();
                }
            }

            // toggle sidebar
            $nav = $('.main-nav');
            $header = $('.page-main-header');
            $toggle_nav_top = $('#sidebar-toggle');
            $toggle_nav_top.click(function () {
                $this = $(this);
                $nav.toggleClass('close_icon');
                $header.toggleClass('close_icon');
            });

            $(window).resize(function () {
                $nav = $('.main-nav');
                $header = $('.page-main-header');
                $toggle_nav_top = $('#sidebar-toggle');
                $toggle_nav_top.click(function () {
                    $this = $(this);
                    $nav.toggleClass('close_icon');
                    $header.toggleClass('close_icon');
                });
            });

            $body_part_side = $('.body-part');
            $body_part_side.click(function () {
                $toggle_nav_top.attr('checked', false);
                $nav.addClass('close_icon');
                $header.addClass('close_icon');
            });

            // document sidebar
            $('.mobile-sidebar').click(function () {
                $('.document').toggleClass('close')
            });

            // responsive sidebar
            var $window = $(window);
            var widthwindow = $window.width();
            (function ($) {
                "use strict";
                if (widthwindow + 17 <= 993) {
                    $toggle_nav_top.attr('checked', false);
                    $nav.addClass("close_icon");
                    $header.addClass("close_icon");
                }
            })(jQuery);

            $(window).resize(function () {
                var widthwindaw = $window.width();
                if (widthwindaw + 17 <= 991) {
                    $toggle_nav_top.attr('checked', false);
                    $nav.addClass("close_icon");
                    $header.addClass("close_icon");
                } else {
                    $toggle_nav_top.attr('checked', true);
                    $nav.removeClass("close_icon");
                    $header.removeClass("close_icon");
                }
            });

            // horizontal arrows
            var view = $("#mainnav");
            var move = "500px";
            var leftsideLimit = -500;

            // get wrapper width
            var getMenuWrapperSize = function () {
                return $('.sidebar-wrapper').innerWidth();
            };
            var menuWrapperSize = getMenuWrapperSize();

            var sliderLimit;
            if (menuWrapperSize >= 1660) {
                sliderLimit = -3000;
            } else if (menuWrapperSize >= 1440) {
                sliderLimit = -3600;
            } else {
                sliderLimit = -4200;
            }

            $("#left-arrow").addClass("disabled");
            $("#right-arrow").click(function () {
                var currentPosition = parseInt(view.css("marginLeft"));
                if (currentPosition >= sliderLimit) {
                    $("#left-arrow").removeClass("disabled");
                    view.stop(false, true).animate({
                        marginLeft: "-=" + move
                    }, {
                        duration: 400
                    });
                    if (currentPosition == sliderLimit) {
                        $(this).addClass("disabled");
                    }
                }
            });

            $("#left-arrow").click(function () {
                var currentPosition = parseInt(view.css("marginLeft"));
                if (currentPosition < 0) {
                    view.stop(false, true).animate({
                        marginLeft: "+=" + move
                    }, {
                        duration: 400
                    });
                    $("#right-arrow").removeClass("disabled");
                    $("#left-arrow").removeClass("disabled");
                    if (currentPosition >= leftsideLimit) {
                        $(this).addClass("disabled");
                    }
                }
            });

            // page active
            $(".main-navbar").find("a").removeClass("active");
            $(".main-navbar").find("li").removeClass("active");

            var current = window.location.pathname;
            $(".main-navbar ul>li a").filter(function () {
                var link = $(this).attr("href");
                if (link) {
                    if (current.indexOf(link) != -1) {
                        $(this).parents().children('a').addClass('active');
                        $(this).parents().parents().children('ul').css('display', 'block');
                        $(this).addClass('active');
                        $(this).parent().parent().parent().children('a').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                        $(this).parent().parent().parent().parent().parent().children('a').find('div').replaceWith('<div class="according-menu"><i class="fa fa-angle-down"></i></div>');
                        return false;
                    }
                }
            });

            var activeLink = $('a.nav-link.menu-title.active');
            if (activeLink.length) {
                $('.custom-scrollbar').animate({
                    scrollTop: activeLink.offset().top - 500
                }, 1000);
            }
        });
    </script>
</asp:Content>