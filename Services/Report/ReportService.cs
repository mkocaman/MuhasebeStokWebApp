using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Models.Report;
using Microsoft.AspNetCore.Hosting;
using DEntityFatura = MuhasebeStokWebApp.Data.Entities.Fatura;
using DEntityIrsaliye = MuhasebeStokWebApp.Data.Entities.Irsaliye;
using DEntityUrun = MuhasebeStokWebApp.Data.Entities.Urun;
using DEntityCari = MuhasebeStokWebApp.Data.Entities.Cari;

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
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                var fatura = await _context.Faturalar
                    .Include(f => f.Cari)
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(d => d.Urun)
                    .FirstOrDefaultAsync(f => f.FaturaID == faturaId);

                if (fatura == null)
                    throw new Exception("Fatura bulunamadı.");

                // Başlık
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("FATURA", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph("\n"));

                // Fatura Bilgileri
                var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var infoTable = new PdfPTable(4);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 1f, 2f, 1f, 2f });

                infoTable.AddCell(new PdfPCell(new Phrase("Fatura No:", infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase(fatura.FaturaNumarasi, infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase("Tarih:", infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase(fatura.FaturaTarihi?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? "", infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase("Cari:", infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase(fatura.Cari?.Ad ?? "", infoFont)) { Border = 0 });

                document.Add(infoTable);
                document.Add(new Paragraph("\n"));

                // Detay Tablosu
                var table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f });

                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                table.AddCell(new PdfPCell(new Phrase("No", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Ürün", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Miktar", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Birim Fiyat", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Toplam", headerFont)));

                var detailFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                int rowNo = 1;
                foreach (var detay in fatura.FaturaDetaylari)
                {
                    table.AddCell(new PdfPCell(new Phrase(rowNo.ToString(), detailFont)));
                    table.AddCell(new PdfPCell(new Phrase(detay.Urun?.UrunAdi ?? "", detailFont)));
                    table.AddCell(new PdfPCell(new Phrase(detay.Miktar.ToString("N2"), detailFont)));
                    table.AddCell(new PdfPCell(new Phrase(detay.BirimFiyat.ToString("N2"), detailFont)));
                    table.AddCell(new PdfPCell(new Phrase(detay.SatirToplam?.ToString("N2") ?? "0.00", detailFont)));
                    rowNo++;
                }

                document.Add(table);

                // Toplam Tablosu
                var totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 100;
                totalTable.SetWidths(new float[] { 3f, 1f });

                var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                AddTotalToTable(totalTable, "Ara Toplam:", fatura.AraToplam ?? 0m, totalFont);
                AddTotalToTable(totalTable, "KDV:", fatura.KDVToplam ?? 0m, totalFont);
                AddTotalToTable(totalTable, "Genel Toplam:", fatura.GenelToplam ?? 0m, totalFont);

                document.Add(new Paragraph("\n"));
                document.Add(totalTable);

                document.Close();
                return ms.ToArray();
            }
        }

        public async Task<byte[]> GenerateIrsaliyeReportAsync(Guid irsaliyeId)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                var irsaliye = await _context.Irsaliyeler
                    .Include(i => i.Cari)
                    .Include(i => i.IrsaliyeDetaylari)
                        .ThenInclude(d => d.Urun)
                    .FirstOrDefaultAsync(i => i.IrsaliyeID == irsaliyeId);

                if (irsaliye == null)
                    throw new Exception("İrsaliye bulunamadı.");

                // Başlık
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("İRSALİYE", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph("\n"));

                // İrsaliye Bilgileri
                var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var infoTable = new PdfPTable(4);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 1f, 2f, 1f, 2f });

                infoTable.AddCell(new PdfPCell(new Phrase("İrsaliye No:", infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase(irsaliye.IrsaliyeNumarasi, infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase("Tarih:", infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase(irsaliye.IrsaliyeTarihi.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase("Cari:", infoFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase(irsaliye.Cari?.Ad ?? "", infoFont)) { Border = 0 });

                document.Add(infoTable);
                document.Add(new Paragraph("\n"));

                // İrsaliye Başlık
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 1f, 1f });

                // Sol bölüm - Şirket bilgisi
                PdfPCell companyInfoCell = new PdfPCell();
                companyInfoCell.Border = 0;
                
                Paragraph companyName = new Paragraph("Şirket Adı: Muhasebecilik A.Ş.", headerFont);
                Paragraph companyAddress = new Paragraph("Adres: İstanbul / Türkiye", normalFont);
                Paragraph companyPhone = new Paragraph("Telefon: +90 (212) 123 45 67", normalFont);
                
                companyInfoCell.AddElement(companyName);
                companyInfoCell.AddElement(companyAddress);
                companyInfoCell.AddElement(companyPhone);
                
                headerTable.AddCell(companyInfoCell);
                headerTable.AddCell(new PdfPCell() { Border = 0 }); // Sağ taraf boş

                document.Add(headerTable);

                // Detay Tablosu
                var table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1f });

                table.AddCell(new PdfPCell(new Phrase("No", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Ürün", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Miktar", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Birim", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Açıklama", headerFont)));

                var detailFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                int rowNo = 1;
                foreach (var detay in irsaliye.IrsaliyeDetaylari)
                {
                    table.AddCell(new PdfPCell(new Phrase(rowNo.ToString(), detailFont)));
                    table.AddCell(new PdfPCell(new Phrase(detay.Urun?.UrunAdi ?? "", detailFont)));
                    table.AddCell(new PdfPCell(new Phrase(detay.Miktar.ToString("N2"), detailFont)));
                    table.AddCell(new PdfPCell(new Phrase(detay.Birim ?? "", detailFont)));
                    table.AddCell(new PdfPCell(new Phrase(detay.Aciklama ?? "", detailFont)));
                    rowNo++;
                }

                document.Add(table);

                document.Close();
                return ms.ToArray();
            }
        }

        private void AddTotalToTable(PdfPTable table, string label, decimal amount, Font font)
        {
            table.AddCell(new PdfPCell(new Phrase(label, font)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(amount.ToString("N2"), font)) { Border = 0 });
        }
    }
} 