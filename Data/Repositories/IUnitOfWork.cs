using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories.EntityRepositories;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        // Generic Repository Properties
        IRepository<Fatura> FaturaRepository { get; }
        IRepository<FaturaDetay> FaturaDetayRepository { get; }
        IRepository<FaturaOdeme> FaturaOdemeleriRepository { get; }
        IRepository<FaturaTuru> FaturaTurleriRepository { get; }
        IRepository<Cari> CariRepository { get; }
        IRepository<CariHareket> CariHareketRepository { get; }
        IRepository<Urun> UrunRepository { get; }
        IRepository<UrunBirim> UrunBirimRepository { get; }
        IRepository<UrunKategori> UrunKategoriRepository { get; }
        IRepository<UrunFiyat> UrunFiyatRepository { get; }
        IRepository<StokFifo> StokFifoRepository { get; }
        IRepository<StokHareket> StokHareketRepository { get; }
        IRepository<Kasa> KasaRepository { get; }
        IRepository<KasaHareket> KasaHareketRepository { get; }
        IRepository<Irsaliye> IrsaliyeRepository { get; }
        IRepository<IrsaliyeDetay> IrsaliyeDetayRepository { get; }
        IRepository<Menu> MenuRepository { get; }
        IRepository<Sozlesme> SozlesmeRepository { get; }
        IRepository<SistemAyarlari> SistemAyarlariRepository { get; }
        IRepository<Depo> DepolarRepository { get; }
        
        // Entity-specific Repository Properties
        IUrunRepository EntityUrunRepository { get; }
        IFaturaRepository EntityFaturaRepository { get; }
        ICariRepository EntityCariRepository { get; }
        IIrsaliyeRepository EntityIrsaliyeRepository { get; }
        IIrsaliyeDetayRepository EntityIrsaliyeDetayRepository { get; }
        // Diğer entity-specific repository'ler
        IRepository<StokHareket> EntityStokHareketRepository { get; }
        IRepository<StokFifo> EntityStokFifoRepository { get; }
        IRepository<FaturaDetay> EntityFaturaDetayRepository { get; }
        IRepository<CariHareket> EntityCariHareketRepository { get; }
        IRepository<Depo> EntityDepolarRepository { get; }
        
        // Generic repository factory method
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        
        // Transaction methods
        Task BeginTransactionAsync();
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task SaveChangesAsync();
        Task SaveAsync();
        Task CompleteAsync();
    }
} 