using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IPasswordService
    {
        Task<string> GenerateResetTokenAsync(long userId);
        Task<PasswordReset?> GetValidResetTokenAsync(string token);
        Task SendResetEmailAsync(string email, string token);
    }
}
