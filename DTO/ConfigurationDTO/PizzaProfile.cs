using AutoMapper;
using CrudeApi.Models.DomainModels;

namespace CrudeApi.DTO.ConfigurationDTO
{
    public class PizzaProfile : Profile
    {
        public PizzaProfile()
        {
            CreateMap<Pizza, PizzaDto>()
                .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => $"{src.Id}")
            )
            .ForMember(
                dest => dest.SizeID,
                opt => opt.MapFrom(src => $"{src.SizeID}")
                )
            .ForMember(dest => dest.Name,
            opt => opt.MapFrom(src => $"{src.Name}"))

            .ForMember(dest => dest.Description,
            opt => opt.MapFrom(src => $"{src.Description}"))

            .ForMember(dest => dest.Price,
            opt => opt.MapFrom(src => $"{src.Price}"))

            .ForMember(dest => dest.ImageName,
            opt => opt.MapFrom(src => $"{src.ImageName}"));
        }
    }
}