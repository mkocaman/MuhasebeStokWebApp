using AutoMapper;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels;
using MuhasebeStokWebApp.ViewModels.Sozlesme;
using MuhasebeStokWebApp.ViewModels.Fatura;

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

            // Diğer dönüşümler buraya eklenebilir...
        }
    }
} 