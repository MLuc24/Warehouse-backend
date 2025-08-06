using AutoMapper;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Model;

namespace WarehouseManage.Configurations;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.SupplierName : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.CurrentStock, opt => opt.MapFrom(src => src.Inventories.Sum(i => i.Quantity)))
            .ForMember(dest => dest.TotalReceived, opt => opt.MapFrom(src => src.GoodsReceiptDetails.Sum(grd => grd.Quantity)))
            .ForMember(dest => dest.TotalIssued, opt => opt.MapFrom(src => src.GoodsIssueDetails.Sum(gid => gid.Quantity)))
            .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => (src.PurchasePrice ?? 0) * src.Inventories.Sum(i => i.Quantity)));

        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            // Explicit mapping for TocoToco fields
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
            .ForMember(dest => dest.MinStockLevel, opt => opt.MapFrom(src => src.MinStockLevel))
            .ForMember(dest => dest.MaxStockLevel, opt => opt.MapFrom(src => src.MaxStockLevel))
            .ForMember(dest => dest.StorageType, opt => opt.MapFrom(src => src.StorageType))
            .ForMember(dest => dest.IsPerishable, opt => opt.MapFrom(src => src.IsPerishable))
            // Navigation properties to ignore
            .ForMember(dest => dest.Inventories, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceiptDetails, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsIssueDetails, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());

        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            // Explicit mapping for TocoToco fields
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
            .ForMember(dest => dest.MinStockLevel, opt => opt.MapFrom(src => src.MinStockLevel))
            .ForMember(dest => dest.MaxStockLevel, opt => opt.MapFrom(src => src.MaxStockLevel))
            .ForMember(dest => dest.StorageType, opt => opt.MapFrom(src => src.StorageType))
            .ForMember(dest => dest.IsPerishable, opt => opt.MapFrom(src => src.IsPerishable))
            // Navigation properties to ignore
            .ForMember(dest => dest.Inventories, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsReceiptDetails, opt => opt.Ignore())
            .ForMember(dest => dest.GoodsIssueDetails, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());
    }
}
