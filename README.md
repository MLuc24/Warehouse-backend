# Warehouse Management System - Backend API

## Giới thiệu / Introduction

Hệ thống quản lý kho hàng (Warehouse Management System) là một API backend được xây dựng với .NET 8.0, cung cấp các tính năng quản lý kho hàng toàn diện bao gồm quản lý sản phẩm, tồn kho, nhập xuất hàng, và quản lý người dùng.

A comprehensive Warehouse Management System API built with .NET 8.0, providing complete warehouse management features including product management, inventory tracking, goods receipt/issue, and user management.

## 🚀 Tính năng chính / Key Features

- **Quản lý người dùng** - User authentication với JWT, verification email/SMS
- **Quản lý sản phẩm** - CRUD operations, SKU management, pricing
- **Quản lý kho** - Multi-warehouse support, inventory tracking
- **Nhập xuất hàng** - Goods receipts và issues với detailed tracking
- **Quản lý đối tác** - Supplier và customer management
- **API Documentation** - Swagger UI integration
- **Security** - JWT authentication, role-based access control

## 🛠️ Công nghệ sử dụng / Technology Stack

Xem chi tiết tại [TECHNOLOGIES.md](./TECHNOLOGIES.md)

### Core Technologies
- .NET 9.0 & ASP.NET Core
- Entity Framework Core 9.0
- SQL Server
- JWT Authentication
- Swagger/OpenAPI

## 📋 Yêu cầu hệ thống / System Requirements

- .NET 9.0 SDK (hoặc .NET 8.0 SDK compatible)
- SQL Server (Local hoặc Remote)
- Visual Studio 2022 hoặc VS Code (khuyến nghị)

## 🔧 Cài đặt / Installation

### 1. Clone repository
```bash
git clone https://github.com/MLuc24/Warehouse-backend.git
cd Warehouse-backend
```

### 2. Restore packages
```bash
dotnet restore
```

### 3. Cấu hình database
Cập nhật connection string trong `appsettings.json` hoặc `.env`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=WareHouse;User Id=sa;Password=your-password;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 4. Chạy migrations
```bash
dotnet ef database update
```

### 5. Chạy ứng dụng
```bash
dotnet run
```

API sẽ chạy tại: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

## 🔐 Cấu hình bảo mật / Security Configuration

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "WarehouseManageAPI",
    "Audience": "WarehouseManageClient",
    "ExpiryInHours": 24
  }
}
```

### Email Configuration (SMTP)
```env
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
SMTP_FROM_EMAIL=your-email@gmail.com
```

## 🌐 API Endpoints

### Authentication
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/verify-email` - Xác thực email
- `POST /api/auth/verify-phone` - Xác thực số điện thoại

### User Management
- `GET /api/users` - Danh sách người dùng
- `GET /api/users/{id}` - Chi tiết người dùng
- `PUT /api/users/{id}` - Cập nhật người dùng
- `DELETE /api/users/{id}` - Xóa người dùng

### Product Management
- `GET /api/products` - Danh sách sản phẩm
- `POST /api/products` - Tạo sản phẩm mới
- `GET /api/products/{id}` - Chi tiết sản phẩm
- `PUT /api/products/{id}` - Cập nhật sản phẩm
- `DELETE /api/products/{id}` - Xóa sản phẩm

### Inventory Management
- `GET /api/inventory` - Tồn kho
- `GET /api/inventory/warehouse/{id}` - Tồn kho theo kho
- `PUT /api/inventory/adjust` - Điều chỉnh tồn kho

### Goods Transactions
- `GET /api/goods-receipts` - Phiếu nhập kho
- `POST /api/goods-receipts` - Tạo phiếu nhập
- `GET /api/goods-issues` - Phiếu xuất kho
- `POST /api/goods-issues` - Tạo phiếu xuất

*Chi tiết đầy đủ các endpoint có thể xem tại Swagger UI khi chạy ứng dụng*

## 📊 Database Schema

### Core Tables
- **Users** - Thông tin người dùng và xác thực
- **Products** - Sản phẩm và thông tin cơ bản
- **Warehouses** - Danh sách kho hàng
- **Suppliers** - Nhà cung cấp
- **Customers** - Khách hàng
- **Inventory** - Tồn kho realtime
- **GoodsReceipts** - Phiếu nhập kho
- **GoodsIssues** - Phiếu xuất kho
- **VerificationCodes** - Mã xác thực

## 🤝 Đóng góp / Contributing

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📝 License

Dự án này được phát hành dưới [MIT License](LICENSE).

## 👤 Tác giả / Author

**Phạm Mạnh Lực**
- GitHub: [@MLuc24](https://github.com/MLuc24)
- Email: lucp99198@gmail.com

## 🙋‍♂️ Hỗ trợ / Support

Nếu bạn gặp vấn đề hoặc có câu hỏi, vui lòng:
1. Kiểm tra [Issues](https://github.com/MLuc24/Warehouse-backend/issues) hiện tại
2. Tạo issue mới nếu cần thiết
3. Liên hệ qua email: lucp99198@gmail.com

---

*Phát triển với ❤️ bởi Phạm Mạnh Lực*