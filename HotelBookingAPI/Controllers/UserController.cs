
using HotelBookingAPI.Constants;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelBookingAPI.Entity;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.SuperAdmin + "," + UserRoles.User)]
        public async Task<IActionResult> GetUser(long id)
        {
            var currentUserId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
            var isAdmin = User.IsInRole(UserRoles.Admin) || isSuperAdmin;

            // Normal kullanıcı yalnızca kendi bilgilerini görebilir.
            if (!isAdmin && currentUserId != id.ToString())
                return Forbid();

            // Admin yalnızca kendi otelindeki kullanıcıları görebilir.
            if (isAdmin && !isSuperAdmin)
            {
                if (!long.TryParse(currentUserId, out var adminUserId))
                    return Forbid();

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(
                    adminUserId);

                if (!adminHotelId.HasValue)
                    return Forbid();

                var targetExists = await _userService.UserExistsAsync(id);

                if (!targetExists)
                    return NotFound();

                var targetHotelId = await _userService.GetHotelIdByUserIdAsync(id);

                if (targetHotelId != adminHotelId)
                    return Forbid();
            }

            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetAllUsers()
        {
            long? hotelId = null;

            // Admin yalnızca kendi otelindeki kullanıcıları görebilir.
            if (User.IsInRole(UserRoles.Admin) &&
                !User.IsInRole(UserRoles.SuperAdmin))
            {
                var currentUserId = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (!long.TryParse(currentUserId, out var adminUserId))
                    return Forbid();

                hotelId = await _userService.GetHotelIdByUserIdAsync(adminUserId);

                if (!hotelId.HasValue)
                    return Forbid();
            }

            var users = await _userService.GetAllUsersAsync(hotelId);

            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.SuperAdmin)]
        public async Task<IActionResult> CreateUser([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string normalizedRole;

            try
            {
                normalizedRole = UserRoles.Normalize(dto.Role);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "Invalid role" });
            }

            var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
            long? hotelId = dto.HotelId;

            // Yalnızca SuperAdmin Admin oluşturabilir.
            if (normalizedRole == UserRoles.Admin && !isSuperAdmin)
                return Forbid();

            // Admin yalnızca kendi otelinde normal kullanıcı oluşturabilir.
            if (!isSuperAdmin)
            {
                if (normalizedRole != UserRoles.User)
                    return Forbid();

                var currentUserId = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (!long.TryParse(currentUserId, out var adminUserId))
                    return Forbid();

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(
                    adminUserId);

                if (!adminHotelId.HasValue)
                    return Forbid();

                hotelId = adminHotelId;
            }

            try
            {
                var user = await _userService.CreateUserAsync(
                    dto.Email,
                    dto.Password,
                    normalizedRole,
                    hotelId);

                var response = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role
                };

                return CreatedAtAction(
                    nameof(GetUser),
                    new { id = user.Id },
                    response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.SuperAdmin + "," + UserRoles.User)]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UserDto dto)
        {
            var currentUserId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
            var isAdmin = User.IsInRole(UserRoles.Admin) || isSuperAdmin;

            // Normal kullanıcı yalnızca kendi hesabını güncelleyebilir.
            if (!isAdmin && currentUserId != id.ToString())
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string normalizedRole;

            try
            {
                normalizedRole = UserRoles.Normalize(dto.Role);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "Invalid role" });
            }

            // Admin yalnızca kendi otelindeki kullanıcıları güncelleyebilir.
            if (isAdmin && !isSuperAdmin)
            {
                if (!long.TryParse(currentUserId, out var adminUserId))
                    return Forbid();

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(
                    adminUserId);

                if (!adminHotelId.HasValue)
                    return Forbid();

                var targetUser = await _userService.GetUserByIdAsync(id);

                if (targetUser == null)
                    return NotFound();

                if (targetUser.Role != UserRoles.User) return Forbid();

                var targetHotelId = await _userService.GetHotelIdByUserIdAsync(id);

                if (targetHotelId != adminHotelId)
                    return Forbid();
            }

            // Yalnızca SuperAdmin rol ve otel atamasını değiştirebilir.
            var role = isSuperAdmin ? normalizedRole : null;
            var hotelId = isSuperAdmin ? dto.HotelId : null;

            User? user;

            try
            {
                user = await _userService.UpdateUserAsync(
                    id,
                    dto.Email,
                    dto.Password,
                    role,
                    hotelId);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

            if (user == null)
                return NotFound();

            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(response);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);

            if (!isSuperAdmin)
            {
                var currentUserId = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (!long.TryParse(currentUserId, out var adminUserId))
                    return Forbid();

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(
                    adminUserId);

                if (!adminHotelId.HasValue)
                    return Forbid();

                var targetUser = await _userService.GetUserByIdAsync(id);

                if (targetUser == null)
                    return NotFound();

                // Admin, SuperAdmin veya başka Admin hesaplarını silemez.
                if (targetUser.Role == UserRoles.SuperAdmin ||
                    targetUser.Role == UserRoles.Admin)
                    return Forbid();

                var targetHotelId = await _userService.GetHotelIdByUserIdAsync(id);

                // Yalnızca kendi otelindeki kullanıcıları silebilir.
                if (targetHotelId != adminHotelId)
                    return Forbid();
            }
            else
            {
                var exists = await _userService.UserExistsAsync(id);

                if (!exists)
                    return NotFound();
            }

            var success = await _userService.DeleteUserAsync(id);

            if (!success)
                return Conflict(new
                {
                    message = "User cannot be deleted because related records exist."
                });

            return Ok(new { message = "Deleted user" });
        }
    }
}
