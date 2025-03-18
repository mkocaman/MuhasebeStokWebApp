using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace MuhasebeStokDB.Pages
{
    public partial class InvoiceDetails : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadInvoiceDetails();
            }
        }

        private void LoadInvoiceDetails()
        {
            string invoiceID = Request.QueryString["InvoiceID"];
            if (!string.IsNullOrEmpty(invoiceID))
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
                            SELECT F.*, C.CariAdi, O.OdemeTuru, D.DovizAdi, D.DovizKodu
                            FROM Faturalar F
                            INNER JOIN Cariler C ON F.CariID = C.CariID
                            LEFT JOIN OdemeTurleri O ON F.OdemeTuruID = O.OdemeTuruID
                            LEFT JOIN Dovizler D ON F.DovizTuru = D.DovizKodu
                            WHERE F.FaturaID = @FaturaID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@FaturaID", invoiceID);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        lblFaturaNumarasi.Text = reader["FaturaNumarasi"].ToString();
                        lblCariAdi.Text = reader["CariAdi"].ToString();
                        lblFaturaTarihi.Text = Convert.ToDateTime(reader["FaturaTarihi"]).ToString("dd.MM.yyyy");
                        lblVadeTarihi.Text = Convert.ToDateTime(reader["VadeTarihi"]).ToString("dd.MM.yyyy");
                        lblOdemeTuru.Text = reader["OdemeTuru"].ToString();
                        lblDovizTuru.Text = reader["DovizAdi"].ToString();
                        lblDovizKuru.Text = reader["DovizKuru"].ToString();
                        lblFaturaNotu.Text = reader["FaturaNotu"].ToString();
                        string currencySymbol = GetCurrencySymbol(reader["DovizKodu"].ToString());
                        lblAraToplam.Text = Convert.ToDecimal(reader["AraToplam"]).ToString("N2") + " " + currencySymbol;
                        lblKdvToplam.Text = Convert.ToDecimal(reader["KDVToplam"]).ToString("N2") + " " + currencySymbol;
                        lblGenelToplam.Text = Convert.ToDecimal(reader["GenelToplam"]).ToString("N2") + " " + currencySymbol;
                    }
                }

                // Fatura detaylarını yükle
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
                            SELECT FD.*, U.UrunAdi, D.DovizKodu 
                            FROM FaturaDetaylari FD
                            INNER JOIN Urunler U ON FD.UrunID = U.UrunID
                            INNER JOIN Faturalar F ON FD.FaturaID = F.FaturaID
                            LEFT JOIN Dovizler D ON F.DovizTuru = D.DovizKodu
                            WHERE FD.FaturaID = @FaturaID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@FaturaID", invoiceID);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    var details = new List<InvoiceDetail>();
                    while (reader.Read())
                    {
                        details.Add(new InvoiceDetail
                        {
                            UrunAdi = reader["UrunAdi"].ToString(),
                            Miktar = Convert.ToInt32(reader["Miktar"]),
                            BirimFiyat = Convert.ToDecimal(reader["BirimFiyat"]),
                            SatirKDV = Convert.ToDecimal(reader["SatirKdvToplam"]),
                            SatirToplam = Convert.ToDecimal(reader["SatirToplam"])
                        });
                    }
                    rptInvoiceDetails.DataSource = details;
                    rptInvoiceDetails.DataBind();
                }
            }
        }

        private string GetCurrencySymbol(string currencyCode)
        {
            switch (currencyCode)
            {
                case "USD":
                    return "$";
                case "EUR":
                    return "€";
                case "GBP":
                    return "£";
                case "USD/UZS":
                    return "UZS";
                default:
                    return currencyCode;
            }
        }

        public class InvoiceDetail
        {
            public string UrunAdi { get; set; }
            public int Miktar { get; set; }
            public decimal BirimFiyat { get; set; }
            public decimal SatirKDV { get; set; }

            public decimal SatirToplam { get; set; }

        }

        protected void btnDownloadPDF_Click(object sender, EventArgs e)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    // Logo ekleme
                    string logoPath = Server.MapPath("~/images/logo.png");
                    if (File.Exists(logoPath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                        logo.ScaleToFit(140f, 120f);
                        logo.Alignment = Element.ALIGN_LEFT;
                        document.Add(logo);
                    }

                    // Fatura başlığı
                    Font titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
                    Paragraph title = new Paragraph("Fatura", titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    document.Add(title);

                    // Fatura bilgileri
                    Font infoFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
                    PdfPTable infoTable = new PdfPTable(2)
                    {
                        WidthPercentage = 100
                    };
                    infoTable.SetWidths(new float[] { 30, 70 });
                    infoTable.AddCell(new PdfPCell(new Phrase("Fatura Numarası:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblFaturaNumarasi.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("Fatura Tarihi:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblFaturaTarihi.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("Vade Tarihi:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblVadeTarihi.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("Cari Adı:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblCariAdi.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("Ödeme Türü:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblOdemeTuru.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("Döviz Türü:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblDovizTuru.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("Döviz Kuru:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblDovizKuru.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("Fatura Notu:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblFaturaNotu.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    document.Add(infoTable);

                    // Fatura detayları tablosu
                    PdfPTable table = new PdfPTable(2)
                    {
                        WidthPercentage = 100
                    };
                    table.SetWidths(new float[] { 70, 30 });

                    // Tablo başlıkları
                    table.AddCell(new PdfPCell(new Phrase("Ürün Adı", infoFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Fiyat", infoFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                    // Tablo satırları
                    foreach (RepeaterItem item in rptInvoiceDetails.Items)
                    {
                        Label lblUrunAdi = (Label)item.FindControl("lblUrunAdi");
                        Label lblSatirToplam = (Label)item.FindControl("lblSatirToplam");

                        if (lblUrunAdi != null && lblSatirToplam != null)
                        {
                            table.AddCell(new PdfPCell(new Phrase(lblUrunAdi.Text, infoFont)));
                            table.AddCell(new PdfPCell(new Phrase(lblSatirToplam.Text, infoFont)));
                        }
                    }

                    document.Add(table);

                    // Toplamlar
                    document.Add(new Paragraph("Ara Toplam: " + lblAraToplam.Text, infoFont));
                    document.Add(new Paragraph("KDV Toplam: " + lblKdvToplam.Text, infoFont));
                    document.Add(new Paragraph("Genel Toplam: " + lblGenelToplam.Text, infoFont));

                    document.Close();
                    writer.Close();

                    string fileName = lblFaturaNumarasi.Text + ".pdf";
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", $"attachment;filename={fileName}");
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.BinaryWrite(ms.ToArray());
                    Response.Flush();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                lblMessage.Text = "Hata oluştu: " + ex.Message;
                lblMessage.CssClass = "text-danger";
            }
        }
    }
}