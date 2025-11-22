using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto?> GetUserByIdAsync(long id);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<User> CreateUserAsync(string email, string password, string role = "User");
        Task<User?> UpdateUserAsync(long id, string email, string? password = null, string? role = null);
        Task<bool> DeleteUserAsync(long id);
    }
}
