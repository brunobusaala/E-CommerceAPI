using AutoMapper;
using CrudeApi.Models.DomainModels;

namespace CrudeApi.DTO.ConfigurationDTO
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UsersDto, UsersModel>();
        }
    }
}
