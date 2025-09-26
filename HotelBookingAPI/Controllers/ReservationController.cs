using HotelBookingAPI.Data;
using HotelBookingAPI.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]

    [Authorize]
    public class ReservationController : ControllerBase
    {
        public readonly HotelBookingContext _context;

        public ReservationController(HotelBookingContext context)
        {
            _context = context;
        }

        [HttpGet("all")] //admin sees all reservations
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                .ThenInclude(room => room.Hotel)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    RoomId = r.Room.Id,
                    RoomName = r.Room.Name,
                    HotelName = r.Room.Hotel.Name,
                    UserId = r.UserId,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                })
                .ToListAsync();

            return Ok(reservations);
        }

        [HttpGet("me")] //User sees their own reservations
        public async Task<IActionResult> GetUserReservations()
        {
            var userId = int.Parse(User.Identity.Name);
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .ThenInclude(r => r.Hotel)
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    RoomId = r.RoomId,
                    RoomName = r.Room.Name,
                    HotelName = r.Room.Hotel.Name,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                })

                .ToListAsync();

            return Ok(reservations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                .ThenInclude(room => room.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            if (!User.IsInRole("Admin") && reservation.UserId != int.Parse(User.Identity.Name))
                return Forbid();

            var response = new ReservationResponseDto
            {
                Id = reservation.Id,
                RoomId = reservation.RoomId,
                RoomName = reservation.Room.Name,
                HotelName = reservation.Room.Hotel.Name,
                UserId = reservation.User.Id,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservations([FromBody] ReservationDto dto)
        {
            if (dto.StartDate >= dto.EndDate)
                return BadRequest("StartDate must be before EndDate");

            var room = await _context.Rooms.FindAsync(dto.RoomId);
            if (room == null) return NotFound("Room not found");

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound("User Not Found");

            bool isOverlapping = await _context.Reservations
                .AnyAsync(r => r.RoomId == room.Id &&
                        r.EndDate > dto.StartDate &&
                        r.StartDate < dto.EndDate);
            if (isOverlapping)
                return BadRequest("This room is already booked for the selected dates");

            var reservation = new Reservation
            {
                UserId = dto.UserId,
                RoomId = room.Id,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var response = new ReservationResponseDto
            {
                Id = reservation.Id,
                RoomId = reservation.RoomId,
                RoomName = room.Name,
                HotelName = reservation.Room.Hotel.Name,
                UserId = reservation.UserId,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
            };

            return Ok(response);
        }
    }
}
           
