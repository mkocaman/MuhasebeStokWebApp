using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Filter;

namespace MuhasebeStokWebApp.Services.Filters
{
    /// <summary>
    /// Ürün filtreleme servisi
    /// </summary>
    public class UrunFilterService : FilterServiceBase<Urun, UrunFilterModel>
    {
        public UrunFilterService(
            ApplicationDbContext context,
            ILogger<UrunFilterService> logger) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// Ürün filtreleme kriterlerine göre sorgu ifadesi oluşturur
        /// </summary>
        public override Expression<Func<Urun, bool>> BuildFilterExpression(UrunFilterModel filter)
        {
            // Başlangıç filtresini true olarak ayarla (tüm kayıtları getir)
            Expression<Func<Urun, bool>> expression = u => true;
            
            // Aktif tab filtresi
            if (filter.AktifTab == "aktif")
            {
                expression = u => u.Aktif && !u.Silindi;
            }
            else if (filter.AktifTab == "pasif")
            {
                expression = u => !u.Aktif && !u.Silindi;
            }
            else if (filter.AktifTab == "silindi")
            {
                expression = u => u.Silindi;
            }
            else 
            {
                // Silindi durumuna göre filtrele
                if (!filter.SilinenleriGoster)
                {
                    expression = u => !u.Silindi;
                }
                
                // Aktiflik durumuna göre filtrele
                if (filter.Aktif.HasValue)
                {
                    expression = ExpressionHelper.AndAlso(expression, u => u.Aktif == filter.Aktif.Value);
                }
            }
            
            // Ürün kodu filtresi
            if (!string.IsNullOrWhiteSpace(filter.UrunKodu))
            {
                expression = ExpressionHelper.AndAlso(expression, 
                    u => u.UrunKodu.Contains(filter.UrunKodu));
            }
            
            // Ürün adı filtresi
            if (!string.IsNullOrWhiteSpace(filter.UrunAdi))
            {
                expression = ExpressionHelper.AndAlso(expression, 
                    u => u.UrunAdi.Contains(filter.UrunAdi));
            }
            
            // Kategori filtresi
            if (filter.KategoriID.HasValue && filter.KategoriID != Guid.Empty)
            {
                expression = ExpressionHelper.AndAlso(expression, 
                    u => u.KategoriID == filter.KategoriID);
            }
            
            // Stok miktarı filtreleri
            if (filter.SadeceStokta)
            {
                expression = ExpressionHelper.AndAlso(expression, 
                    u => u.StokMiktar > 0);
            }
            
            if (filter.MinStok.HasValue)
            {
                expression = ExpressionHelper.AndAlso(expression, 
                    u => u.StokMiktar >= filter.MinStok.Value);
            }
            
            if (filter.MaxStok.HasValue)
            {
                expression = ExpressionHelper.AndAlso(expression, 
                    u => u.StokMiktar <= filter.MaxStok.Value);
            }
            
            return expression;
        }
        
        /// <summary>
        /// Filtre kriterlerine göre sıralanmış sonuçları getirir
        /// </summary>
        public async Task<(List<Urun> Results, int TotalCount)> GetSortedFilteredResultsAsync(
            UrunFilterModel filter, int page = 1, int pageSize = 10)
        {
            try
            {
                // Filtre ifadesini oluştur
                var filterExpression = BuildFilterExpression(filter);
                
                // Toplam kayıt sayısını al
                var totalCount = await _context.Urunler
                    .IgnoreQueryFilters() // Silindi filtresini ignore et, kendi filtremizi kullanacağız
                    .Where(filterExpression)
                    .CountAsync();
                
                // Temel sorgu - önce ilgili verileri include et
                var query = _context.Urunler
                    .IgnoreQueryFilters() // Silindi filtresini ignore et, kendi filtremizi kullanacağız
                    .Where(filterExpression)
                    .Include(u => u.Birim)
                    .Include(u => u.Kategori)
                    .AsQueryable(); // Açık bir şekilde IQueryable olarak belirt
                
                // Sıralama - ayrı bir IQueryable olarak işle
                IQueryable<Urun> sortedQuery;
                if (!string.IsNullOrEmpty(filter.SortOrder))
                {
                    switch (filter.SortOrder)
                    {
                        case "name_asc":
                            sortedQuery = query.OrderBy(u => u.UrunAdi);
                            break;
                        case "name_desc":
                            sortedQuery = query.OrderByDescending(u => u.UrunAdi);
                            break;
                        case "code_asc":
                            sortedQuery = query.OrderBy(u => u.UrunKodu);
                            break;
                        case "code_desc":
                            sortedQuery = query.OrderByDescending(u => u.UrunKodu);
                            break;
                        case "stock_asc":
                            sortedQuery = query.OrderBy(u => u.StokMiktar);
                            break;
                        case "stock_desc":
                            sortedQuery = query.OrderByDescending(u => u.StokMiktar);
                            break;
                        case "date_asc":
                            sortedQuery = query.OrderBy(u => u.OlusturmaTarihi);
                            break;
                        case "date_desc":
                            sortedQuery = query.OrderByDescending(u => u.OlusturmaTarihi);
                            break;
                        default:
                            sortedQuery = query.OrderBy(u => u.UrunAdi);
                            break;
                    }
                }
                else
                {
                    sortedQuery = query.OrderBy(u => u.UrunAdi);
                }
                
                // Sayfalama
                var results = await sortedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                return (results, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSortedFilteredResultsAsync metodu hatası: {Message}", ex.Message);
                return (new List<Urun>(), 0);
            }
        }
    }
    
    /// <summary>
    /// Expression'ları birleştirmeye yardımcı statik sınıf
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// İki expression'ı AND operatörü ile birleştirir
        /// </summary>
        public static Expression<Func<T, bool>> AndAlso<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            // expr2'nin gövdesini expr1'in parametreleriyle değiştir
            var parameter = expr1.Parameters[0];
            var visitor = new ParameterReplacer(parameter);
            var body2 = visitor.Replace(expr2.Body);
            
            // İki gövdeyi AndAlso ile birleştir
            var combined = Expression.AndAlso(expr1.Body, body2);
            
            // Yeni lambda expression oluştur
            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }
        
        /// <summary>
        /// Expression'daki parametreleri değiştirmek için ziyaretçi sınıf
        /// </summary>
        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;
            
            public ParameterReplacer(ParameterExpression parameter)
            {
                _parameter = parameter;
            }
            
            public Expression Replace(Expression node)
            {
                return Visit(node);
            }
            
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _parameter;
            }
        }
    }
} 