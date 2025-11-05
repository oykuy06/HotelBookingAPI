using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto dto);
        Task<string?> LoginAsync(LoginDto dto);
        Task<string?> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
