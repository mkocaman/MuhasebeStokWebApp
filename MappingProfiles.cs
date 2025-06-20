using AutoMapper;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels;
using MuhasebeStokWebApp.ViewModels.Sozlesme;
using MuhasebeStokWebApp.ViewModels.Fatura;
using MuhasebeStokWebApp.ViewModels.Birim;
using MuhasebeStokWebApp.ViewModels.Depo;
using MuhasebeStokWebApp.ViewModels.UrunKategori;
using MuhasebeStokWebApp.ViewModels.Cari;
using MuhasebeStokWebApp.ViewModels.Urun;
using System;
using System.Linq;

namespace MuhasebeStokWebApp
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Sozlesme entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Sozlesme, SozlesmeViewModel>()
                .ForMember(dest => dest.CariAdi, opt => opt.MapFrom(src => src.Cari != null ? src.Cari.Ad : string.Empty));

            CreateMap<SozlesmeViewModel, Sozlesme>();
            
            // Fatura entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Fatura, ViewModels.Fatura.FaturaViewModel>()
                .ForMember(dest => dest.CariAdi, opt => opt.MapFrom(src => src.Cari != null ? src.Cari.Ad : string.Empty))
                .ForMember(dest => dest.FaturaTuruAdi, opt => opt.MapFrom(src => src.FaturaTuru != null ? src.FaturaTuru.FaturaTuruAdi : string.Empty));

            CreateMap<ViewModels.Fatura.FaturaViewModel, Fatura>();
            CreateMap<FaturaCreateViewModel, Fatura>();
            CreateMap<FaturaEditViewModel, Fatura>();
            
            // FaturaDetay entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<FaturaKalemViewModel, FaturaDetay>();
            CreateMap<FaturaDetay, FaturaKalemViewModel>()
                .ForMember(dest => dest.UrunAdi, opt => opt.MapFrom(src => src.Urun != null ? src.Urun.UrunAdi : string.Empty))
                .ForMember(dest => dest.Birim, opt => opt.MapFrom(src => src.Urun != null && src.Urun.Birim != null ? src.Urun.Birim.BirimAdi : string.Empty));
            
            // Birim entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Birim, BirimViewModel>();
            CreateMap<BirimViewModel, Birim>();
            CreateMap<BirimCreateViewModel, Birim>();
            CreateMap<BirimEditViewModel, Birim>();
            CreateMap<Birim, BirimEditViewModel>();

            // Depo entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Depo, DepoViewModel>();
            CreateMap<DepoViewModel, Depo>();
            CreateMap<DepoCreateViewModel, Depo>();
            CreateMap<DepoEditViewModel, Depo>();
            CreateMap<Depo, DepoEditViewModel>();
            
            // UrunKategori entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<UrunKategori, ViewModels.UrunKategori.UrunKategoriViewModel>();
            CreateMap<ViewModels.UrunKategori.UrunKategoriViewModel, UrunKategori>();
            CreateMap<ViewModels.UrunKategori.UrunKategoriCreateViewModel, UrunKategori>();
            CreateMap<ViewModels.UrunKategori.UrunKategoriEditViewModel, UrunKategori>();
            CreateMap<UrunKategori, ViewModels.UrunKategori.UrunKategoriEditViewModel>();
            
            // Cari entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Cari, CariViewModel>();
            CreateMap<CariViewModel, Cari>();
            CreateMap<CariCreateViewModel, Cari>();
            CreateMap<CariEditViewModel, Cari>();
            
            // Transfer entity ve viewmodel'leri arasındaki eşleştirmeler (eğer eklenmiş ise)
            // CreateMap<StokTransfer, StokTransferViewModel>();
            // CreateMap<StokTransferViewModel, StokTransfer>();
            
            // Urun entity ve viewmodel'leri arasındaki eşleştirmeler
            CreateMap<Urun, UrunViewModel>()
                .ForMember(dest => dest.KategoriAdi, opt => opt.MapFrom(src => src.Kategori != null ? src.Kategori.KategoriAdi : string.Empty))
                .ForMember(dest => dest.BirimAdi, opt => opt.MapFrom(src => src.Birim != null ? src.Birim.BirimAdi : string.Empty));

            CreateMap<UrunViewModel, Urun>();
            CreateMap<UrunCreateViewModel, Urun>();
            CreateMap<UrunEditViewModel, Urun>();
            CreateMap<Urun, UrunEditViewModel>();
        }
    }
} 
