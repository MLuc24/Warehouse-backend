# Warehouse Management System - Backend API

## 📋 Tổng quan dự án

Hệ thống quản lý kho hàng (Warehouse Management System) là một ứng dụng web giúp doanh nghiệp quản lý hoạt động nhập xuất kho một cách hiệu quả và chính xác. Dự án được xây dựng với ASP.NET Core 9.0 và Entity Framework Core.

## 🏢 Nghiệp vụ chính

### 1. Quản lý Nhập kho (Goods Receipt Management)

- **Tạo phiếu nhập kho**: Tạo phiếu nhập với số phiếu tự động (VD: NK20250803001)
- **Quy trình phê duyệt**: Workflow phê duyệt từ người tạo → người phê duyệt → xác nhận nhà cung cấp
- **Theo dõi trạng thái**: Draft → Pending Approval → Approved → Supplier Confirmed → Completed
- **Quản lý chi tiết**: Theo dõi từng sản phẩm, số lượng, giá nhập, tổng tiền
- **Cập nhật tồn kho**: Tự động cập nhật số lượng tồn kho khi hoàn thành nhập

### 2. Quản lý Xuất kho (Goods Issue Management)

- **Tạo phiếu xuất kho**: Tạo phiếu xuất với số phiếu tự động
- **Quy trình xử lý**: Draft → Approved → Prepared → Delivered → Completed
- **Quản lý giao hàng**: Theo dõi ngày yêu cầu giao, người chuẩn bị, người giao hàng
- **Kiểm soát tồn kho**: Kiểm tra tồn kho trước khi xuất, cảnh báo thiếu hàng
- **Xuất cho khách hàng**: Liên kết với thông tin khách hàng và địa chỉ giao hàng

### 3. Quản lý Sản phẩm (Product Management)

- **Danh mục sản phẩm**: Quản lý SKU, tên sản phẩm, mô tả, đơn vị tính
- **Định giá linh hoạt**: Giá nhập, giá bán, giá khuyến mãi theo thời gian
- **Phân loại sản phẩm**: Quản lý danh mục và nhà cung cấp
- **Theo dõi tồn kho**: Số lượng tồn kho theo thời gian thực
- **Cảnh báo tồn kho**: Cảnh báo khi số lượng dưới mức tối thiểu

### 4. Quản lý Đối tác (Partner Management)

- **Nhà cung cấp (Suppliers)**: Thông tin liên hệ, điều khoản thanh toán, lịch sử giao dịch
- **Khách hàng (Customers)**: Thông tin khách hàng, địa chỉ giao hàng, công nợ
- **Phân loại đối tác**: Quản lý loại đối tác và mức độ ưu tiên

### 5. Quản lý Người dùng & Phân quyền

- **Hệ thống tài khoản**: Đăng ký, đăng nhập, xác thực JWT
- **Phân quyền theo vai trò**: Admin, Manager, Staff, Viewer
- **Quản lý nhân viên**: Thông tin cá nhân, phòng ban, chức vụ
- **Theo dõi hoạt động**: Lịch sử tạo, phê duyệt, thực hiện phiếu

### 6. Báo cáo & Thống kê

- **Báo cáo tồn kho**: Tình hình tồn kho theo sản phẩm, danh mục
- **Báo cáo nhập xuất**: Thống kê nhập xuất theo thời gian, nhà cung cấp, khách hàng
- **Phân tích xu hướng**: Xu hướng nhập xuất, sản phẩm bán chạy
- **Xuất báo cáo PDF**: Tự động tạo báo cáo định dạng PDF

## 🔧 Công nghệ sử dụng

- **Framework**: ASP.NET Core 9.0
- **Database**: SQL Server với Entity Framework Core
- **Authentication**: JWT Bearer Token
- **Mapping**: AutoMapper
- **PDF Generation**: iText7
- **Environment**: DotNetEnv

## 🏗️ Kiến trúc hệ thống

### Cấu trúc thư mục chi tiết

