using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
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
        public async Task<IActionResult> GetAllHotels()
        {
            var hotels = await _hotelService.GetAllHotelsAsync();
            var hotelDtos = hotels.Select(h => new HotelDto
            {
                Name = h.Name,
                Description = h.Description,
                Address = h.Address,
                Photo = h.Photo
            });

            return Ok(hotelDtos);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetHotelById(long id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);
            if (hotel == null)
                return NotFound(new { message = "Hotel not found" });

            var dto = new HotelDto
            {
                Name = hotel.Name,
                Description = hotel.Description,
                Address = hotel.Address,
                Photo = hotel.Photo
            };

            return Ok(dto);
        }

        [HttpPost]
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
            var responseDto = new HotelDto
            {
                Name = createdHotel.Name,
                Description = createdHotel.Description,
                Address = createdHotel.Address,
                Photo = createdHotel.Photo
            };

            return CreatedAtAction(nameof(GetHotelById), new { id = createdHotel.Id }, responseDto);
        }

        [HttpPut("{id:long}")]
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
        public async Task<IActionResult> DeleteHotel(long id)
        {
            var success = await _hotelService.DeleteHotelAsync(id);
            if (!success)
                return NotFound(new { message = "Hotel not found" });

            return Ok(new { message = "Hotel deleted successfully" });
        }
    }
}
