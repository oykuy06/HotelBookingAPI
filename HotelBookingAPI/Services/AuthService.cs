using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelBookingAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HotelBookingDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IPasswordService _passwordService;

        public AuthService(
            HotelBookingDBContext context,
            IConfiguration configuration,
            IPasswordHashService passwordHashService,
            IPasswordService passwordService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHashService = passwordHashService;
            _passwordService = passwordService;
        }

        public async Task<User?> RegisterAsync(UserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return null;

            var user = new User
            {
                Email = dto.Email,
                Username = dto.Email.Split('@')[0],
                PasswordHash = _passwordHashService.HashPassword(dto.Password),
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<string?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !_passwordHashService.VerifyPassword(dto.Password, user.PasswordHash))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string?> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return null;

            var token = await _passwordService.GenerateResetTokenAsync(user.Id);
            await _passwordService.SendResetEmailAsync(user.Email, token);

            return token;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _passwordService.ValidateUserTokenAsync(token);
            if (user == null)
                return false;

            user.PasswordHash = _passwordHashService.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