```text
backend/
├── 📁 Controllers/              # API Controllers - Xử lý HTTP requests
│   ├── AuthController.cs        # Đăng nhập, đăng ký, làm mới token
│   ├── ProductController.cs     # CRUD sản phẩm, tìm kiếm, lọc
│   ├── CustomerController.cs    # Quản lý thông tin khách hàng
│   ├── SupplierController.cs    # Quản lý nhà cung cấp
│   ├── EmployeeController.cs    # Quản lý nhân viên
│   ├── GoodsReceiptController.cs # Xử lý phiếu nhập kho
│   ├── GoodsIssueController.cs  # Xử lý phiếu xuất kho
│   └── Product/                 # Sub-controllers cho Product
│
├── 📁 Models/                   # Database Models - Entity Framework
│   ├── Product.cs               # Sản phẩm (SKU, tên, giá, danh mục)
│   ├── Customer.cs              # Khách hàng
│   ├── Supplier.cs              # Nhà cung cấp
│   ├── User.cs                  # Người dùng và nhân viên
│   ├── GoodsReceipt.cs          # Phiếu nhập kho
│   ├── GoodsReceiptDetail.cs    # Chi tiết phiếu nhập
│   ├── GoodsIssue.cs            # Phiếu xuất kho
│   ├── GoodsIssueDetail.cs      # Chi tiết phiếu xuất
│   ├── Inventory.cs             # Tồn kho
│   └── Category.cs              # Danh mục sản phẩm
│
├── 📁 DTOs/                     # Data Transfer Objects
│   ├── Auth/                    # DTO cho authentication
│   ├── Product/                 # DTO cho sản phẩm
│   ├── Customer/                # DTO cho khách hàng
│   ├── Supplier/                # DTO cho nhà cung cấp
│   ├── Employee/                # DTO cho nhân viên
│   ├── GoodsReceipt/           # DTO cho phiếu nhập
│   └── GoodsIssue/             # DTO cho phiếu xuất
│
├── 📁 Services/                 # Business Logic Layer
│   ├── AuthService.cs           # Xử lý logic đăng nhập, JWT
│   ├── ProductService.cs        # Logic nghiệp vụ sản phẩm
│   ├── CustomerService.cs       # Logic nghiệp vụ khách hàng
│   ├── SupplierService.cs       # Logic nghiệp vụ nhà cung cấp
│   ├── GoodsReceiptService.cs   # Logic workflow phiếu nhập
│   └── GoodsIssueService.cs     # Logic workflow phiếu xuất
│
├── 📁 Repositories/             # Data Access Layer
│   ├── UserRepository.cs        # Truy cập dữ liệu User
│   ├── ProductRepository.cs     # Truy cập dữ liệu Product
│   ├── CustomerRepository.cs    # Truy cập dữ liệu Customer
│   ├── SupplierRepository.cs    # Truy cập dữ liệu Supplier
│   ├── GoodsReceiptRepository.cs # Truy cập dữ liệu phiếu nhập
│   └── GoodsIssueRepository.cs  # Truy cập dữ liệu phiếu xuất
│
├── 📁 Interfaces/               # Service Contracts
│   ├── IAuthService.cs          # Interface cho AuthService
│   ├── IProductService.cs       # Interface cho ProductService
│   ├── IBaseRepository.cs       # Base interface cho Repository
│   └── ...                      # Các interface khác
│
├── 📁 Configurations/           # AutoMapper Profiles
│   ├── ProductMappingProfile.cs # Mapping Product ↔ DTO
│   ├── CustomerMappingProfile.cs # Mapping Customer ↔ DTO
│   └── ...                      # Các mapping profile khác
│
├── 📁 Constants/                # Business Constants
│   ├── AuthConstants.cs         # Hằng số cho authentication
│   ├── ErrorMessages.cs         # Thông báo lỗi chuẩn
│   ├── RoleConstants.cs         # Định nghĩa roles
│   └── ...                      # Các constants khác
│
├── 📁 Helpers/                  # Utility Classes
│   ├── JwtHelper.cs             # Xử lý JWT token
│   ├── PasswordHelper.cs        # Mã hóa password
│   └── ...                      # Các helper khác
│
├── 📁 Data/                     # Database Context
│   └── WarehouseDbContext.cs    # EF Core DbContext
│
├── 📁 Migrations/               # Database Migrations
│   └── ...                      # Auto-generated migration files
│
└── 📁 Assets/                   # Static Assets
    └── Fonts/                   # Fonts cho PDF generation
```

