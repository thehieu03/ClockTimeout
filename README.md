# E-Commerce Microservices Platform

Hệ thống microservices cho nền tảng thương mại điện tử được xây dựng bằng .NET 8.0, áp dụng Clean Architecture và các pattern hiện đại.

## 📋 Mục lục

- [Tổng quan](#tổng-quan)
- [Kiến trúc](#kiến-trúc)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt và chạy](#cài-đặt-và-chạy)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Cơ sở dữ liệu](#cơ-sở-dữ-liệu)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Development](#development)

## 🎯 Tổng quan

Dự án này là một nền tảng microservices cho hệ thống thương mại điện tử, được thiết kế với kiến trúc Clean Architecture và Domain-Driven Design (DDD). Hệ thống hỗ trợ nhiều dịch vụ độc lập có thể mở rộng và bảo trì dễ dàng.

### Tính năng chính

- ✅ Clean Architecture với phân tách rõ ràng các layer
- ✅ CQRS Pattern với MediatR
- ✅ Domain-Driven Design (DDD)
- ✅ Event-Driven Architecture
- ✅ API Gateway pattern
- ✅ Distributed Tracing với OpenTelemetry
- ✅ Structured Logging với Serilog
- ✅ Health Checks
- ✅ Swagger/OpenAPI Documentation
- ✅ Containerization với Docker

## 🏗️ Kiến trúc

### Kiến trúc tổng thể

```
┌─────────────────┐
│   API Gateway   │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
┌───▼───┐ ┌───▼───┐
│Catalog│ │Other │
│Service│ │Services│
└───────┘ └───────┘
```

### Clean Architecture Layers

Mỗi service được tổ chức theo Clean Architecture:

```
┌─────────────────────────────────────┐
│           API Layer                  │  ← Controllers/Endpoints
├─────────────────────────────────────┤
│        Application Layer            │  ← Use Cases, DTOs, Validators
├─────────────────────────────────────┤
│         Domain Layer                 │  ← Entities, Value Objects, Domain Events
├─────────────────────────────────────┤
│      Infrastructure Layer           │  ← Data Access, External Services
└─────────────────────────────────────┘
```

## 🛠️ Công nghệ sử dụng

### Core Technologies

- **.NET 8.0** - Framework chính
- **C#** - Ngôn ngữ lập trình
- **ASP.NET Core** - Web framework

### Libraries & Frameworks

- **MediatR** (14.0.0) - CQRS implementation
- **AutoMapper** (12.0.1) - Object mapping
- **FluentValidation** (12.1.1) - Input validation
- **Marten** (8.17.0) - Document database cho PostgreSQL
- **Entity Framework Core** (8.0.11) - ORM
- **Carter** (8.0.0) - Minimal API framework
- **Serilog** (4.3.0) - Structured logging
- **OpenTelemetry** (1.14.0) - Distributed tracing
- **Swashbuckle** (8.1.4) - Swagger/OpenAPI

### Databases & Storage

- **PostgreSQL 16** - Primary database (Marten)
- **MongoDB 7.0** - Document database
- **MySQL 8.0** - Relational database
- **SQL Server 2022** - Enterprise database
- **Redis 7.0** - Caching & session storage

### Message Broker & Queue

- **RabbitMQ 3.11** - Message broker

### Testing

- **MSTest** (4.0.1) - Testing framework
- **Moq** (4.20.70) - Mocking framework
- **Microsoft.Playwright.MSTest** (1.55.0) - E2E testing

## 📦 Yêu cầu hệ thống

### Prerequisites

- **.NET SDK 8.0** hoặc cao hơn
- **Docker Desktop** (cho Windows/Mac) hoặc **Docker Engine** (cho Linux)
- **Docker Compose** (thường đi kèm với Docker Desktop)
- **Git**

### Recommended Tools

- **Visual Studio 2022** hoặc **Visual Studio Code**
- **Postman** hoặc **Swagger UI** (cho API testing)
- **Docker Desktop** với WSL2 (cho Windows)

## 🚀 Cài đặt và chạy

### 1. Clone repository

```bash
git clone <repository-url>
cd Microservices
```

### 2. Khởi động Infrastructure Services

Chạy các dịch vụ cơ sở hạ tầng (databases, Redis, RabbitMQ):

```bash
docker-compose -f docker-compose.infrastructure.yml up -d
```

Các dịch vụ sẽ được khởi động trên các port sau:
- **PostgreSQL**: `localhost:5433`
- **MongoDB**: `localhost:27018`
- **MySQL**: `localhost:3307`
- **SQL Server**: `localhost:1434`
- **Redis**: `localhost:6379`
- **RabbitMQ Management**: `http://localhost:15673` (admin/admin123)

### 3. Restore packages

```bash
dotnet restore
```

### 4. Build solution

```bash
dotnet build eccomere.slnx
```

### 5. Chạy Catalog API

```bash
cd src/Services/Catalog/Api/Catalog.Api
dotnet run
```

Hoặc chạy từ root:

```bash
dotnet run --project src/Services/Catalog/Api/Catalog.Api/Catalog.Api.csproj
```

API sẽ chạy tại: `http://localhost:5000` hoặc `https://localhost:5001`

### 6. Truy cập Swagger UI

Mở trình duyệt và truy cập:
```
http://localhost:5000/swagger
```

### 7. Chạy với Docker (Optional)

Build và chạy Catalog API với Docker:

```bash
docker-compose up -d catalog-api
```

API sẽ chạy tại: `http://localhost:5112`

## 📁 Cấu trúc dự án

```
Microservices/
├── src/
│   ├── Services/                    # Microservices
│   │   └── Catalog/                 # Catalog Service
│   │       ├── Api/                 # API Layer (Endpoints)
│   │       ├── Core/
│   │       │   ├── Application/     # Application Layer (Use Cases, DTOs)
│   │       │   ├── Domain/          # Domain Layer (Entities, Domain Logic)
│   │       │   └── Infrastructure/ # Infrastructure Layer (Data Access)
│   │       └── Test/                # Unit & Integration Tests
│   │
│   └── Shared/                      # Shared Libraries
│       ├── BuildingBlocks/         # Cross-cutting concerns
│       │   ├── CQRS/               # CQRS abstractions
│       │   ├── Pagination/         # Pagination utilities
│       │   ├── Authentication/     # Auth extensions
│       │   ├── Logging/            # Logging setup
│       │   └── Swagger/            # Swagger configuration
│       └── Common/                  # Common utilities
│           ├── Constants/           # Application constants
│           ├── Models/              # Shared models
│           └── ValueObjects/       # Value objects
│
├── docker-compose.yml               # Main docker-compose
├── docker-compose.infrastructure.yml # Infrastructure services
├── Directory.Build.props            # Common build properties
├── Directory.Packages.props         # Centralized package versions
└── eccomere.slnx                   # Solution file
```

## 🗄️ Cơ sở dữ liệu

### Connection Strings

Các connection strings được cấu hình trong `appsettings.json` hoặc environment variables:

**PostgreSQL (Marten)**
```
Host=postgres-sql;Port=5432;Database=catalog_db;Username=postgres;Password=postgres123
```

**MongoDB**
```
mongodb://admin:admin123@localhost:27018
```

**MySQL**
```
Server=localhost;Port=3307;Database=inventory_db;User=root;Password=root123
```

**SQL Server**
```
Server=localhost,1434;Database=master;User Id=sa;Password=SqlServer123!;TrustServerCertificate=True
```

**Redis**
```
localhost:6379,password=redis123
```

### Database Migrations

Để tạo migrations cho Entity Framework:

```bash
dotnet ef migrations add <MigrationName> --project src/Services/Catalog/Core/Catalog.Infrastructure
```

Để apply migrations:

```bash
dotnet ef database update --project src/Services/Catalog/Core/Catalog.Infrastructure
```

## 📚 API Documentation

### Swagger UI

Sau khi chạy ứng dụng, truy cập Swagger UI tại:
```
http://localhost:5000/swagger
```

### API Endpoints

#### Catalog Service

- `POST /api/products` - Tạo sản phẩm mới
- `GET /api/products/{id}` - Lấy thông tin sản phẩm theo ID
- `GET /api/products` - Lấy danh sách sản phẩm (có phân trang)
- `PUT /api/products/{id}` - Cập nhật sản phẩm
- `DELETE /api/products/{id}` - Xóa sản phẩm

### Authentication

API sử dụng JWT Bearer authentication. Để gọi API, cần thêm header:
```
Authorization: Bearer <your-token>
```

## 🧪 Testing

### Chạy Unit Tests

```bash
dotnet test
```

### Chạy Integration Tests

```bash
dotnet test --filter Category=Integration
```

### Chạy tests cho một project cụ thể

```bash
dotnet test src/Services/Catalog/Test/TestProject1/TestProject1.csproj
```

## 💻 Development

### Coding Standards

- Sử dụng **C# coding conventions**
- **Nullable reference types** được bật
- **Implicit usings** được enable
- Sử dụng **file-scoped namespaces**

### Git Workflow

1. Tạo branch mới từ `master`:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Commit changes:
   ```bash
   git add .
   git commit -m "feat: your feature description"
   ```

3. Push và tạo Pull Request

### Environment Variables

Tạo file `.env` từ `.env.example` và cấu hình các biến môi trường cần thiết.

### Logging

Logging được cấu hình với Serilog và có thể export đến:
- Console
- OpenTelemetry
- File (tùy cấu hình)

### Health Checks

Health checks endpoint:
```
GET /health
```

## 🔧 Troubleshooting

### Port đã được sử dụng

Nếu port đã được sử dụng, thay đổi port trong `docker-compose.infrastructure.yml` hoặc `appsettings.json`.

### Database connection issues

1. Kiểm tra các container đã chạy:
   ```bash
   docker ps
   ```

2. Kiểm tra logs:
   ```bash
   docker logs postgres-sql
   ```

3. Kiểm tra connection string trong configuration

### Build errors

1. Clean solution:
   ```bash
   dotnet clean
   ```

2. Restore packages:
   ```bash
   dotnet restore
   ```

3. Rebuild:
   ```bash
   dotnet build --no-incremental
   ```

## 📝 License

[Thêm license của bạn ở đây]

## 👥 Contributors

[Thêm danh sách contributors]

## 📞 Support

[Thêm thông tin liên hệ hoặc support channels]
