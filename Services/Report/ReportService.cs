using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Models.Report;
using Microsoft.AspNetCore.Hosting;
using DEntityFatura = MuhasebeStokWebApp.Data.Entities.Fatura;
using DEntityIrsaliye = MuhasebeStokWebApp.Data.Entities.Irsaliye;
using DEntityUrun = MuhasebeStokWebApp.Data.Entities.Urun;
using DEntityCari = MuhasebeStokWebApp.Data.Entities.Cari;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Geom;
using Path = System.IO.Path;

namespace MuhasebeStokWebApp.Services.Report
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;
        private readonly IMemoryCache _cache;
        private readonly string _uploadPath;
        private readonly string _templatePath;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRepository<DEntityFatura> _faturaRepository;
        private readonly IRepository<DEntityIrsaliye> _irsaliyeRepository;

        public ReportService(
            ApplicationDbContext context,
            ILogger<ReportService> logger,
            IMemoryCache cache,
            IWebHostEnvironment env,
            IWebHostEnvironment webHostEnvironment,
            IRepository<DEntityFatura> faturaRepository,
            IRepository<DEntityIrsaliye> irsaliyeRepository)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _webHostEnvironment = webHostEnvironment;
            _faturaRepository = faturaRepository;
            _irsaliyeRepository = irsaliyeRepository;
            _uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            _templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "templates");
        }

        public async Task<byte[]> GenerateFaturaReportAsync(Guid faturaId)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // PDF belgesini oluştur
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Fontları tanımla
                PdfFont headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                var fatura = await _context.Faturalar
                    .Include(f => f.Cari)
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(d => d.Urun)
                    .FirstOrDefaultAsync(f => f.FaturaID == faturaId);

                if (fatura == null)
                    throw new Exception("Fatura bulunamadı.");

                // Başlık
                Paragraph title = new Paragraph("FATURA")
                    .SetFont(headerFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(title);
                document.Add(new Paragraph("\n"));

                // Fatura Bilgileri
                Table infoTable = new Table(4);
                infoTable.SetWidth(UnitValue.CreatePercentValue(100));
                float[] columnWidths = { 1, 2, 1, 2 };
                infoTable.SetWidth(UnitValue.CreatePercentValue(100));

                Cell cell1 = new Cell().Add(new Paragraph("Fatura No:").SetFont(normalFont));
                cell1.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell1);

                Cell cell2 = new Cell().Add(new Paragraph(fatura.FaturaNumarasi).SetFont(normalFont));
                cell2.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell2);

                Cell cell3 = new Cell().Add(new Paragraph("Tarih:").SetFont(normalFont));
                cell3.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell3);

                Cell cell4 = new Cell().Add(new Paragraph(fatura.FaturaTarihi?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? "").SetFont(normalFont));
                cell4.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell4);

                Cell cell5 = new Cell().Add(new Paragraph("Cari:").SetFont(normalFont));
                cell5.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell5);

                Cell cell6 = new Cell().Add(new Paragraph(fatura.Cari?.Ad ?? "").SetFont(normalFont));
                cell6.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell6);

                document.Add(infoTable);
                document.Add(new Paragraph("\n"));

                // Detay Tablosu
                Table table = new Table(5);
                table.SetWidth(UnitValue.CreatePercentValue(100));
                float[] detailColumnWidths = { 0.5f, 2, 1, 1, 1 };

                table.AddHeaderCell(new Cell().Add(new Paragraph("No").SetFont(headerFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Ürün").SetFont(headerFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Miktar").SetFont(headerFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Birim Fiyat").SetFont(headerFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Toplam").SetFont(headerFont)));

                int rowNo = 1;
                foreach (var detay in fatura.FaturaDetaylari)
                {
                    table.AddCell(new Cell().Add(new Paragraph(rowNo.ToString()).SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(detay.Urun?.UrunAdi ?? "").SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(detay.Miktar.ToString("N2")).SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(detay.BirimFiyat.ToString("N2")).SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(detay.SatirToplam?.ToString("N2") ?? "0.00").SetFont(normalFont)));
                    rowNo++;
                }

                document.Add(table);

                // Toplam Tablosu
                Table totalTable = new Table(2);
                totalTable.SetWidth(UnitValue.CreatePercentValue(100));

                AddTotalToTable(totalTable, "Ara Toplam:", fatura.AraToplam ?? 0m, headerFont);
                AddTotalToTable(totalTable, "KDV:", fatura.KDVToplam ?? 0m, headerFont);
                AddTotalToTable(totalTable, "Genel Toplam:", fatura.GenelToplam ?? 0m, headerFont);

                document.Add(new Paragraph("\n"));
                document.Add(totalTable);

                document.Close();
                return ms.ToArray();
            }
        }

        public async Task<byte[]> GenerateIrsaliyeReportAsync(Guid irsaliyeId)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // PDF belgesini oluştur
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Fontları tanımla
                PdfFont headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                var irsaliye = await _context.Irsaliyeler
                    .Include(i => i.Cari)
                    .Include(i => i.IrsaliyeDetaylari)
                        .ThenInclude(d => d.Urun)
                    .FirstOrDefaultAsync(i => i.IrsaliyeID == irsaliyeId);

                if (irsaliye == null)
                    throw new Exception("İrsaliye bulunamadı.");

                // Başlık
                Paragraph title = new Paragraph("İRSALİYE")
                    .SetFont(headerFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(title);
                document.Add(new Paragraph("\n"));

                // İrsaliye Bilgileri
                Table infoTable = new Table(4);
                infoTable.SetWidth(UnitValue.CreatePercentValue(100));

                Cell cell1 = new Cell().Add(new Paragraph("İrsaliye No:").SetFont(normalFont));
                cell1.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell1);

                Cell cell2 = new Cell().Add(new Paragraph(irsaliye.IrsaliyeNumarasi).SetFont(normalFont));
                cell2.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell2);

                Cell cell3 = new Cell().Add(new Paragraph("Tarih:").SetFont(normalFont));
                cell3.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell3);

                Cell cell4 = new Cell().Add(new Paragraph(irsaliye.IrsaliyeTarihi.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)).SetFont(normalFont));
                cell4.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell4);

                Cell cell5 = new Cell().Add(new Paragraph("Cari:").SetFont(normalFont));
                cell5.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell5);

                Cell cell6 = new Cell().Add(new Paragraph(irsaliye.Cari?.Ad ?? "").SetFont(normalFont));
                cell6.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                infoTable.AddCell(cell6);

                document.Add(infoTable);
                document.Add(new Paragraph("\n"));

                // İrsaliye Başlık
                Table headerTable = new Table(2);
                headerTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Sol bölüm - Şirket bilgisi
                Cell companyInfoCell = new Cell();
                companyInfoCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                
                companyInfoCell.Add(new Paragraph("Şirket Adı: Muhasebecilik A.Ş.").SetFont(headerFont));
                companyInfoCell.Add(new Paragraph("Adres: İstanbul / Türkiye").SetFont(normalFont));
                companyInfoCell.Add(new Paragraph("Telefon: +90 (212) 123 45 67").SetFont(normalFont));
                
                headerTable.AddCell(companyInfoCell);
                
                Cell emptyCell = new Cell();
                emptyCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                headerTable.AddCell(emptyCell); // Sağ taraf boş

                document.Add(headerTable);

                // Detay Tablosu
                Table table = new Table(5);
                table.SetWidth(UnitValue.CreatePercentValue(100));

                table.AddHeaderCell(new Cell().Add(new Paragraph("No").SetFont(headerFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Ürün").SetFont(headerFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Miktar").SetFont(headerFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Birim").SetFont(headerFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Açıklama").SetFont(headerFont)));

                int rowNo = 1;
                foreach (var detay in irsaliye.IrsaliyeDetaylari)
                {
                    table.AddCell(new Cell().Add(new Paragraph(rowNo.ToString()).SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(detay.Urun?.UrunAdi ?? "").SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(detay.Miktar.ToString("N2")).SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(detay.Birim ?? "").SetFont(normalFont)));
                    table.AddCell(new Cell().Add(new Paragraph(detay.Aciklama ?? "").SetFont(normalFont)));
                    rowNo++;
                }

                document.Add(table);

                document.Close();
                return ms.ToArray();
            }
        }

        private void AddTotalToTable(Table table, string label, decimal amount, PdfFont font)
        {
            Cell labelCell = new Cell();
            labelCell.Add(new Paragraph(label).SetFont(font));
            labelCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            Cell valueCell = new Cell();
            valueCell.Add(new Paragraph(amount.ToString("N2")).SetFont(font));
            valueCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            table.AddCell(labelCell);
            table.AddCell(valueCell);
        }
        
        public byte[] GenerateExcelReport(DataTable data, string reportTitle)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");
                
                // Başlık
                worksheet.Cell(1, 1).Value = reportTitle;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Range(1, 1, 1, data.Columns.Count).Merge();
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Sütun başlıkları
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    worksheet.Cell(3, i + 1).Value = data.Columns[i].ColumnName;
                    worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(3, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Verileri ekle
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    for (int j = 0; j < data.Columns.Count; j++)
                    {
                        worksheet.Cell(i + 4, j + 1).Value = data.Rows[i][j].ToString();
                    }
                }

                // Sütun genişliklerini otomatik ayarla
                worksheet.Columns().AdjustToContents();
                
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GeneratePdfReport(DataTable data, string reportTitle)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // PDF belgesini oluştur
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);
                
                // Yazı tiplerini tanımla
                PdfFont headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                
                // Rapor başlığı
                Paragraph title = new Paragraph(reportTitle)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(headerFont)
                    .SetFontSize(14);
                document.Add(title);
                
                // Boşluk ekle
                document.Add(new Paragraph("\n"));
                
                // Tablo oluşturma
                Table table = new Table(data.Columns.Count);
                table.SetWidth(UnitValue.CreatePercentValue(100));
                
                // Sütun başlıklarını ekle
                foreach (DataColumn column in data.Columns)
                {
                    Cell headerCell = new Cell()
                        .Add(new Paragraph(column.ColumnName).SetFont(headerFont))
                        .SetBackgroundColor(new DeviceRgb(211, 211, 211))
                        .SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell(headerCell);
                }
                
                // Verileri ekle
                foreach (DataRow row in data.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(item?.ToString() ?? "").SetFont(normalFont)));
                    }
                }
                
                document.Add(table);
                document.Close();
                
                return ms.ToArray();
            }
        }

        // Para birimini formatlı göstermek için yardımcı metot
        public string FormatCurrency(decimal value, string currencySymbol = "₺")
        {
            return $"{value.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))} {currencySymbol}";
        }
    }
} 