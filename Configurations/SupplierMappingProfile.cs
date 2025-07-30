using AutoMapper;
using WarehouseManage.DTOs.Supplier;
using WarehouseManage.Model;

namespace WarehouseManage.Configurations;

public class SupplierMappingProfile : Profile
{
    public SupplierMappingProfile()
    {
        // Supplier -> SupplierDto
        CreateMap<Supplier, SupplierDto>()
            .ForMember(dest => dest.TotalProducts, opt => opt.MapFrom(src => src.Products.Count))
            .ForMember(dest => dest.TotalReceipts, opt => opt.MapFrom(src => src.GoodsReceipts.Count))
            .ForMember(dest => dest.TotalPurchaseValue, opt => opt.MapFrom(src => src.GoodsReceipts.Sum(gr => gr.TotalAmount ?? 0)));

        // CreateSupplierDto -> Supplier
        CreateMap<CreateSupplierDto, Supplier>()
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceipts, opt => opt.Ignore());

        // UpdateSupplierDto -> Supplier
        CreateMap<UpdateSupplierDto, Supplier>()
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceipts, opt => opt.Ignore());
    }
}