### Kiến trúc phân lớp (Layered Architecture)

```text
┌─────────────────────────────────────┐
│          Presentation Layer         │  ← Controllers (API Endpoints)
├─────────────────────────────────────┤
│           Business Layer            │  ← Services (Business Logic)
├─────────────────────────────────────┤
│         Data Access Layer           │  ← Repositories (Data Access)
├─────────────────────────────────────┤
│           Database Layer            │  ← SQL Server Database
└─────────────────────────────────────┘
```

## 🚀 Cài đặt và chạy

### Yêu cầu hệ thống

- .NET 9.0 SDK
- SQL Server 2019+
- Visual Studio 2022 hoặc VS Code

### Các bước cài đặt

1. **Clone repository**

```bash
git clone <repository-url>
cd WarehouseManage/backend
```

1. **Cấu hình database**

```bash
# Tạo file .env với connection string
echo "ConnectionStrings__DefaultConnection=Server=.;Database=WarehouseDB;Trusted_Connection=true;TrustServerCertificate=true;" > .env
```

1. **Chạy migrations**

```bash
dotnet ef database update
```

1. **Chạy ứng dụng**

```bash
dotnet run
```

## 📚 API Documentation

API được thiết kế theo RESTful principles với các endpoint chính:

### Authentication & Authorization
- **POST** `/api/auth/login` - Đăng nhập (email/password)
- **POST** `/api/auth/register` - Đăng ký tài khoản mới
- **POST** `/api/auth/refresh` - Làm mới access token
- **POST** `/api/auth/logout` - Đăng xuất

### Product Management
- **GET** `/api/products` - Lấy danh sách sản phẩm (có phân trang, lọc)
- **GET** `/api/products/{id}` - Lấy chi tiết sản phẩm
- **POST** `/api/products` - Tạo sản phẩm mới
- **PUT** `/api/products/{id}` - Cập nhật sản phẩm
- **DELETE** `/api/products/{id}` - Xóa sản phẩm

### Goods Receipt (Phiếu nhập)
- **GET** `/api/goods-receipts` - Danh sách phiếu nhập
- **POST** `/api/goods-receipts` - Tạo phiếu nhập mới
- **PUT** `/api/goods-receipts/{id}/approve` - Phê duyệt phiếu nhập
- **PUT** `/api/goods-receipts/{id}/confirm` - Xác nhận từ nhà cung cấp

### Goods Issue (Phiếu xuất)
- **GET** `/api/goods-issues` - Danh sách phiếu xuất
- **POST** `/api/goods-issues` - Tạo phiếu xuất mới
- **PUT** `/api/goods-issues/{id}/prepare` - Chuẩn bị hàng
- **PUT** `/api/goods-issues/{id}/deliver` - Xác nhận giao hàng

### Partner Management
- **GET/POST/PUT/DELETE** `/api/suppliers/*` - Quản lý nhà cung cấp
- **GET/POST/PUT/DELETE** `/api/customers/*` - Quản lý khách hàng
- **GET/POST/PUT/DELETE** `/api/employees/*` - Quản lý nhân viên

**Response Format**: Tất cả API trả về JSON với cấu trúc chuẩn:
```json
{
  "success": true,
  "data": { ... },
  "message": "Success message",
  "errors": []
}
```

## 🔐 Bảo mật

