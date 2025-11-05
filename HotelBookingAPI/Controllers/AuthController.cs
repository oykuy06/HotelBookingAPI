using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserDto dto)
        {
            var user = await _authService.RegisterAsync(dto);
            if (user == null)
                return BadRequest(new { message = "Email already registered" });

            return Ok(new { message = "User created successfully", user.Id });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            if (token == null) return Unauthorized(new { message = "Invalid credentials" });

            return Ok(new { token });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var token = await _authService.ForgotPasswordAsync(dto.Email);
            if (token == null) return NotFound(new { message = "User not found" });

            return Ok(new { message = "Reset link sent to email" });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var success = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!success) return BadRequest(new { message = "Invalid or expired token" });

            return Ok(new { message = "Password has been reset successfully" });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var newAccessToken = await _authService.RefreshAccessTokenAsync(refreshToken);

            if (newAccessToken == null)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            return Ok(new { accessToken = newAccessToken });
        }
    }
}
