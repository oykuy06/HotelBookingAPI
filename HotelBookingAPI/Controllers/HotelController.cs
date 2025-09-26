using HotelBookingAPI.Data;
using HotelBookingAPI.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class HotelController : ControllerBase
    {
        private readonly HotelBookingContext _context;

        public HotelController(HotelBookingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllHotel()
        {
            var hotels = await _context.Hotels.ToListAsync();
            return Ok(hotels);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotel(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hotel == null)
                return NotFound();

            var hotelDto = new HotelDto
            {
                Name = hotel.Name,
                Description = hotel.Description,
                Address = hotel.Address,
                Photo = hotel.Photo,
                Rooms = hotel.Rooms.Select(r => new RoomResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    HotelId = r.HotelId,
                    RoomNumber = r.RoomNumber,
                    Capacity = r.Capacity,
                    Price = r.Price,
                    Description = r.Description,
                    Photo = r.Photo
                }).ToList()
            };

            return Ok(hotelDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHotel([FromBody] HotelDto dto)
        {
            var hotel = new Hotel
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                Photo = dto.Photo,
                Rooms = new List<Room>()
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Created a new hotel"});
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] HotelDto dto)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if(hotel == null) return NotFound();

            hotel.Name = dto.Name;
            hotel.Description = dto.Description;
            hotel.Address = dto.Address;
            hotel.Photo = dto.Photo;

            await _context.SaveChangesAsync();
            return Ok(hotel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if(hotel == null) return NotFound();

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deleted hotel" });
        }
    }
}
      
