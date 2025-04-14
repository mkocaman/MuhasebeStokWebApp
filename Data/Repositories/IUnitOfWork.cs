using System;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Fatura> FaturaRepository { get; }
        IRepository<FaturaDetay> FaturaDetayRepository { get; }
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
        
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task SaveChangesAsync();
        Task SaveAsync();
        Task CompleteAsync();
    }
} 