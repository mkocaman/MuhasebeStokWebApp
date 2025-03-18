using System;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        
        IRepository<Urun> UrunRepository { get; }
        IRepository<Cari> CariRepository { get; }
        IRepository<Fatura> FaturaRepository { get; }
        IRepository<FaturaDetay> FaturaDetayRepository { get; }
        IRepository<StokHareket> StokHareketRepository { get; }
        IRepository<CariHareket> CariHareketRepository { get; }
        IRepository<Irsaliye> IrsaliyeRepository { get; }
        IRepository<IrsaliyeDetay> IrsaliyeDetayRepository { get; }
        IRepository<Birim> BirimRepository { get; }
        IRepository<Depo> DepoRepository { get; }
        IRepository<FaturaTuru> FaturaTuruRepository { get; }
        IRepository<IrsaliyeTuru> IrsaliyeTuruRepository { get; }
        IRepository<OdemeTuru> OdemeTuruRepository { get; }
        
        Task SaveAsync();
    }
} 