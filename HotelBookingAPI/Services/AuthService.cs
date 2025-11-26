using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
            var email = dto.Email.ToLower(); 

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return null;

            var user = new User
            {
                Email = email,
                Username = email.Split('@')[0],
                PasswordHash = _passwordHashService.HashPassword(dto.Password),
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var email = dto.Email.ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !_passwordHashService.VerifyPassword(dto.Password, user.PasswordHash))
                return null;

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpiresInDays"))
            });

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }


        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")
                ?? throw new Exception("JWT_KEY not found."));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        }),
                Expires = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpiresInHours", 2)),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        // If Refresh Token is valid, generate new Access Token
        public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == refreshToken &&
                    !t.IsRevoked &&
                    t.Expiration > DateTime.UtcNow); 

            if (storedToken == null)
                return null;

            // create new token
            var newAccessToken = GenerateJwtToken(storedToken.User);

            // revoke old token
            storedToken.IsRevoked = true;

            // create new refresh token
            var newRefreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                Expiration = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpiresInDays", 7)),
                UserId = storedToken.UserId
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return newAccessToken;
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
