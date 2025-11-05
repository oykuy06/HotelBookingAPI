using HotelBookingAPI.Entity;
using HotelBookingAPI.Middlewares;
using HotelBookingAPI.Services;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
             ?? throw new Exception("JWT_KEY environment variable not set.");

var expiresInHours = jwtSettings.GetValue<int>("ExpiresInHours", 2);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<HotelBookingDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
