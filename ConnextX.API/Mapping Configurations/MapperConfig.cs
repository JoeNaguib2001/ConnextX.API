using AutoMapper;
using ConnextX.API.Data.Models.Users;

namespace ConnextX.API.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<ApiUserDto, ApiUser>().ReverseMap();
        }   
    }
}
