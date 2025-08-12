# Backend - Warehouse Management API

Web API được xây dựng với ASP.NET Core 9.0 cho hệ thống quản lý kho hàng.

## Công nghệ sử dụng

- **Framework**: ASP.NET Core 9.0
- **Database**: SQL Server với Entity Framework Core 9.0
- **Authentication**: JWT Bearer Token
- **Mapping**: AutoMapper
- **PDF Generation**: iText7
- **Configuration**: Environment Variables (.env)

## Cấu trúc dự án

```
backend/
├── Controllers/          # API Controllers
├── Services/            # Business Logic Services
├── Repositories/        # Data Access Layer
├── Models/             # Entity Models
├── DTOs/               # Data Transfer Objects
├── Data/               # DbContext
├── Migrations/         # EF Migrations
├── Configurations/     # AutoMapper Profiles
├── Constants/          # Application Constants
├── Helpers/            # Utility Classes
├── Interfaces/         # Interface Definitions
├── Extensions/         # Extension Methods
└── Utilities/          # Utility Classes
```

## Cài đặt

1. Cài đặt dependencies:
```bash
dotnet restore
```

2. Tạo file `.env` trong thư mục backend:
```
ConnectionStrings__DefaultConnection=Server=.;Database=WarehouseDB;Trusted_Connection=True;TrustServerCertificate=True;
Jwt__Key=your-secret-key-here
Jwt__Issuer=WarehouseManage
Jwt__Audience=WarehouseManage
Jwt__ExpiryInHours=24
```

3. Chạy migrations:
```bash
dotnet ef database update
```

4. Khởi động ứng dụng:
```bash
dotnet run
```

API sẽ chạy tại: `https://localhost:7139` hoặc `http://localhost:5139`

## API Endpoints

### Authentication
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/register` - Đăng ký

### Products & Categories
- `GET /api/category` - Lấy danh sách danh mục
- `GET /api/product` - Quản lý sản phẩm

### Customers & Suppliers
- `GET /api/customer` - Quản lý khách hàng
- `GET /api/supplier` - Quản lý nhà cung cấp

### Warehouse Operations
- `GET /api/goodsreceipt` - Quản lý phiếu nhập kho
- `GET /api/goodsissue` - Quản lý phiếu xuất kho

### Profile
- `GET /api/profile` - Quản lý thông tin người dùng

## Database

Sử dụng SQL Server với Entity Framework Core. Các models chính:
- `User`, `Customer`, `Supplier`
- `Category`, `Product`
- `GoodsReceipt`, `GoodsIssue`
- Và các bảng liên kết

## Authentication

Sử dụng JWT Bearer Token với các role:
- Admin
- Employee
- Manager

## Swagger Documentation

Khi chạy trong môi trường Development, có thể truy cập Swagger UI tại:
`https://localhost:7139/swagger`
