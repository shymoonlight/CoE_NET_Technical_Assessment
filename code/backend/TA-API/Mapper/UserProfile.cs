using AutoMapper;
using TA_API.Models.Data;
using TA_API.Models.DTOs;

namespace TA_API.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Mapping between User entity and UserDto
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreationDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore());

            // Output DTO (without password)
            CreateMap<User, UserResponseDto>();
        }
    }
}