# Hotel Booking API 🏨

A RESTful hotel booking API built with ASP.NET Core 8 and Entity Framework Core. The project provides hotel, room, reservation, and user management with JWT authentication and role-based authorization.

## 🚀 Features

- JWT-based authentication and authorization
- Refresh token rotation
- Password reset functionality via email
- Secure password hashing using PBKDF2
- Role-based access control (User, Admin, SuperAdmin)
- Hotel and room management
- Reservation creation and management
- Reservation date validation and availability checks
- Role-based data access and hotel-level authorization
- User profile and account management
- Centralized exception handling

## 💻 Tech Stack

- **Backend:** ASP.NET Core 8 Web API
- **Language:** C#
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core 8
- **Authentication:** JWT Bearer Authentication
- **API Documentation:** Swagger / OpenAPI
- **Testing:** xUnit, Moq, EF Core InMemory
- **CI:** GitHub Actions

## 🏗️ Project Structure

```text
HotelBookingAPI/
├── HotelBookingAPI/
│   ├── Controllers/
│   ├── Dto/
│   ├── Entity/
│   ├── Services/
│   ├── Middlewares/
│   ├── Properties/
│   └── Program.cs
│
├── HotelBookingAPITests/
│   └── Tests/
│
├── HotelBookingAPI.sln
└── README.md
```

## 🔐 Authentication & Authorization

The API uses JWT Bearer Authentication to secure endpoints.

Three authorization roles are supported:

- **User:** Can manage their own profile and reservations.
- **Admin:** Can manage resources associated with their assigned hotel.
- **SuperAdmin:** Has system-wide administrative access.

Refresh tokens are rotated during token renewal, and password reset operations invalidate existing refresh tokens.

## 🧪 Testing

The project includes automated tests written with xUnit and Moq.

Tests cover key application services and authentication-related operations.

## ⚙️ Configuration

Before running the application, configure the required settings:

- SQL Server connection string
- JWT secret key
- SMTP settings for password reset emails

Sensitive configuration values are managed through User Secrets or environment variables rather than being stored in the repository.

## 📦 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/oykuy06/HotelBookingAPI.git
cd HotelBookingAPI
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Configure application settings

Set up the required database connection, JWT secret, and SMTP configuration.

### 4. Apply database migrations

```bash
dotnet ef database update
```

### 5. Run the application

```bash
dotnet run
```

Once the application is running, access the Swagger UI to explore the available API endpoints.
