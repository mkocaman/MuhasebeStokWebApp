using AutoMapper;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels;
using MuhasebeStokWebApp.ViewModels.Sozlesme;
using MuhasebeStokWebApp.ViewModels.Fatura;
using MuhasebeStokWebApp.ViewModels.Birim;
using MuhasebeStokWebApp.ViewModels.Depo;
using MuhasebeStokWebApp.ViewModels.UrunKategori;

namespace MuhasebeStokWebApp
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Sozlesme entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Sozlesme, SozlesmeViewModel>()
                .ForMember(dest => dest.SozlesmeID, opt => opt.MapFrom(src => src.SozlesmeID.ToString()))
                .ForMember(dest => dest.CariID, opt => opt.MapFrom(src => src.CariID.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.SozlesmeID, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.SozlesmeID) ? Guid.Empty : Guid.Parse(src.SozlesmeID)))
                .ForMember(dest => dest.CariID, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.CariID) ? Guid.Empty : Guid.Parse(src.CariID)));

            CreateMap<Sozlesme, SozlesmeListViewModel>()
                .ForMember(dest => dest.SozlesmeID, opt => opt.MapFrom(src => src.SozlesmeID.ToString()))
                .ForMember(dest => dest.CariAdi, opt => opt.MapFrom(src => src.Cari.CariUnvani));

            // Fatura entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Fatura, FaturaViewModel>()
                .ForMember(dest => dest.FaturaID, opt => opt.MapFrom(src => src.FaturaID.ToString()))
                .ForMember(dest => dest.CariID, opt => opt.MapFrom(src => src.CariID))
                .ForMember(dest => dest.FaturaNumarasi, opt => opt.MapFrom(src => src.FaturaNumarasi))
                .ReverseMap()
                .ForMember(dest => dest.FaturaID, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.FaturaID) ? Guid.Empty : Guid.Parse(src.FaturaID)));

            // Birim entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Birim, BirimViewModel>();

            CreateMap<Birim, BirimEditViewModel>()
                .ForMember(dest => dest.BirimID, opt => opt.MapFrom(src => src.BirimID))
                .ForMember(dest => dest.BirimAdi, opt => opt.MapFrom(src => src.BirimAdi))
                .ForMember(dest => dest.Aciklama, opt => opt.MapFrom(src => src.Aciklama))
                .ForMember(dest => dest.Aktif, opt => opt.MapFrom(src => src.Aktif));

            CreateMap<BirimCreateViewModel, Birim>()
                .ForMember(dest => dest.BirimID, opt => opt.Ignore())
                .ForMember(dest => dest.OlusturmaTarihi, opt => opt.Ignore())
                .ForMember(dest => dest.GuncellemeTarihi, opt => opt.Ignore())
                .ForMember(dest => dest.OlusturanKullaniciID, opt => opt.Ignore())
                .ForMember(dest => dest.SonGuncelleyenKullaniciID, opt => opt.Ignore())
                .ForMember(dest => dest.Silindi, opt => opt.Ignore())
                .ForMember(dest => dest.SirketID, opt => opt.Ignore())
                .ForMember(dest => dest.Urunler, opt => opt.Ignore())
                .ForMember(dest => dest.FaturaDetaylari, opt => opt.Ignore())
                .ForMember(dest => dest.IrsaliyeDetaylari, opt => opt.Ignore());

            CreateMap<BirimEditViewModel, Birim>()
                .ForMember(dest => dest.OlusturmaTarihi, opt => opt.Ignore())
                .ForMember(dest => dest.GuncellemeTarihi, opt => opt.Ignore())
                .ForMember(dest => dest.OlusturanKullaniciID, opt => opt.Ignore())
                .ForMember(dest => dest.SonGuncelleyenKullaniciID, opt => opt.Ignore())
                .ForMember(dest => dest.Silindi, opt => opt.Ignore())
                .ForMember(dest => dest.SirketID, opt => opt.Ignore())
                .ForMember(dest => dest.Urunler, opt => opt.Ignore())
                .ForMember(dest => dest.FaturaDetaylari, opt => opt.Ignore())
                .ForMember(dest => dest.IrsaliyeDetaylari, opt => opt.Ignore());

            // Depo entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Depo, DepoViewModel>();
            CreateMap<Depo, DepoEditViewModel>();
            CreateMap<DepoCreateViewModel, Depo>();
            CreateMap<DepoEditViewModel, Depo>();

            // UrunKategori entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<UrunKategori, UrunKategoriViewModel>();
            CreateMap<UrunKategori, UrunKategoriEditViewModel>();
            CreateMap<UrunKategoriCreateViewModel, UrunKategori>();
            CreateMap<UrunKategoriEditViewModel, UrunKategori>();

            // Diğer dönüşümler buraya eklenebilir...
        }
    }
} 