# Hotel Booking API

A RESTful hotel booking API built with **ASP.NET Core 8**, **Entity Framework Core**, and **SQL Server**. The project provides user authentication, role-based authorization, hotel and room management, and reservation operations.

## Features

- **Authentication & Authorization**
  - JWT-based authentication.
  - Refresh token management and renewal.
  - Role-based access control (`User`, `Admin`, `SuperAdmin`).
  - Password hashing using PBKDF2 with HMAC-SHA256.
  - Password reset functionality.

- **Hotel & Room Management**
  - Hotel and room CRUD operations.
  - Hotel-based access restrictions for administrators.
  - Room capacity and price validation.
  - Duplicate hotel and room validation.

- **Reservation Management**
  - Create and manage reservations.
  - Check-in and check-out date validation.
  - Room availability and reservation conflict checks.
  - Automatic reservation price calculation.
  - New reservations are created with a `Pending` status. An automated reservation approval workflow is not implemented.

- **Additional Features**
  - Centralized exception handling middleware.
  - Request validation using Data Annotations.
  - Email notifications through MailKit.
  - Entity Framework Core relationships and configurations.
  - Swagger / OpenAPI documentation.

## Tech Stack

| Category | Technologies |
|---|---|
| Backend | C#, ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Database | Microsoft SQL Server |
| Authentication | JWT, Refresh Tokens |
| Security | PBKDF2, HMAC-SHA256 |
| Email | MailKit |
| API Documentation | Swagger / OpenAPI |
| Testing | xUnit, Moq, EF Core InMemory |
| CI | GitHub Actions |

## Architecture

The project follows a layered structure to separate API endpoints, business logic, and data access.

- **Controllers:** Handle HTTP requests and responses.
- **Services:** Contain application and business logic.
- **Entity:** Contains domain models and Entity Framework Core configurations.
- **DTOs:** Separate request and response models from database entities.
- **Middlewares:** Handle cross-cutting concerns such as unhandled exceptions.
- **Tests:** Contain unit tests for application behavior.

## Role-Based Access Control

The API supports three user roles:

| Role | Permissions |
|---|---|
| User | Access personal information and perform permitted reservation operations. |
| Admin | Manage resources and users within their assigned hotel, subject to authorization rules. |
| SuperAdmin | Perform administrative operations across hotels, including role and hotel assignments. |

Authorization rules are enforced at the API level to prevent users and administrators from accessing resources outside their permissions.

## Project Structure

```text
HotelBookingAPI/
│
├── HotelBookingAPI.sln
│
├── HotelBookingAPI/
│   ├── Controllers/
│   ├── Dto/
│   ├── Entity/
│   │   ├── Configurations/
│   │   └── Models/
│   ├── Middlewares/
│   ├── Services/
│   │   └── Interfaces/
│   ├── Migrations/
│   ├── Properties/
│   ├── appsettings.json
│   └── Program.cs
│
├── HotelBookingAPITests/
│   └── Tests/
│
└── README.md
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Microsoft SQL Server
- Visual Studio 2022 or another .NET-compatible IDE
- Entity Framework Core CLI

Install the EF Core CLI if it is not already installed:

```bash
dotnet tool install --global dotnet-ef
```

### 1. Clone the Repository

```bash
git clone https://github.com/oykuy06/HotelBookingAPI.git
cd HotelBookingAPI
```

### 2. Restore Dependencies

Run this command from the solution directory:

```bash
dotnet restore
```

### 3. Configure User Secrets

The application uses .NET User Secrets for sensitive configuration values instead of storing credentials in `appsettings.json`.

Navigate to the API project directory:

```bash
cd HotelBookingAPI/HotelBookingAPI
```

Configure the following required values:

| Configuration Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:Key` | JWT signing key (at least 32 characters) |
| `Smtp:Host` | SMTP server address |
| `Smtp:Port` | SMTP server port |
| `Smtp:Username` | SMTP username |
| `Smtp:Password` | SMTP password |
| `Smtp:SenderEmail` | Sender email address |

Example commands:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=HotelBookingDB;Trusted_Connection=True;TrustServerCertificate=True;"

dotnet user-secrets set "Jwt:Key" "YOUR_RANDOM_SECRET_KEY_AT_LEAST_32_CHARACTERS"

dotnet user-secrets set "Smtp:Host" "YOUR_SMTP_HOST"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:Username" "YOUR_SMTP_USERNAME"
dotnet user-secrets set "Smtp:Password" "YOUR_SMTP_PASSWORD"
dotnet user-secrets set "Smtp:SenderEmail" "YOUR_SENDER_EMAIL"
```

Replace the example values with your own configuration.

**Security Note:** Never commit real connection strings, JWT signing keys, SMTP credentials, or other sensitive information to source control. Use User Secrets for local development and environment variables or a secure secret manager in production.

### 4. Apply Database Migrations

From the API project directory:

```bash
dotnet ef database update
```

This creates or updates the database according to the existing Entity Framework Core migrations.

### 5. Run the Application

```bash
dotnet run
```

When running in the Development environment, Swagger UI is available at:

```text
https://localhost:<port>/swagger
```

Use the HTTPS URL and port displayed in your application's startup output.

## Initial SuperAdmin Setup

Newly registered users do not automatically receive the `SuperAdmin` role.

To create the initial SuperAdmin account:

1. Register a user through the API.
2. Open the application's SQL Server database.
3. Update the registered user's role:

```sql
UPDATE Users
SET Role = 'SuperAdmin'
WHERE Email = 'your-email@example.com';
```

Replace the email with the registered account's email address.

After this update, the account can perform SuperAdmin operations.

## Running Tests

The project uses **xUnit** and **Moq** for testing, with EF Core InMemory where applicable.

Run the tests from the solution directory:

```bash
dotnet test
```

### Test Results

- **197 Passed**
- **0 Failed**
- **0 Skipped**

Tests cover authentication, authorization, service behavior, password hashing, and other application scenarios.

## Continuous Integration

The project uses **GitHub Actions** to automatically restore dependencies, build the solution, and run tests when changes are pushed to the `main` branch or a pull request targets `main`.

[![CI](https://github.com/oykuy06/HotelBookingAPI/workflows/CI/badge.svg)](https://github.com/oykuy06/HotelBookingAPI/actions)

The workflow verifies that the solution builds successfully and that the automated tests pass.

---

## License

This project is available for educational and portfolio purposes. No specific open-source license has been assigned.
