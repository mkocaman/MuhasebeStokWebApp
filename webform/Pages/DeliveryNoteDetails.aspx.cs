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
    public partial class DeliveryNoteDetails : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["DeliveryNoteID"]))
                {
                    LoadDeliveryNoteDetails(Request.QueryString["DeliveryNoteID"]);
                }
            }
        }

        private void LoadDeliveryNoteDetails(string deliveryNoteID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT DN.IrsaliyeNumarasi, C.CariAdi, DN.IrsaliyeTarihi, DN.Aciklama, DN.GenelToplam
                FROM Irsaliyeler DN
                INNER JOIN Cariler C ON DN.CariID = C.CariID
                WHERE DN.IrsaliyeID = @IrsaliyeID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@IrsaliyeID", deliveryNoteID);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lblIrsaliyeNumarasi.Text = reader["IrsaliyeNumarasi"].ToString();
                    lblCariAdi.Text = reader["CariAdi"].ToString();
                    lblIrsaliyeTarihi.Text = Convert.ToDateTime(reader["IrsaliyeTarihi"]).ToString("dd.MM.yyyy");
                    lblAciklama.Text = reader["Aciklama"].ToString();
                    lblGenelToplam.Text = Convert.ToDecimal(reader["GenelToplam"]).ToString("N2");
                }
            }

            // İrsaliye detaylarını yükle
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT DND.*, U.UrunAdi 
                FROM IrsaliyeDetaylari DND
                INNER JOIN Urunler U ON DND.UrunID = U.UrunID
                WHERE DND.IrsaliyeID = @IrsaliyeID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@IrsaliyeID", deliveryNoteID);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    TableRow row = new TableRow();

                    TableCell cellProductName = new TableCell();
                    cellProductName.Text = reader["UrunAdi"].ToString();
                    row.Cells.Add(cellProductName);

                    TableCell cellQuantity = new TableCell();
                    cellQuantity.Text = reader["Miktar"].ToString();
                    row.Cells.Add(cellQuantity);

                    TableCell cellUnitPrice = new TableCell();
                    cellUnitPrice.Text = Convert.ToDecimal(reader["BirimFiyat"]).ToString("N2");
                    row.Cells.Add(cellUnitPrice);

                    TableCell cellLineTotal = new TableCell();
                    cellLineTotal.Text = Convert.ToDecimal(reader["SatirToplam"]).ToString("N2");
                    row.Cells.Add(cellLineTotal);

                    deliveryNoteDetailsBody.Rows.Add(row);
                }
            }
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

                    // İrsaliye başlığı
                    Font titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
                    Paragraph title = new Paragraph("İrsaliye", titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    document.Add(title);

                    // İrsaliye bilgileri
                    Font infoFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
                    PdfPTable infoTable = new PdfPTable(2)
                    {
                        WidthPercentage = 100
                    };
                    infoTable.SetWidths(new float[] { 30, 70 });
                    infoTable.AddCell(new PdfPCell(new Phrase("İrsaliye Numarası:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblIrsaliyeNumarasi.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("İrsaliye Tarihi:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblIrsaliyeTarihi.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("Cari Adı:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblCariAdi.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase("Açıklama:", infoFont)) { Border = Rectangle.NO_BORDER });
                    infoTable.AddCell(new PdfPCell(new Phrase(lblAciklama.Text, infoFont)) { Border = Rectangle.NO_BORDER });
                    document.Add(infoTable);

                    // İrsaliye detayları tablosu
                    PdfPTable table = new PdfPTable(4)
                    {
                        WidthPercentage = 100
                    };
                    table.SetWidths(new float[] { 40, 20, 20, 20 });

                    // Tablo başlıkları
                    table.AddCell(new PdfPCell(new Phrase("Ürün Adı", infoFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Miktar", infoFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Birim Fiyat", infoFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Satır Toplamı", infoFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                    // Tablo satırları
                    foreach (TableRow row in deliveryNoteDetailsBody.Rows)
                    {
                        foreach (TableCell cell in row.Cells)
                        {
                            table.AddCell(new PdfPCell(new Phrase(cell.Text, infoFont)));
                        }
                    }

                    document.Add(table);

                    // Toplam
                    document.Add(new Paragraph("Genel Toplam: " + lblGenelToplam.Text, infoFont));

                    document.Close();
                    writer.Close();

                    string fileName = lblIrsaliyeNumarasi.Text + ".pdf";
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