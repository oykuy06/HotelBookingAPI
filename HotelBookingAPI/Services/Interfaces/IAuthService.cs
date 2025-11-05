using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto dto);
        Task<(string accessToken, string refreshToken)?> LoginAsync(LoginDto dto); // 🔥 burası değişti
        Task<string?> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<string?> RefreshAccessTokenAsync(string refreshToken);
    }
}
