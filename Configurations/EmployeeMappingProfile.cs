using AutoMapper;
using WarehouseManage.DTOs.Employee;
using WarehouseManage.Model;

namespace WarehouseManage.Configurations;

public class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        // User (Employee) to EmployeeDto
        CreateMap<User, EmployeeDto>()
            .ForMember(dest => dest.TotalGoodsIssuesCreated, 
                opt => opt.MapFrom(src => src.GoodsIssues.Count))
            .ForMember(dest => dest.TotalGoodsReceiptsCreated, 
                opt => opt.MapFrom(src => src.GoodsReceipts.Count))
            .ForMember(dest => dest.TotalApprovals, opt => opt.Ignore()) // Will be calculated separately
            .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore()); // Will be added later if needed

        // CreateEmployeeDto to User
        CreateMap<CreateEmployeeDto, User>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Will be set separately
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Will be set separately
            .ForMember(dest => dest.GoodsIssues, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceipts, opt => opt.Ignore())
            .ForMember(dest => dest.Image, opt => opt.MapFrom(src => (string?)null));

        // UpdateEmployeeDto to User
        CreateMap<UpdateEmployeeDto, User>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Username, opt => opt.Ignore()) // Username cannot be changed
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsIssues, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceipts, opt => opt.Ignore())
            .ForMember(dest => dest.Image, opt => opt.Ignore());
    }
}
