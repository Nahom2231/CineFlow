# CineFlow - Premier Cinema Ticketing & Reservation Backend API

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![C# 13](https://img.shields.io/badge/C%23-13.0-239120?logo=c-sharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16.0-4169E1?logo=postgresql&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)
![License](https://img.shields.io/badge/License-MIT-green.svg)

**CineFlow API** is an enterprise-grade RESTful Web API built with **.NET 10** and **Clean Architecture**. It powers the **CineFlow Cinema & Ticketing Platform**, providing real-time movie catalog management, multi-hall screening scheduling, seat selection matrices, background reservation cleanup, QR ticket generation, and real-time payment clearance via the **Chapa Ethiopian Payment Gateway**.

---

## ?? Architectural Overview & Key Capabilities

- **Clean Architecture & CQRS Pattern**: Decoupled domain models, application business logic using MediatR CQRS, infrastructure services, and presentation controllers.
- **Movie Catalog & Metadata Engine**: Complete movie management supporting multi-language metadata (English & Amharic), director/cast tracking, and flexible ID/slug resolution.
- **Auditorium & Seat Matrix Management**: Interactive seating layouts for Standard & VIP Recliner lounges with real-time seat lock and reservation handling.
- **Chapa Ethiopian Payment Gateway**: Instant transaction initialization, web checkout redirection, payment verification, and webhook notifications for Telebirr, CBE Birr, Awash Birr, and Card payments.
- **Background Reservation Cleanup Worker**: Hosted background worker (ExpiredReservationCleanupWorker) that automatically releases unpaid/unconfirmed seat holds after timeout.
- **Digital Tickets & Contactless QR Pass**: Automatic ticket generation and QR Code SVG/PNG rendering for fast contactless venue verification.
- **Identity & Security**: ASP.NET Core Identity with JWT Bearer Token authentication, role-based authorization (Admin vs Customer), account lockout protection, and AES data payload encryption.

---

## ??? Tech Stack & Dependencies

| Layer | Technologies / Libraries |
| :--- | :--- |
| **Framework & Runtime** | .NET 10.0 Web API, C# 13 |
| **Architecture** | Clean Architecture, CQRS (MediatR), Repository Pattern |
| **Database & ORM** | PostgreSQL, Entity Framework Core 10 (Npgsql Provider) |
| **Authentication & AuthZ** | ASP.NET Core Identity, JWT Bearer Tokens, Custom Authorization Policies |
| **Payments** | Chapa API (pi.chapa.co), Multi-Bank Clearance |
| **Real-time Broadcast** | ASP.NET Core SignalR Hubs (TmsHub) |
| **Background Processing** | IHostedService Background Service Workers |
| **API Reference & OpenAPI** | Scalar API Reference, OpenAPI / Swagger UI |
| **Security & Utilities** | AES Payload Encryption Service, QRCoder SVG Generator |

---

## ?? Project Structure

`
CineFlow/
+-- CineFlow.Api/                    # Presentation Layer (Controllers, Middleware, Program.cs)
¦   +-- Controllers/                 # MoviesController, ScheduleController, CinemaHallController, PaymentController, TicketsController, AuthController
¦   +-- Properties/                  # launchSettings.json (Port 5066 HTTP)
¦   +-- appsettings.json             # DB connection strings, JWT, Chapa & Encryption settings
+-- CineFlow.Application/            # Application Logic Layer (CQRS Commands, Queries, Interfaces, DTOs)
¦   +-- Interfaces/                  # IPaymentService, IEncryptionService, ICineFlowDbContext
¦   +-- Movies/                      # Movie CQRS Commands & Queries
¦   +-- Schedules/                   # Schedule CQRS Commands & Queries
¦   +-- Configuration/               # Options Pattern (ChapaOptions, EncryptionOptions)
+-- CineFlow.Domain/                 # Core Domain Layer (Entities, Enums, Value Objects)
¦   +-- Entities/                    # Movie, CinemaHall, Schedule, Ticket, SeatReservation, Director, Star
+-- CineFlow.Infrastructure/         # Infrastructure & Persistence Layer
    +-- Persistence/                 # EF Core DbContext, Entity Configurations, Migrations
    +-- Payments/                    # ChapaPaymentService Implementation
    +-- Security/                    # EncryptionService (AES)
    +-- Services/                    # QR Code Service, Background Workers
`

---

## ?? Configuration & Environment Setup

### 1. Database Connection (ppsettings.json)
Configure your PostgreSQL database connection string in CineFlow.Api/appsettings.json:

`json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=CineFlowDb;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "Secret": "CineFlowSuperSecureEnterpriseTokenSigningPrivateKey2026",
    "Issuer": "CineFlowApi",
    "Audience": "CineFlowAngularClient",
    "ExpiryMinutes": 120
  },
  "Chapa": {
    "SecretKey": "CHASECK_TEST-xxxxxxxxxxxxxxxxxxxx",
    "PublicKey": "CHAPUBK_TEST-xxxxxxxxxxxxxxxxxxxx",
    "BaseUrl": "https://api.chapa.co",
    "CallbackUrl": "http://localhost:5066/api/v1/payment/callback"
  },
  "Encryption": {
    "Key": "CineFlowSecretEncryptionKey32Byte",
    "IV": "CineFlowSecretIV"
  }
}
`

---

## ?? Installation & Running Locally

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed
- [PostgreSQL Database Server](https://www.postgresql.org/download/) running on localhost:5432

### 1. Clone the Repository
`ash
git clone https://github.com/Nahom2231/CineFlow.git
cd CineFlow
`

### 2. Apply Database Migrations
`ash
dotnet ef database update --project CineFlow.Infrastructure --startup-project CineFlow.Api
`

### 3. Run the API Server
`ash
dotnet run --project CineFlow.Api/CineFlow.Api.csproj --launch-profile http
`

The API will start listening on **http://localhost:5066**.

### 4. Interactive API Documentation
Access the interactive OpenAPI & Scalar documentation in your browser at:
- **Scalar API Reference**: http://localhost:5066/scalar/v1
- **OpenAPI Json**: http://localhost:5066/openapi/v1.json
- **Swagger UI**: http://localhost:5066/swagger

---

## ?? Key API Endpoints Reference

### ?? Movies Endpoint
| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| GET | /api/v1/Movies | Public | List all movies with optional genre/title/audio filters |
| GET | /api/v1/Movies/{id} | Public | Get movie details by GUID or slug identifier (e.g. m6-batman) |
| POST | /api/v1/Movies | Admin | Create a new movie entry with poster upload |
| DELETE | /api/v1/Movies/{id} | Admin | Remove a movie from the catalog |

### ??? Cinema Halls & Showtimes
| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| GET | /api/v1/CinemaHall | Public | List all cinema auditoriums & seating capacity |
| GET | /api/v1/CinemaHall/{id} | Public | Get auditorium details and seat matrix |
| GET | /api/v1/Schedule/all | Public | Retrieve all screening showtimes |
| GET | /api/v1/Schedule/movie/{movieId}| Public | Get active showtimes for a specific movie |
| POST | /api/v1/Schedule | Admin | Create a new movie screening schedule |

### ?? Chapa Payment Gateway
| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| GET | /api/v1/Payment/config | Public | Get public payment gateway configuration |
| POST | /api/v1/Payment/initialize | Public | Initialize Chapa transaction and get hosted checkout URL |
| GET | /api/v1/Payment/verify/{reference} | Public | Verify payment transaction status |
| POST | /api/v1/Payment/callback | Webhook | Process Chapa transaction status webhooks |

### ??? Tickets & Booking
| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| GET | /api/v1/Tickets/{scheduleId}/seats | Public | Get real-time seat availability for showtime |
| POST | /api/v1/Tickets/book | User | Reserve selected seats and issue digital ticket |
| GET | /api/v1/Tickets/{ticketId} | User | Retrieve digital ticket details and QR code SVG |

---

## ????? Author & Contribution

Developed and maintained by **Nahom** ([@Nahom2231](https://github.com/Nahom2231)).

---

## ?? License

This project is licensed under the [MIT License](LICENSE).