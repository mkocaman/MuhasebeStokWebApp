using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Data.Repositories.EntityRepositories;

namespace MuhasebeStokWebApp.Data.EfCore
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        
        // Generic repository instances
        private IRepository<Fatura>? _faturaRepository;
        private IRepository<FaturaDetay>? _faturaDetayRepository;
        private IRepository<Cari>? _cariRepository;
        private IRepository<CariHareket>? _cariHareketRepository;
        private IRepository<Urun>? _urunRepository;
        private IRepository<UrunBirim>? _urunBirimRepository;
        private IRepository<UrunKategori>? _urunKategoriRepository;
        private IRepository<UrunFiyat>? _urunFiyatRepository;
        private IRepository<StokFifo>? _stokFifoRepository;
        private IRepository<Kasa>? _kasaRepository;
        private IRepository<KasaHareket>? _kasaHareketRepository;
        private IRepository<Irsaliye>? _irsaliyeRepository;
        private IRepository<IrsaliyeDetay>? _irsaliyeDetayRepository;
        private IRepository<Menu>? _menuRepository;
        private IRepository<Sozlesme>? _sozlesmeRepository;
        private IRepository<SistemAyarlari>? _sistemAyarlariRepository;
        private IRepository<StokHareket>? _stokHareketRepository;
        
        // Entity-specific repository instances
        private IUrunRepository? _entityUrunRepository;
        private IFaturaRepository? _entityFaturaRepository;
        private ICariRepository? _entityCariRepository;
        private IIrsaliyeRepository? _entityIrsaliyeRepository;
        
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // Generic Repository Properties
        public IRepository<Fatura> FaturaRepository => _faturaRepository ??= new Repository<Fatura>(_context);
        public IRepository<FaturaDetay> FaturaDetayRepository => _faturaDetayRepository ??= new Repository<FaturaDetay>(_context);
        public IRepository<Cari> CariRepository => _cariRepository ??= new Repository<Cari>(_context);
        public IRepository<CariHareket> CariHareketRepository => _cariHareketRepository ??= new Repository<CariHareket>(_context);
        public IRepository<Urun> UrunRepository => _urunRepository ??= new Repository<Urun>(_context);
        public IRepository<UrunBirim> UrunBirimRepository => _urunBirimRepository ??= new Repository<UrunBirim>(_context);
        public IRepository<UrunKategori> UrunKategoriRepository => _urunKategoriRepository ??= new Repository<UrunKategori>(_context);
        public IRepository<UrunFiyat> UrunFiyatRepository => _urunFiyatRepository ??= new Repository<UrunFiyat>(_context);
        public IRepository<StokFifo> StokFifoRepository => _stokFifoRepository ??= new Repository<StokFifo>(_context);
        public IRepository<Kasa> KasaRepository => _kasaRepository ??= new Repository<Kasa>(_context);
        public IRepository<KasaHareket> KasaHareketRepository => _kasaHareketRepository ??= new Repository<KasaHareket>(_context);
        public IRepository<Irsaliye> IrsaliyeRepository => _irsaliyeRepository ??= new Repository<Irsaliye>(_context);
        public IRepository<IrsaliyeDetay> IrsaliyeDetayRepository => _irsaliyeDetayRepository ??= new Repository<IrsaliyeDetay>(_context);
        public IRepository<Menu> MenuRepository => _menuRepository ??= new Repository<Menu>(_context);
        public IRepository<Sozlesme> SozlesmeRepository => _sozlesmeRepository ??= new Repository<Sozlesme>(_context);
        public IRepository<SistemAyarlari> SistemAyarlariRepository => _sistemAyarlariRepository ??= new Repository<SistemAyarlari>(_context);
        public IRepository<StokHareket> StokHareketRepository => _stokHareketRepository ??= new Repository<StokHareket>(_context);
        
        // Entity-specific Repository Properties
        public IUrunRepository EntityUrunRepository => _entityUrunRepository ??= new UrunRepository(_context);
        public IFaturaRepository EntityFaturaRepository => _entityFaturaRepository ??= new FaturaRepository(_context);
        public ICariRepository EntityCariRepository => _entityCariRepository ??= new CariRepository(_context);
        public IIrsaliyeRepository EntityIrsaliyeRepository => _entityIrsaliyeRepository ??= new IrsaliyeRepository(_context);
        
        // Generic repository factory method
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            return new Repository<TEntity>(_context);
        }
        
        // Transaction methods
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                return;
            }
            
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }
        
        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }
        
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        
        // SozlesmeService için SaveAsync metodu ekleniyor
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
        
        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            _context.Dispose();
            _transaction?.Dispose();
        }
    }
} 