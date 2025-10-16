using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(long id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(string email, string password, string role = "User");
        Task<User?> UpdateUserAsync(long id, string email, string? password = null, string? role = null);
        Task<bool> DeleteUserAsync(long id);
    }
}
