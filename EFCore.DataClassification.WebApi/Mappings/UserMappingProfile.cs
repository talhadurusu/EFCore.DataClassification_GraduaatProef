using AutoMapper;
using EFCore.DataClassification.WebApi.DTOs;
using EFCore.DataClassification.WebApi.Models;

namespace EFCore.DataClassification.WebApi.Mappings;

/// <summary>
/// AutoMapper profile for User entity mappings.
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User -> UserResponseDto
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Address, 
                       opt => opt.MapFrom(src => src.Adress)); 

        // CreateUserDto -> User
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Games, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore())
            .ForMember(dest => dest.Adress, 
                       opt => opt.MapFrom(src => src.Address ?? string.Empty))
            .ForMember(dest => dest.UserName, 
                       opt => opt.MapFrom(src => (src.Name + " " + src.Surname).Trim()))
            .ForMember(dest => dest.Email, 
                       opt => opt.MapFrom(src => src.Email ?? string.Empty))
            .ForMember(dest => dest.PhoneNumber, 
                       opt => opt.MapFrom(src => src.PhoneNumber ?? string.Empty))
            .ForMember(dest => dest.AdminId, 
                       opt => opt.MapFrom(src => src.AdminId))
            .ForMember(dest => dest.AccountStatus, opt => opt.Ignore())
            .ForMember(dest => dest.LastPasswordChangeUtc, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        // UpdateUserDto -> User (patch mapping)
        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Games, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore())
            .ForMember(dest => dest.Adress, 
                       opt => opt.MapFrom(src => src.Address ?? string.Empty))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
