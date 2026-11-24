# HotelBookingAPI

A hotel reservation management REST API built with ASP.NET Core 8, Entity Framework Core and SQL Server. The API provides authentication, role-based authorization, hotel and room management, reservation operations and password recovery.

## Features

- JWT-based authentication and role-based authorization
- User registration, login and profile management
- Hotel and room management
- Reservation management
- Refresh token support
- Password recovery via email
- Password hashing with BCrypt
- DTO-based request and response handling
- Entity Framework Core code-first migrations
- Unit testing with xUnit and Moq

## Tech Stack

- C#
- ASP.NET Core 8
- Entity Framework Core 8
- SQL Server
- JWT
- BCrypt
- MailKit
- Swagger / OpenAPI
- xUnit
- Moq
- GitHub Actions

## Project Structure

The project follows a layered structure to separate API endpoints, business logic, data access and models.

- **Controllers:** Handle HTTP requests and responses.
- **Services:** Contain business logic.
- **Entities:** Represent database models.
- **DTOs:** Handle API request and response data.
- **Tests:** Contain unit tests for controllers and services.

## Roles & Authorization

The API uses role-based authorization with three roles:

| Role | Description |
|---|---|
| `User` | User-level operations and reservation-related functionality. |
| `Admin` | Hotel-related administrative operations. |
| `SuperAdmin` | Higher-level administrative operations, including user and hotel management. |

Authentication is handled using JWT tokens, with refresh tokens for renewing authentication sessions.

## Reservation Workflow

New reservations are created with a `Pending` status. An automated reservation approval workflow is not included in the current implementation.

## Unit Tests

The project includes unit tests for controller and service behavior using xUnit and Moq.

**Latest verified test results:**

- 197 passed
- 0 failed
- 0 skipped

Run tests with:

```bash
dotnet test
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server
- Visual Studio 2022 or a compatible IDE

### 1. Clone the repository

```bash
git clone https://github.com/oykuy06/HotelBookingAPI.git
cd HotelBookingAPI
```

### 2. Navigate to the API project

The API `.csproj` file is located in the inner `HotelBookingAPI` directory.

```bash
cd HotelBookingAPI
```

### 3. Configure application secrets

Initialize User Secrets:

```bash
dotnet user-secrets init
```

Configure the following settings:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key` (at least 32 characters)
- `Smtp:Host`
- `Smtp:Port`
- `Smtp:Username`
- `Smtp:Password`
- `Smtp:SenderEmail`

Example configuration commands:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=HotelBookingDB;Trusted_Connection=True;TrustServerCertificate=True;"

dotnet user-secrets set "Jwt:Key" "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS"

dotnet user-secrets set "Smtp:Host" "YOUR_SMTP_HOST"
dotnet user-secrets set "Smtp:Port" "YOUR_SMTP_PORT"
dotnet user-secrets set "Smtp:Username" "YOUR_SMTP_USERNAME"
dotnet user-secrets set "Smtp:Password" "YOUR_SMTP_PASSWORD"
dotnet user-secrets set "Smtp:SenderEmail" "YOUR_SENDER_EMAIL"
```

Replace the example and placeholder values with your own configuration. Do not commit credentials or secrets to the repository.

### 4. Install Entity Framework Core CLI (if not installed)

```bash
dotnet tool install --global dotnet-ef
```

### 5. Apply database migrations

From the API project directory:

```bash
dotnet ef database update
```

This applies the existing migrations to the configured SQL Server database.

### 6. Run the application

```bash
dotnet run
```

Swagger is available in the Development environment:

```text
https://localhost:xxxx/swagger
```

Replace `xxxx` with the port configured in your local launch settings.

## Initial SuperAdmin Setup

The registration endpoint creates a user with the default `User` role. To create the first SuperAdmin:

1. Register a user through the registration endpoint.
2. Update the user's role directly in the database.

Example SQL:

```sql
UPDATE Users
SET Role = 'SuperAdmin'
WHERE Email = 'your-email@example.com';
```

Replace the email with the email address of the registered account.

## Configuration

The application requires the following configuration values:

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:Key` | JWT signing key (at least 32 characters) |
| `Smtp:Host` | SMTP server host |
| `Smtp:Port` | SMTP server port |
| `Smtp:Username` | SMTP username |
| `Smtp:Password` | SMTP password |
| `Smtp:SenderEmail` | Sender email address |

Sensitive configuration values should be stored using User Secrets or environment variables and must not be committed to source control.

---

Developed as a backend portfolio project to demonstrate REST API development, authentication, authorization, database operations and unit testing using the .NET ecosystem.
