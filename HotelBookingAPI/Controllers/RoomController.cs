using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBookingAPI.Dto.ResponseDto;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class RoomController : ControllerBase
    {
        private readonly HotelBookingDBContext _context;

        public RoomController(HotelBookingDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Hotel)
                .Select(r => new RoomResponseDto
                {
                    Id = (int)r.Id,
                    Name = r.Name,
                    HotelId = r.HotelId,
                    HotelName = r.Hotel.Name,
                    RoomNumber = r.RoomNumber,
                    Capacity = r.Capacity,
                    Price = r.Price,
                    Description = r.Description,
                    Photo = r.Photo
                })
                .ToListAsync();

            return Ok(rooms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            var response = new RoomResponseDto
            {
                Id = (int)room.Id,
                Name = room.Name,
                HotelId = room.HotelId,
                HotelName = room.Hotel.Name,
                RoomNumber = room.RoomNumber,
                Capacity = room.Capacity,
                Price = room.Price,
                Description = room.Description,
                Photo = room.Photo
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] RoomDto dto)
        {
            var room = new Room
            {
                Name = dto.Name,
                HotelId = dto.HotelId,
                RoomNumber = dto.RoomNumber,
                Capacity = dto.Capacity,
                Price = dto.Price,
                Description = dto.Description,
                Photo = dto.Photo
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Created a new room", room.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] RoomDto dto)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            room.Name = dto.Name;
            room.HotelId = dto.HotelId;
            room.RoomNumber = dto.RoomNumber;
            room.Capacity = dto.Capacity;
            room.Price = dto.Price;
            room.Description = dto.Description;
            room.Photo = dto.Photo;

            await _context.SaveChangesAsync();
            return Ok(room);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deleted room" });
        }
    }

}

