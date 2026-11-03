using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterDto dto);
        Task<string?> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<AuthResponseDto?> RefreshAccessTokenAsync(string refreshToken);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto); 
    }
}
