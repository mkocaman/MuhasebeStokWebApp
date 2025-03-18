using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private Dictionary<Type, object> _repositories;
        private bool disposed = false;

        // Özel repository'ler
        private IRepository<Urun>? _urunRepository;
        private IRepository<Cari>? _cariRepository;
        private IRepository<Fatura>? _faturaRepository;
        private IRepository<FaturaDetay>? _faturaDetayRepository;
        private IRepository<StokHareket>? _stokHareketRepository;
        private IRepository<CariHareket>? _cariHareketRepository;
        private IRepository<Irsaliye>? _irsaliyeRepository;
        private IRepository<IrsaliyeDetay>? _irsaliyeDetayRepository;
        private IRepository<Birim>? _birimRepository;
        private IRepository<Depo>? _depoRepository;
        private IRepository<FaturaTuru>? _faturaTuruRepository;
        private IRepository<IrsaliyeTuru>? _irsaliyeTuruRepository;
        private IRepository<OdemeTuru>? _odemeTuruRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (_repositories.ContainsKey(typeof(TEntity)))
            {
                return (IRepository<TEntity>)_repositories[typeof(TEntity)];
            }

            var repository = new Repository<TEntity>(_context);
            _repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        // Özel repository'ler için property'ler
        public IRepository<Urun> UrunRepository => _urunRepository ??= Repository<Urun>();
        public IRepository<Cari> CariRepository => _cariRepository ??= Repository<Cari>();
        public IRepository<Fatura> FaturaRepository => _faturaRepository ??= Repository<Fatura>();
        public IRepository<FaturaDetay> FaturaDetayRepository => _faturaDetayRepository ??= Repository<FaturaDetay>();
        public IRepository<StokHareket> StokHareketRepository => _stokHareketRepository ??= Repository<StokHareket>();
        public IRepository<CariHareket> CariHareketRepository => _cariHareketRepository ??= Repository<CariHareket>();
        public IRepository<Irsaliye> IrsaliyeRepository => _irsaliyeRepository ??= Repository<Irsaliye>();
        public IRepository<IrsaliyeDetay> IrsaliyeDetayRepository => _irsaliyeDetayRepository ??= Repository<IrsaliyeDetay>();
        public IRepository<Birim> BirimRepository => _birimRepository ??= Repository<Birim>();
        public IRepository<Depo> DepoRepository => _depoRepository ??= Repository<Depo>();
        public IRepository<FaturaTuru> FaturaTuruRepository => _faturaTuruRepository ??= Repository<FaturaTuru>();
        public IRepository<IrsaliyeTuru> IrsaliyeTuruRepository => _irsaliyeTuruRepository ??= Repository<IrsaliyeTuru>();
        public IRepository<OdemeTuru> OdemeTuruRepository => _odemeTuruRepository ??= Repository<OdemeTuru>();

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
} 