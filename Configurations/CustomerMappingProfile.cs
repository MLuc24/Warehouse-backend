using AutoMapper;
using WarehouseManage.DTOs.Customer;
using WarehouseManage.Model;

namespace WarehouseManage.Configurations;

public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        // Customer mappings
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.TotalOrders, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.TotalPurchaseValue, opt => opt.Ignore()); // Will be set manually

        CreateMap<CreateCustomerDto, Customer>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set in repository
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active")) // Default status
            .ForMember(dest => dest.GoodsIssues, opt => opt.Ignore()); // Navigation property

        CreateMap<UpdateCustomerDto, Customer>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Don't update ID
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Don't update creation date
            .ForMember(dest => dest.GoodsIssues, opt => opt.Ignore()); // Navigation property

        // Reverse mappings for flexibility
        CreateMap<CustomerDto, Customer>()
            .ForMember(dest => dest.GoodsIssues, opt => opt.Ignore()); // Navigation property
    }
}
