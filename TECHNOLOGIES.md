# Danh sách các công nghệ sử dụng trong dự án / Technologies Used in This Project

## Tổng quan / Overview

Dự án **Warehouse Management System** là một hệ thống quản lý kho hàng được xây dựng bằng .NET Core API với kiến trúc RESTful và các công nghệ hiện đại.

**Warehouse Management System** is a warehouse management system built with .NET Core API using RESTful architecture and modern technologies.

---

## 🚀 Công nghệ cốt lõi / Core Technologies

### Backend Framework
- **ASP.NET Core 9.0** - Framework phát triển web API
- **.NET 9.0** - Nền tảng runtime và SDK (compatible với .NET 8.0)
- **C# 12** - Ngôn ngữ lập trình chính

> **Lưu ý tương thích / Compatibility Note**: Dự án được thiết kế cho .NET 9.0 nhưng có thể chạy trên .NET 8.0 SDK với một số điều chỉnh phiên bản packages.

### Cơ sở dữ liệu / Database
- **Microsoft SQL Server** - Hệ quản trị cơ sở dữ liệu quan hệ
- **Entity Framework Core 8.0.10** - ORM (Object-Relational Mapping)
- **EF Core Code First Migrations** - Quản lý schema database

---

## 📦 Thư viện và Dependencies / Libraries and Dependencies

### Authentication & Security
- **Microsoft.AspNetCore.Authentication.JwtBearer (9.0.4)**
  - Xác thực JWT Bearer Token
- **Microsoft.IdentityModel.Tokens (8.0.1)**
  - Xử lý và validation JWT tokens
- **System.IdentityModel.Tokens.Jwt (8.0.1)**
  - Tạo và xử lý JWT tokens

### Database Access
- **Microsoft.EntityFrameworkCore.SqlServer (9.0.4)**
  - SQL Server provider cho Entity Framework Core
- **Microsoft.EntityFrameworkCore.Tools (9.0.4)**
  - EF Core CLI tools cho migrations
- **Microsoft.EntityFrameworkCore.Design (9.0.4)**
  - Design-time services cho EF Core

### API Documentation
- **Microsoft.AspNetCore.OpenApi (9.0.4)**
  - OpenAPI specification support
- **Swashbuckle.AspNetCore (9.0.3)**
  - Swagger/OpenAPI documentation generator

### Object Mapping
- **AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.1)**
  - Object-to-object mapping library

---

## 🏗️ Kiến trúc và Patterns / Architecture and Patterns

### Design Patterns
- **Repository Pattern** - Thông qua Entity Framework Core
- **Dependency Injection** - Built-in DI container của .NET Core
- **Model-View-Controller (MVC)** - API Controllers pattern
- **Code First** - Database modeling approach

### API Architecture
- **RESTful API** - REST architectural style
- **JWT Authentication** - Stateless authentication
- **CORS Support** - Cross-Origin Resource Sharing
- **Middleware Pipeline** - Request processing pipeline

---

## 🔧 Development Tools và Environment

### Development Environment
- **Visual Studio / VS Code** - IDE support
- **Git** - Version control system
- **.NET CLI** - Command line interface tools
- **Entity Framework CLI** - Database migration tools

### Configuration Management
- **appsettings.json** - Application configuration
- **Environment Variables** - Secure configuration (.env file)
- **Configuration Providers** - Multiple configuration sources

---

## 🌐 External Integrations / Tích hợp bên ngoài

### Email Services
- **SMTP Integration** - Gmail SMTP server
  - Username: Configured via environment variables
  - SSL/TLS support for secure email delivery

### SMS Services (Optional)
- **Twilio SMS API** - SMS verification service
  - Account SID and Auth Token configuration
  - Phone number verification support

### Frontend Integration
- **CORS Configuration** - Support for React/Vue.js frontends
  - Allowed origins: `http://localhost:5173`, `http://localhost:3000`
  - Common development server ports

---

## 🗃️ Data Models / Mô hình dữ liệu

### Core Entities
- **Users** - Quản lý người dùng và phân quyền
- **Products** - Sản phẩm và thông tin kho
- **Warehouses** - Thông tin kho hàng
- **Suppliers** - Nhà cung cấp
- **Customers** - Khách hàng
- **Inventory** - Tồn kho theo từng kho
- **GoodsReceipts** - Phiếu nhập kho
- **GoodsIssues** - Phiếu xuất kho
- **VerificationCodes** - Mã xác thực email/SMS

---

## 🔐 Security Features / Tính năng bảo mật

### Authentication
- **JWT Bearer Authentication** - Stateless token-based authentication
- **Password Hashing** - Secure password storage
- **Email/Phone Verification** - Two-factor verification system

### Authorization
- **Role-based Access Control** - User roles and permissions
- **API Security** - Protected endpoints with JWT tokens

### CORS Security
- **Configured Origins** - Restricted cross-origin access
- **Credential Support** - Secure credential handling

---

## 🚀 Deployment và Production

### Environment Support
- **Development Environment** - Local development settings
- **Production Environment** - Production-ready configuration
- **Environment Variables** - Secure configuration management

### Database Configuration
- **Connection Pooling** - Optimized database connections
- **Connection Retry** - Resilient database connectivity
- **Timeout Configuration** - Connection and command timeouts

### Performance Features
- **HttpClient Configuration** - Optimized API validation
- **SSL Certificate Bypass** - Development environment support
- **Connection Pool Management** - Min/Max pool size configuration

---

## 📋 API Features / Tính năng API

### Core Functionality
- **Warehouse Management** - Multi-warehouse support
- **Inventory Tracking** - Real-time inventory management
- **User Management** - Registration, login, verification
- **Product Management** - CRUD operations for products
- **Supplier/Customer Management** - Contact management
- **Goods Receipt/Issue** - Import/export transactions

### API Documentation
- **Swagger UI** - Interactive API documentation
- **OpenAPI Specification** - Standard API documentation format
- **JWT Authentication in Swagger** - Secured API testing

---

## 🔄 Development Workflow

### Database Management
- **Code First Migrations** - Automated schema management
- **Migration History** - Version-controlled database changes
- **Seed Data** - Initial data setup

### Version Control
- **Git Integration** - Source code management
- **.gitignore** - Comprehensive ignore patterns
- **MIT License** - Open source licensing

---

*Tài liệu này được tạo tự động dựa trên phân tích mã nguồn dự án.*
*This documentation is automatically generated based on project source code analysis.*

**Phiên bản dự án / Project Version**: 1.0.0  
**Ngày cập nhật / Last Updated**: $(date)  
**Tác giả / Author**: Phạm Mạnh Lực