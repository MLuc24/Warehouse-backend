using AutoMapper;
using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Model;

namespace WarehouseManage.Configurations;

public class GoodsReceiptMappingProfile : Profile
{
    public GoodsReceiptMappingProfile()
    {
        // GoodsReceipt mappings
        CreateMap<GoodsReceipt, GoodsReceiptDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.SupplierName : ""))
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName : ""))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.GoodsReceiptDetails));

        CreateMap<CreateGoodsReceiptDto, GoodsReceipt>()
            .ForMember(dest => dest.GoodsReceiptId, opt => opt.Ignore())
            .ForMember(dest => dest.ReceiptNumber, opt => opt.Ignore())
            .ForMember(dest => dest.ReceiptDate, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceiptDetails, opt => opt.MapFrom(src => src.Details));

        CreateMap<UpdateGoodsReceiptDto, GoodsReceipt>()
            .ForMember(dest => dest.ReceiptNumber, opt => opt.Ignore())
            .ForMember(dest => dest.ReceiptDate, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceiptDetails, opt => opt.MapFrom(src => src.Details));

        // GoodsReceiptDetail mappings
        CreateMap<GoodsReceiptDetail, GoodsReceiptDetailDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : ""))
            .ForMember(dest => dest.ProductSku, opt => opt.MapFrom(src => src.Product != null ? src.Product.Sku : ""));

        CreateMap<CreateGoodsReceiptDetailDto, GoodsReceiptDetail>()
            .ForMember(dest => dest.GoodsReceiptId, opt => opt.Ignore())
            .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceipt, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());

        CreateMap<UpdateGoodsReceiptDetailDto, GoodsReceiptDetail>()
            .ForMember(dest => dest.GoodsReceiptId, opt => opt.Ignore())
            .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceipt, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());
    }
}
