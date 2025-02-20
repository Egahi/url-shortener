using AutoMapper;
using url_shortener.Models;
using url_shortener.ModelsDTO;

namespace url_shortener.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Url, UrlRepsonseDTO>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.LongUrl));
            CreateMap<UrlRequestDTO, Url>()
                .ForMember(dest => dest.LongUrl, opt => opt.MapFrom(src => src.Url));
        }
    }
}
