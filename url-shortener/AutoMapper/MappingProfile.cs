using AutoMapper;
using url_shortener.Models;
using url_shortener.ModelsDTO;

namespace url_shortener.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Url, LongUrlRepsonseDTO>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.LongUrl));
            CreateMap<Url, ShortUrlResponseDTO>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.ShortUrl));
            CreateMap<Url, URLStatsResponseDTO>();
            CreateMap<UrlRequestDTO, Url>()
                .ForMember(dest => dest.LongUrl, opt => opt.MapFrom(src => src.Url));
        }
    }
}
