using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(long id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var dto = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(dto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,User")]
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
        public async Task<IActionResult> CreateUser([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userService.CreateUserAsync(dto.Email, dto.Password, dto.Role);

            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userService.UpdateUserAsync(id, dto.Email, dto.Password, dto.Role);
            if (user == null) return NotFound();

            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(response);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success) return NotFound();

            return Ok(new { message = "Deleted user" });
        }
    }
}
