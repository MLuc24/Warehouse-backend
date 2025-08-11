using AutoMapper;
using WarehouseManage.DTOs.Auth;
using WarehouseManage.Model;

namespace WarehouseManage.Configurations;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserInfoDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty));
            
        CreateMap<CompleteRegistrationRequestDto, User>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsIssues, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceipts, opt => opt.Ignore());
    }
}
