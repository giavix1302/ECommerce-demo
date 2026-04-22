# ecommerceApiDemo

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![EF Core](https://img.shields.io/badge/EF_Core-10.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-blue)
![Docker](https://img.shields.io/badge/docker-ready-2496ED?logo=docker)

RESTful API cho hệ thống thương mại điện tử, xây dựng theo **Clean Architecture** với .NET 10. Hỗ trợ quản lý sản phẩm, đơn hàng, thanh toán trực tuyến (PayOS) và giao hàng (Ahamove).

---

## Mục lục

- [Giới thiệu dự án](#giới-thiệu-dự-án)
- [Kiến trúc](#kiến-trúc)
- [Cấu trúc thư mục](#cấu-trúc-thư-mục)
- [Tech Stack](#tech-stack)
- [Yêu cầu cài đặt](#yêu-cầu-cài-đặt)
- [Hướng dẫn chạy](#hướng-dẫn-chạy)
- [Cấu hình](#cấu-hình)

---

## Giới thiệu dự án

### Bài toán giải quyết

Xây dựng backend cho một nền tảng thương mại điện tử hoàn chỉnh — từ quản lý danh mục, sản phẩm, biến thể (variants), đến checkout, thanh toán và theo dõi vận chuyển — trong một API duy nhất, dễ mở rộng.

### Tính năng chính

| Nhóm           | Tính năng                                                             |
| -------------- | --------------------------------------------------------------------- |
| **Auth**       | Đăng ký, đăng nhập, refresh token, logout (token blacklist)           |
| **Sản phẩm**   | CRUD sản phẩm, danh mục, biến thể (size/color/...), thuộc tính động   |
| **Giỏ hàng**   | Thêm/sửa/xóa sản phẩm, áp dụng mã giảm giá                            |
| **Đơn hàng**   | Checkout, xem lịch sử đơn, preview trước khi đặt                      |
| **Khuyến mãi** | Tạo/quản lý coupon, promotion rule với điều kiện linh hoạt            |
| **Thanh toán** | Tích hợp PayOS, xử lý webhook, tự động huỷ đơn quá hạn                |
| **Vận chuyển** | Tra cứu phí ship, tích hợp Ahamove, xử lý webhook cập nhật trạng thái |
| **Đánh giá**   | Khách hàng đánh giá sản phẩm sau mua hàng                             |
| **Admin**      | Quản lý danh mục, coupon, khuyến mãi, biến thể qua API riêng          |

---

## Kiến trúc

Dự án áp dụng **Clean Architecture** kết hợp **CQRS** (Command Query Responsibility Segregation) thông qua MediatR.

```
┌─────────────────────────────────────────────────┐
│                      API                        │
│  Controllers · Middleware · Swagger · Hangfire  │
└───────────────────────┬─────────────────────────┘
                        │ depends on
┌───────────────────────▼─────────────────────────┐
│                  Application                    │
│  Commands · Queries · DTOs · Validators         │
│  Interfaces (contracts)                         │
└──────────┬────────────────────────┬─────────────┘
           │ depends on             │ implements via
┌──────────▼──────────┐  ┌──────────▼─────────────┐
│       Domain        │  │     Infrastructure      │
│  Entities · Enums   │  │  EF Core · Repositories │
│  Interfaces         │  │  JWT · Redis · PayOS    │
│  Domain logic       │  │  Ahamove · Hangfire     │
└─────────────────────┘  └────────────────────────┘
```

| Layer              | Vai trò                                                                                    |
| ------------------ | ------------------------------------------------------------------------------------------ |
| **Domain**         | Entities, enums, interface cốt lõi. Không phụ thuộc gì.                                    |
| **Application**    | Use-cases (Commands/Queries), DTO, validation, interface contracts.                        |
| **Infrastructure** | Implement các interface: database (EF Core), cache (Redis), JWT, PayOS, Ahamove, Hangfire. |
| **API**            | HTTP layer: controllers, middleware xử lý lỗi, Swagger, DI wiring.                         |

---

## Cấu trúc thư mục

```
ecommerceApiDemo/
├── src/
│   ├── API/                        # Startup project
│   │   ├── Controllers/            # REST controllers (v1)
│   │   │   ├── AuthController.cs
│   │   │   ├── ProductsController.cs
│   │   │   ├── OrdersController.cs
│   │   │   ├── CartController.cs
│   │   │   ├── PaymentController.cs
│   │   │   ├── ShippingController.cs
│   │   │   ├── ReviewsController.cs
│   │   │   ├── AdminCategoriesController.cs
│   │   │   ├── AdminCouponsController.cs
│   │   │   ├── AdminPromotionsController.cs
│   │   │   └── AdminVariantsController.cs
│   │   ├── Middleware/             # Exception handling middleware
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Program.cs
│   │
│   ├── Application/                # Business logic (CQRS)
│   │   ├── Features/
│   │   │   ├── Auth/               # Login, Register, Logout, Refresh
│   │   │   ├── Cart/               # AddToCart, UpdateCartItem, GetCart
│   │   │   ├── Categories/         # CRUD categories
│   │   │   ├── Coupon/             # CRUD coupons, Apply/Remove
│   │   │   ├── Order/              # Checkout, GetOrders, Preview
│   │   │   ├── Payment/            # PayOS webhook, ExpiredPaymentJob
│   │   │   ├── Products/           # CRUD products
│   │   │   ├── Promotion/          # CRUD promotions
│   │   │   ├── Review/             # CreateReview, GetProductReviews
│   │   │   ├── Shipping/           # GetShippingFee, Ahamove webhook
│   │   │   └── Variants/           # CRUD variants
│   │   ├── Common/
│   │   │   ├── Interfaces/         # Repository & service contracts
│   │   │   └── Behaviours/         # MediatR pipeline behaviours
│   │   └── Extensions/
│   │
│   ├── Domain/                     # Core domain
│   │   ├── Entities/               # 19 entities (User, Product, Order...)
│   │   ├── Enums/                  # OrderStatus, PaymentStatus...
│   │   └── Interfaces/
│   │
│   └── Infrastructure/             # External concerns
│       ├── Persistence/
│       │   ├── ApplicationDbContext.cs
│       │   ├── Configurations/     # EF entity configurations
│       │   ├── Migrations/
│       │   ├── Repositories/       # Repository implementations
│       │   └── Seeders/            # DatabaseSeeder
│       ├── Services/               # JWT, PayOS, Ahamove, Redis, Hangfire
│       └── Extensions/
│
├── tests/
│   └── UnitTests/                  # xUnit + NSubstitute + FluentAssertions
│
├── docs_dev/                       # Tài liệu nội bộ, seed SQL
├── Dockerfile
├── docker-compose.yml
├── .env.example
└── ecommerceApiDemo.slnx
```

---

## Tech Stack

### Core

| Công nghệ             | Phiên bản | Mục đích                 |
| --------------------- | --------- | ------------------------ |
| .NET / ASP.NET Core   | 10.0      | Web API framework        |
| Entity Framework Core | 10.0      | ORM, migrations          |
| SQL Server            | 2022      | Cơ sở dữ liệu chính      |
| Redis                 | 7         | Caching, token blacklist |

### Libraries

| Thư viện              | Phiên bản | Mục đích           |
| --------------------- | --------- | ------------------ |
| MediatR               | 14.1      | CQRS mediator      |
| AutoMapper            | 16.1      | Object mapping     |
| FluentValidation      | 12.1      | Request validation |
| Hangfire              | 1.8       | Background jobs    |
| BCrypt.Net-Next       | 4.1       | Password hashing   |
| Swashbuckle (Swagger) | 6.9       | API documentation  |

### Integrations

| Dịch vụ    | Mục đích                         |
| ---------- | -------------------------------- |
| PayOS      | Thanh toán trực tuyến (Việt Nam) |
| Ahamove    | Dịch vụ vận chuyển               |
| JWT Bearer | Authentication & authorization   |

### Testing

| Thư viện         | Mục đích       |
| ---------------- | -------------- |
| xUnit            | Test framework |
| NSubstitute      | Mocking        |
| FluentAssertions | Assertion      |

---

## Yêu cầu cài đặt

### Chạy thủ công (local)

| Yêu cầu                                                       | Phiên bản tối thiểu                   |
| ------------------------------------------------------------- | ------------------------------------- |
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0                                  |
| SQL Server                                                    | 2019+ (hoặc dùng Docker)              |
| Redis                                                         | 6+ (tùy chọn — có fallback in-memory) |
| EF Core CLI                                                   | `dotnet tool install -g dotnet-ef`    |

### Chạy bằng Docker

| Yêu cầu                           | Phiên bản tối thiểu |
| --------------------------------- | ------------------- |
| [Docker](https://www.docker.com/) | 24+                 |
| Docker Compose                    | v2+                 |

### Biến môi trường bắt buộc

| Biến                                   | Bắt buộc | Mô tả                             |
| -------------------------------------- | -------- | --------------------------------- |
| `ConnectionStrings__DefaultConnection` | ✅       | Connection string SQL Server      |
| `JwtSettings__SecretKey`               | ✅       | Ít nhất 32 ký tự                  |
| `AdminSeed__Password`                  | ✅       | Mật khẩu tài khoản admin khởi tạo |
| `PayOSSettings__ClientId`              | ✅       | PayOS client ID                   |
| `PayOSSettings__ApiKey`                | ✅       | PayOS API key                     |
| `PayOSSettings__ChecksumKey`           | ✅       | PayOS checksum key                |
| `AhamoveSettings__ApiKey`              | ✅       | Ahamove API key                   |
| `ConnectionStrings__Redis`             | ⬜       | Bỏ trống để dùng in-memory cache  |

---

## Hướng dẫn chạy

### Cách 1 — Chạy với Docker (khuyến nghị)

```bash
# 1. Clone repo
git clone https://github.com/<your-username>/ecommerceApiDemo.git
cd ecommerceApiDemo

# 2. Tạo file .env từ template
cp .env.example .env
# Mở .env và điền các giá trị cần thiết

# 3. Build và khởi động toàn bộ stack
docker compose up -d --build

# 4. Kiểm tra log
docker compose logs -f api
```

Sau khi khởi động:

| Service            | URL                            |
| ------------------ | ------------------------------ |
| API                | http://localhost:5173          |
| Swagger UI         | http://localhost:5173/swagger  |
| Hangfire Dashboard | http://localhost:5173/hangfire |
| SQL Server         | localhost:1434                 |
| Redis              | localhost:6380                 |

> **Lưu ý:** Migration và seed data chạy tự động khi API khởi động lần đầu.

#### Cấu hình webhook khi chạy local (ngrok)

Khi chạy local, PayOS và Ahamove cần một URL công khai để gọi webhook về máy của bạn. Dùng **ngrok** để tạo tunnel:

```bash
# Cài ngrok nếu chưa có: https://ngrok.com/download
ngrok http 5173
```

Ngrok sẽ in ra một URL dạng `https://xxxx-xx-xx-xx-xx.ngrok-free.app`. Dùng URL đó để cấu hình:

| Dịch vụ    | Mục cần điền          | Giá trị                                                      |
| ---------- | --------------------- | ------------------------------------------------------------ |
| **PayOS**  | Webhook URL           | `https://<ngrok-url>/api/v1/payment/payos/webhook`           |
| **Ahamove**| Webhook URL (Staging) | `https://<ngrok-url>/api/v1/shipping/webhook`                |

**PayOS** — vào [PayOS Dashboard](https://business.payos.vn) → chọn kênh thanh toán → **Webhook** → dán URL vào ô *Webhook URL* → lưu lại.

**Ahamove** — liên hệ Ahamove (staging) hoặc vào cổng quản lý để cấu hình webhook URL cho tài khoản của bạn.

> URL ngrok thay đổi mỗi lần restart. Nhớ cập nhật lại webhook trên dashboard sau mỗi lần chạy ngrok mới (trừ khi dùng [ngrok static domain](https://ngrok.com/blog-post/free-static-domains-ngrok-users)).

---

### Cách 2 — Chạy thủ công (local)

```bash
# 1. Clone repo
git clone https://github.com/<your-username>/ecommerceApiDemo.git
cd ecommerceApiDemo

# 2. Restore dependencies
dotnet restore

# 3. Cấu hình appsettings
# Điền connection string và các key vào src/API/appsettings.Development.json
# (xem mục Cấu hình bên dưới)

# 4. Cài EF Core CLI (nếu chưa có)
dotnet tool install -g dotnet-ef

# 5. Áp dụng migration
dotnet ef database update --project src/Infrastructure --startup-project src/API

# 6. Chạy ứng dụng
dotnet run --project src/API

# Hoặc chạy ở môi trường Development (bật Swagger)
dotnet run --project src/API --environment Development
```

Sau khi khởi động, API chạy tại:

- HTTP: http://localhost:5173
- HTTPS: https://localhost:7228
- Swagger: http://localhost:5173/swagger

#### Cấu hình webhook khi chạy local (ngrok)

```bash
ngrok http 5173
```

Lấy URL ngrok (`https://xxxx-xx-xx-xx-xx.ngrok-free.app`) rồi cấu hình trên dashboard của từng dịch vụ:

| Dịch vụ    | Mục cần điền          | Giá trị                                                      |
| ---------- | --------------------- | ------------------------------------------------------------ |
| **PayOS**  | Webhook URL           | `https://<ngrok-url>/api/v1/payment/payos/webhook`           |
| **Ahamove**| Webhook URL (Staging) | `https://<ngrok-url>/api/v1/shipping/webhook`                |

**PayOS** — vào [PayOS Dashboard](https://business.payos.vn) → chọn kênh thanh toán → **Webhook** → dán URL → lưu.

**Ahamove** — vào cổng quản lý staging và cấu hình webhook URL cho tài khoản của bạn.

> URL ngrok thay đổi mỗi lần restart. Nhớ cập nhật lại sau mỗi lần chạy ngrok mới.

---

### Chạy test

```bash
dotnet test tests/UnitTests
```

---

## Cấu hình

### appsettings.json — Tổng quan

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=ecommerce_db;...",
    "Redis": "localhost:6380"
  },
  "JwtSettings": {
    "SecretKey": "",
    "Issuer": "ecommerceApiDemo",
    "Audience": "ecommerceApiDemo-clients",
    "ExpiresInMinutes": 60,
    "RefreshTokenExpiresInDays": 7
  },
  "AdminSeed": {
    "Email": "admin@ecommerce.com",
    "Password": ""
  },
  "PayOSSettings": {
    "ClientId": "",
    "ApiKey": "",
    "ChecksumKey": "",
    "ReturnUrl": "http://localhost:3000/payment/return",
    "CancelUrl": "http://localhost:3000/payment/cancel"
  },
  "AhamoveSettings": {
    "BaseUrl": "https://partner-apistg.ahamove.com",
    "ApiKey": "",
    "Phone": "",
    "PickupAddress": "7/28 Thành Thái, P.14, Q.10, TP.HCM",
    "PickupName": "GiaVi Ecommerce",
    "PickupMobile": "",
    "PickupLat": 10.76975,
    "PickupLng": 106.66366
  }
}
```

### Giải thích từng section

#### `ConnectionStrings`

| Key                 | Mô tả                                                                             |
| ------------------- | --------------------------------------------------------------------------------- |
| `DefaultConnection` | Kết nối SQL Server. Đổi `Server`, `User Id`, `Password` cho phù hợp môi trường.   |
| `Redis`             | Kết nối Redis theo dạng `host:port`. **Để trống** để fallback về in-memory cache. |

#### `JwtSettings`

| Key                         | Mô tả                                                      |
| --------------------------- | ---------------------------------------------------------- |
| `SecretKey`                 | Khóa ký JWT, **tối thiểu 32 ký tự**. Giữ bí mật tuyệt đối. |
| `ExpiresInMinutes`          | Thời hạn access token (phút). Mặc định: `60`.              |
| `RefreshTokenExpiresInDays` | Thời hạn refresh token (ngày). Mặc định: `7`.              |

#### `AdminSeed`

Tài khoản admin được tạo tự động khi chạy lần đầu nếu chưa tồn tại.

| Key        | Mô tả                                                      |
| ---------- | ---------------------------------------------------------- |
| `Email`    | Email tài khoản admin.                                     |
| `Password` | Mật khẩu tài khoản admin. **Không để trống ở production.** |

#### `PayOSSettings`

Lấy các key tại [PayOS Dashboard](https://business.payos.vn).

| Key           | Mô tả                                            |
| ------------- | ------------------------------------------------ |
| `ClientId`    | Client ID của merchant.                          |
| `ApiKey`      | API key để gọi PayOS.                            |
| `ChecksumKey` | Dùng để verify webhook signature.                |
| `ReturnUrl`   | URL frontend redirect sau thanh toán thành công. |
| `CancelUrl`   | URL frontend redirect khi huỷ thanh toán.        |

#### `AhamoveSettings`

| Key                       | Mô tả                                                                                         |
| ------------------------- | --------------------------------------------------------------------------------------------- |
| `BaseUrl`                 | Staging: `https://partner-apistg.ahamove.com` · Production: `https://partner-api.ahamove.com` |
| `ApiKey`                  | API key Ahamove.                                                                              |
| `PickupAddress`           | Địa chỉ lấy hàng.                                                                             |
| `PickupLat` / `PickupLng` | Tọa độ điểm lấy hàng.                                                                         |

### Biến môi trường (Docker / Production)

Khi chạy với Docker, các key trong `appsettings.json` được override bằng biến môi trường theo cú pháp dấu `__` thay cho `:`:

```bash
# Ví dụ
ConnectionStrings__DefaultConnection=Server=sqlserver,1433;...
JwtSettings__SecretKey=my-super-secret-key-32-chars-min
PayOSSettings__ClientId=abc123
```

Tạo file `.env` từ template và điền giá trị:

```bash
cp .env.example .env
```