### Xác thực và Phân quyền
- **JWT Authentication**: Sử dụng access token (15 phút) và refresh token (7 ngày)
- **Role-based Authorization**: 4 levels - Admin, Manager, Staff, Viewer
- **Password Security**: BCrypt hashing với salt rounds = 12
- **Token Validation**: Middleware tự động validate token cho protected routes

### Bảo vệ dữ liệu
- **Input Validation**: FluentValidation cho tất cả DTOs
- **SQL Injection Protection**: Entity Framework parameterized queries
- **XSS Protection**: Built-in ASP.NET Core protection
- **CORS Configuration**: Chỉ cho phép origins được cấu hình

### API Security Headers
```csharp
// Security headers được áp dụng
app.UseHsts();
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();
```


### Test Structure
```text
Tests/
├── UnitTests/
│   ├── Services/           # Test business logic
│   ├── Controllers/        # Test API endpoints
│   └── Repositories/       # Test data access
├── IntegrationTests/
│   ├── Api/               # End-to-end API tests
│   └── Database/          # Database integration tests
└── TestUtilities/         # Test helpers và fixtures
```

## 📝 Logging & Monitoring

### Logging Levels
Hệ thống sử dụng built-in logging của ASP.NET Core với các level:

- **Information**: Hoạt động bình thường (API calls, business operations)
- **Warning**: Cảnh báo nghiệp vụ (tồn kho thấp, validation warnings)
- **Error**: Lỗi hệ thống (exceptions, database errors)
- **Debug**: Thông tin debug (chỉ trong Development environment)

### Log Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

## 🔧 Configuration & Environment

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection="Server=.;Database=WarehouseDB;..."

# JWT Settings
JWT__SecretKey="your-super-secret-key-here"
JWT__Issuer="WarehouseManagement"
JWT__Audience="WarehouseManagement"
JWT__AccessTokenExpirationMinutes=15
JWT__RefreshTokenExpirationDays=7

# CORS Settings
CORS__AllowedOrigins="http://localhost:3000,http://localhost:5173"

# Email Settings (for notifications)
Email__SmtpServer="smtp.gmail.com"
Email__SmtpPort=587
Email__Username="your-email@domain.com"
Email__Password="your-app-password"
```

### Development vs Production
```text
Development:
- Detailed error responses
- Swagger UI enabled
- Database recreated on startup
- CORS allows all origins

Production:
- Generic error messages
- Swagger UI disabled
- Database migrations only
- Strict CORS policy
```



### Database Migrations

```bash
# Tạo migration mới
dotnet ef migrations add "AddNewFeature"

# Review migration file trước khi apply
# Apply migration
dotnet ef database update
```

## 📊 Performance & Scalability

### Database Optimization
- **Indexing**: Indexes trên foreign keys và search fields
- **Pagination**: Tất cả list APIs đều có phân trang
- **Lazy Loading**: Tắt lazy loading, sử dụng explicit Include()
- **Connection Pooling**: EF Core connection pooling enabled

### Caching Strategy
```csharp
// Memory caching cho data ít thay đổi
services.AddMemoryCache();

// Cached categories, suppliers, etc.
var categories = await _cache.GetOrCreateAsync("categories", 
    async entry => {
        entry.SlidingExpiration = TimeSpan.FromHours(1);
        return await _categoryService.GetAllAsync();
    });
```

### API Performance
- **Response Compression**: Gzip compression enabled
- **Async/Await**: Tất cả database operations đều async
- **DTO Mapping**: AutoMapper với optimized profiles
- **Request Validation**: Early validation để tránh unnecessary processing

## 🔍 Troubleshooting

### Common Issues

**1. Database Connection Issues**
```bash
# Check connection string in .env file
# Verify SQL Server is running
# Check firewall settings
```

**2. JWT Token Issues**
```bash
# Verify JWT secret key in environment variables
# Check token expiration settings
# Validate token format and claims
```

**3. Migration Issues**
```bash
# Reset database (development only)
dotnet ef database drop
dotnet ef database update

# Check for pending migrations
dotnet ef migrations list
```

