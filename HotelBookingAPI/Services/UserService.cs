using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services
{
    public class UserService : IUserService
    {
        private readonly HotelBookingDBContext _context;
        private readonly IPasswordHashService _passwordHashService;

        public UserService(
            HotelBookingDBContext context,
            IPasswordHashService passwordHashService)
        {
            _context = context;
            _passwordHashService = passwordHashService;
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(long id)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = u.Role,
                    Reservations = u.Reservations.Select(r => new ReservationResponseDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        UserName = u.Username,
                        RoomId = r.RoomId,
                        RoomName = r.Room.Name,
                        CheckInDate = r.CheckInDate,
                        CheckOutDate = r.CheckOutDate,
                        TotalPrice = (decimal)r.TotalPrice,
                        Status = r.Status
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = u.Role,
                    Reservations = u.Reservations.Select(r => new ReservationResponseDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        UserName = u.Username,
                        RoomId = r.RoomId,
                        RoomName = r.Room.Name,
                        CheckInDate = r.CheckInDate,
                        CheckOutDate = r.CheckOutDate,
                        TotalPrice = (decimal)r.TotalPrice,
                        Status = r.Status
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<User> CreateUserAsync(
            string email,
            string password,
            string role = "User")
        {
            var user = new User
            {
                Email = email,
                Username = email.Split('@')[0],
                PasswordHash = _passwordHashService.HashPassword(password),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> UpdateUserAsync(
            long id,
            string email,
            string? password = null,
            string? role = null)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return null;

            user.Email = email;
            user.Username = email.Split('@')[0];

            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHash = _passwordHashService.HashPassword(password);
            }

            if (!string.IsNullOrEmpty(role))
            {
                user.Role = role;
            }

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<long?> GetHotelIdByUserIdAsync(long userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.HotelId)
                .FirstOrDefaultAsync();
        }
    }
}