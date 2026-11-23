using HotelBookingAPI.Constants;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly IUserService _userService;

        public HotelController(
            IHotelService hotelService,
            IUserService userService)
        {
            _hotelService = hotelService;
            _userService = userService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllHotels()
        {
            var hotels = await _hotelService.GetAllHotelsAsync();

            return Ok(hotels);
        }

        [HttpGet("{id:long}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHotelById(long id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);

            if (hotel == null)
                return NotFound(new { message = "Hotel not found" });

            return Ok(hotel);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> CreateHotel([FromBody] HotelDto dto)
        {
            var hotel = new Hotel
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                Photo = dto.Photo
            };

            Hotel createdHotel;

            try
            {
                createdHotel = await _hotelService.CreateHotelAsync(hotel);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

            var responseDto = new HotelResponseDto
            {
                Id = createdHotel.Id,
                Name = createdHotel.Name,
                Description = createdHotel.Description,
                Address = createdHotel.Address,
                Photo = createdHotel.Photo,
                Rooms = new List<RoomResponseDto>()
            };

            return CreatedAtAction(nameof(GetHotelById), new { id = createdHotel.Id }, responseDto);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.SuperAdmin)]
        public async Task<IActionResult> UpdateHotel(long id, [FromBody] HotelDto dto)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);

            if (hotel == null)
                return NotFound(new { message = "Hotel not found" });

            if (User.IsInRole(UserRoles.Admin))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null ||
                    !long.TryParse(userIdClaim.Value, out var userId))
                {
                    return Forbid();
                }

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(userId);

                if (!adminHotelId.HasValue || hotel.Id != adminHotelId.Value)
                    return Forbid();
            }

            Hotel? updatedHotel;

            try
            {
                updatedHotel = await _hotelService.UpdateHotelAsync(id, new Hotel
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Address = dto.Address,
                    Photo = dto.Photo
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

            if (updatedHotel == null)
                return NotFound(new { message = "Hotel not found" });

            return Ok(new { message = "Hotel updated successfully" });
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteHotel(long id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);

            if (hotel == null)
                return NotFound(new { message = "Hotel not found" });

            try
            {
                var success = await _hotelService.DeleteHotelAsync(id);

                if (!success)
                    return NotFound(new { message = "Hotel not found" });

                return Ok(new { message = "Hotel deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

        }

    }
}
