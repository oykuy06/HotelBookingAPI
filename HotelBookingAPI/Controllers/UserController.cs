
using HotelBookingAPI.Constants;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            var isAdmin = User.IsInRole(UserRoles.Admin) ||
                          User.IsInRole(UserRoles.SuperAdmin);

            if (!isAdmin && currentUserId != id.ToString())
                return Forbid();

            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            var dtoList = users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role
            });

            return Ok(dtoList);
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

            if (normalizedRole == UserRoles.SuperAdmin &&
                !User.IsInRole(UserRoles.SuperAdmin))
            {
                return Forbid();
            }

            var user = await _userService.CreateUserAsync(
                dto.Email, dto.Password, normalizedRole);

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

            // Admin, SuperAdmin rolü atayamaz.
            if (normalizedRole == UserRoles.SuperAdmin && !isSuperAdmin)
                return Forbid();

            // Admin, mevcut bir SuperAdmin hesabını değiştiremez.
            if (isAdmin && !isSuperAdmin)
            {
                var targetUser = await _userService.GetUserByIdAsync(id);

                if (targetUser == null)
                    return NotFound();

                if (targetUser.Role == UserRoles.SuperAdmin)
                    return Forbid();
            }

            // Normal kullanıcı kendi rolünü değiştiremez.
            var role = isAdmin ? normalizedRole : null;

            var user = await _userService.UpdateUserAsync(
                id, dto.Email, dto.Password, role);

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
            var success = await _userService.DeleteUserAsync(id);

            if (!success)
                return NotFound();

            return Ok(new { message = "Deleted user" });
        }
    }
}
