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
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;

        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllHotels()
        {
            var hotels = await _hotelService.GetAllHotelsAsync();
            var hotelDtos = hotels.Select(h => new HotelResponseDto
            {
                Id = h.Id,
                Name = h.Name,
                Description = h.Description,
                Address = h.Address,
                Photo = h.Photo,
                Rooms = h.Rooms.Select(r => new RoomResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    HotelId = r.HotelId,
                    HotelName = h.Name,
                    RoomNumber = r.RoomNumber,
                    Capacity = r.Capacity,
                    Price = r.Price,
                    Description = r.Description,
                    Photo = r.Photo
                }).ToList()
            });

            return Ok(hotelDtos);
        }

        [HttpGet("{id:long}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHotelById(long id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);
            if (hotel == null)
                return NotFound(new { message = "Hotel not found" });

            var dto = new HotelResponseDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Address = hotel.Address,
                Photo = hotel.Photo,
                Rooms = hotel.Rooms.Select(r => new RoomResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    HotelId = r.HotelId,
                    HotelName = hotel.Name,
                    RoomNumber = r.RoomNumber,
                    Capacity = r.Capacity,
                    Price = r.Price,
                    Description = r.Description,
                    Photo = r.Photo
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateHotel([FromBody] HotelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hotel = new Hotel
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                Photo = dto.Photo
            };

            var createdHotel = await _hotelService.CreateHotelAsync(hotel);
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHotel(long id, [FromBody] HotelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedHotel = await _hotelService.UpdateHotelAsync(id, new Hotel
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                Photo = dto.Photo
            });

            if (updatedHotel == null)
                return NotFound(new { message = "Hotel not found" });

            return Ok(new { message = "Hotel updated successfully" });
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHotel(long id)
        {
            var success = await _hotelService.DeleteHotelAsync(id);
            if (!success)
                return NotFound(new { message = "Hotel not found" });

            return Ok(new { message = "Hotel deleted successfully" });
        }
    }
}
