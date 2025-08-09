using AutoMapper;
using WarehouseManage.DTOs.GoodsIssue;
using WarehouseManage.Model;

namespace WarehouseManage.Configurations;

public class GoodsIssueMappingProfile : Profile
{
    public GoodsIssueMappingProfile()
    {
        // GoodsIssue mappings
        CreateMap<GoodsIssue, GoodsIssueDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustomerName : null))
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.FullName))
            .ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null))
            .ForMember(dest => dest.PreparedByUserName, opt => opt.MapFrom(src => src.PreparedByUser != null ? src.PreparedByUser.FullName : null))
            .ForMember(dest => dest.DeliveredByUserName, opt => opt.MapFrom(src => src.DeliveredByUser != null ? src.DeliveredByUser.FullName : null))
            .ForMember(dest => dest.CompletedByUserName, opt => opt.MapFrom(src => src.CompletedByUser != null ? src.CompletedByUser.FullName : null))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.GoodsIssueDetails));

        CreateMap<CreateGoodsIssueDto, GoodsIssue>()
            .ForMember(dest => dest.GoodsIssueDetails, opt => opt.MapFrom(src => src.Details));

        CreateMap<UpdateGoodsIssueDto, GoodsIssue>()
            .ForMember(dest => dest.GoodsIssueDetails, opt => opt.MapFrom(src => src.Details));

        // GoodsIssueDetail mappings
        CreateMap<GoodsIssueDetail, GoodsIssueDetailDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
            .ForMember(dest => dest.ProductSku, opt => opt.MapFrom(src => src.Product.Sku))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Product.Unit))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));

        CreateMap<CreateGoodsIssueDetailDto, GoodsIssueDetail>()
            .ForMember(dest => dest.Subtotal, opt => opt.Ignore()); // Computed column
    }
}
