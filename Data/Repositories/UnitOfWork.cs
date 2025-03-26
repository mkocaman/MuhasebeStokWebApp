using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using MuhasebeStokWebApp.Data.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private Dictionary<Type, object> _repositories;
        private bool disposed = false;
        private IDbContextTransaction _transaction;

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
        private IRepository<UrunFiyat>? _urunFiyatRepository;
        private IRepository<FiyatTipi>? _fiyatTipiRepository;
        private IRepository<Menu>? _menuRepository;
        private IRepository<MenuRol>? _menuRolRepository;
        private IIrsaliyeRepository? _customIrsaliyeRepository;
        private IIrsaliyeDetayRepository? _customIrsaliyeDetayRepository;

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
        public IRepository<UrunFiyat> UrunFiyatRepository => _urunFiyatRepository ??= Repository<UrunFiyat>();
        public IRepository<FiyatTipi> FiyatTipiRepository => _fiyatTipiRepository ??= Repository<FiyatTipi>();
        public IRepository<Menu> MenuRepository => _menuRepository ??= Repository<Menu>();
        public IRepository<MenuRol> MenuRolRepository => _menuRolRepository ??= Repository<MenuRol>();
        public IIrsaliyeRepository IrsaliyeCustomRepository => _customIrsaliyeRepository ??= new IrsaliyeRepository(_context);
        public IIrsaliyeDetayRepository IrsaliyeDetayCustomRepository => _customIrsaliyeDetayRepository ??= new IrsaliyeDetayRepository(_context);

        public async Task SaveAsync()
        {
            try 
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Eşzamanlılık hataları için kayıtları yeniden yükleme
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is BaseEntity)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = await entry.GetDatabaseValuesAsync();

                        // Değer yoksa silindi demektir, bu durumu yönet
                        if (databaseValues == null)
                        {
                            // Silinen kaydı güncellemek mümkün değil, işlem iptali
                            throw;
                        }

                        // Değerleri yeniden yükle
                        foreach (var property in proposedValues.Properties)
                        {
                            // GuncellemeTarihi gibi izlenen alanları atla
                            if (property.Name == "GuncellemeTarihi" || property.Name == "OlusturmaTarihi")
                                continue;

                            var proposedValue = proposedValues[property];
                            var databaseValue = databaseValues[property];

                            // Değer değişmişse güncelle
                            if (proposedValue != databaseValue)
                            {
                                proposedValues[property] = databaseValue;
                            }
                        }

                        // Değişiklikler uygulandı, tekrar kaydet
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "Don't know how to handle concurrency conflicts for "
                            + entry.Metadata.Name);
                    }
                }

                // Tekrar kaydetmeyi dene
                await _context.SaveChangesAsync();
            }
        }

        // CompleteAsync metodu ekledik
        public async Task CompleteAsync()
        {
            await SaveAsync();
        }

        // Transaction yönetimi için yeni metotlar
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                return _transaction;
            }

            _transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            return _transaction;
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    await DisposeTransactionAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }
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