using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using HotelBookingAPI.Constants;

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

        public async Task<User> CreateUserAsync(string email, string password, string role = UserRoles.User)
        {

            email = email.Trim().ToLowerInvariant();

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                Email = email,
                Username = email.Split('@')[0],
                PasswordHash = _passwordHashService.HashPassword(password),
                Role = UserRoles.Normalize(role)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> UpdateUserAsync(long id, string email, string? password = null, string? role = null)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return null;

            email = email.Trim().ToLowerInvariant();

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == email && u.Id != id);

            if (emailExists)
                throw new InvalidOperationException("Email is already registered.");

            user.Email = email;
            user.Username = email.Split('@')[0];

            if (!string.IsNullOrWhiteSpace(password))
            {
                user.PasswordHash = _passwordHashService.HashPassword(password);

                var activeRefreshTokens = await _context.RefreshTokens
                    .Where(t => t.UserId == user.Id && !t.IsRevoked)
                    .ToListAsync();

                foreach (var refreshToken in activeRefreshTokens)
                {
                    refreshToken.IsRevoked = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                user.Role = UserRoles.Normalize(role);
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

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                _context.Entry(user).State = EntityState.Unchanged;
                return false;
            }
        }

        public async Task<bool> UserExistsAsync(long id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
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