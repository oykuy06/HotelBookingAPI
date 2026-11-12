using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto?> GetUserByIdAsync(long id);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(long? hotelId);
        Task<User> CreateUserAsync(string email, string password, string role = "User", long? hotelId = null);
        Task<User?> UpdateUserAsync(
    long id,
    string email,
    string? password = null,
    string? role = null,
    long? hotelId = null);
        Task<bool> DeleteUserAsync(long id);
        Task<long?> GetHotelIdByUserIdAsync(long userId);
        Task<bool> UserExistsAsync(long id);
    }
}
