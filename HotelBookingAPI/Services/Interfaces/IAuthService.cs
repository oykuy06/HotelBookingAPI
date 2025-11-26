using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto dto);
        Task<string?> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<string?> RefreshAccessTokenAsync(string refreshToken);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto); 
    }
}
