Hotel Booking API 🏨

A comprehensive backend API for hotel reservation system built with ASP.NET Core, featuring full CRUD operations, user authentication, and role-based access control.

🚀 Features
- JWT-based Authentication & Authorization
- Hotel and Room management with admin panel
- Booking reservation system
- User profile management
- Password reset functionality

💻 Tech Stack
- **Backend:** ASP.NET Core 6.0
- **Database:** SQL Server with Entity Framework Core
- **Authentication:** JWT Bearer Tokens
- **Documentation:** Swagger/OpenAPI

📦 Quick Start

bash
# Clone repository
git clone https://github.com/oykuy06/HotelBookingAPI
cd HotelBookingAPI

# Restore dependencies
dotnet restore

# Run application
dotnet run

🔗 API Endpoints

👤 Authentication
POST /api/Auth/register - Create new user account
POST /api/Auth/login - User authentication
POST /api/Auth/forgot-password - Request password reset
POST /api/Auth/reset-password - Reset password with token

🏨 Hotel Management
GET /api/Hotel - Get all hotels
POST /api/Hotel - Create new hotel 
GET /api/Hotel/{id} - Get hotel details
PUT /api/Hotel/{id} - Update hotel information
DELETE /api/Hotel/{id} - Delete hotel

🛏️ Room Management
GET /api/Room - Get all rooms with filters
POST /api/Room - Create new room 
GET /api/Room/{id} - Get room details
PUT /api/Room/{id} - Update room information
DELETE /api/Room/{id} - Delete room

📅 Reservation System
POST /api/Reservation - Create new booking
GET /api/Reservation/me - Get user's reservations
GET /api/Reservation/{id} - Get reservation details
GET /api/Reservation/all - Get all reservations 

👥 User Management
GET /api/User/{id} - Get user profile
PUT /api/User/{id} - Update user profile
DELETE /api/User/{id} - Delete user account

🏗️ Architecture
HotelBookingAPI/
├── Controllers/     # API endpoints
├── Models/          # Data models & DTOs
├── Services/        # Business logic layer
├── Data/            # Database context & entities
└── Program.cs       # Dependency injection & configuration

