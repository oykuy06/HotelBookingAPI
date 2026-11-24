using HotelBookingAPI.Constants;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

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

            //Regular users can only view their own information.
            if (!isAdmin && currentUserId != id.ToString())
                return Forbid();

            // Admins can only view users belonging to their own hotel.
            if (isAdmin && !isSuperAdmin)
            {
                if (!long.TryParse(currentUserId, out var adminUserId))
                    return Forbid();

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(adminUserId);

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

            // Admins can only view users belonging to their own hotel.
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

            // Only SuperAdmins can create Admin users.
            if (normalizedRole == UserRoles.Admin && !isSuperAdmin)
                return Forbid();

            // Admins can only create regular users within their own hotel.
            if (!isSuperAdmin)
            {
                if (normalizedRole != UserRoles.User)
                    return Forbid();

                var currentUserId = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (!long.TryParse(currentUserId, out var adminUserId))
                    return Forbid();

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(adminUserId);

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
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserDto dto)
        {
            var currentUserId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
            var isAdmin = User.IsInRole(UserRoles.Admin) || isSuperAdmin;

            // Regular users can only update their own accounts.
            if (!isAdmin && currentUserId != id.ToString())
                return Forbid();

            string? normalizedRole = null;

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                try
                {
                    normalizedRole = UserRoles.Normalize(dto.Role);
                }
                catch (ArgumentException)
                {
                    return BadRequest(new { message = "Invalid role" });
                }
            }

            // Admins can only update regular users belonging to their own hotel.
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

                if (targetUser.Role != UserRoles.User)
                    return Forbid();

                var targetHotelId = await _userService.GetHotelIdByUserIdAsync(id);

                if (targetHotelId != adminHotelId)
                    return Forbid();
            }

            // Rol ve otel atamasını yalnızca SuperAdmin değiştirebilir.
            var role = isSuperAdmin ? normalizedRole : null;
            var hotelId = isSuperAdmin && normalizedRole != null
                ? dto.HotelId
                : null;

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

            var currentUserId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(currentUserId, out var currentUserIdLong))
                return Forbid();

            // SuperAdmins cannot delete their own accounts.
            if (isSuperAdmin && currentUserIdLong == id)
                return Forbid();

            if (!isSuperAdmin)
            {
                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(
                    currentUserIdLong);

                if (!adminHotelId.HasValue)
                    return Forbid();

                var targetUser = await _userService.GetUserByIdAsync(id);

                if (targetUser == null)
                    return NotFound();

                // Admins cannot delete SuperAdmin or other Admin accounts.
                if (targetUser.Role == UserRoles.SuperAdmin ||
                    targetUser.Role == UserRoles.Admin)
                    return Forbid();

                var targetHotelId = await _userService.GetHotelIdByUserIdAsync(id);

                // Users can only be deleted if they belong to the admin's own hotel.
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
