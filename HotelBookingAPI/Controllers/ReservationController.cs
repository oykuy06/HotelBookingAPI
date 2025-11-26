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
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _reservationService.GetAllReservationsAsync();

            var response = reservations.Select(r => new ReservationResponseDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.UserName,
                RoomId = r.RoomId,
                RoomName = r.RoomName,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                TotalPrice = r.TotalPrice,
                Status = r.Status
            });

            return Ok(response);
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetReservationById(long id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            var dto = new ReservationResponseDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.UserName,
                RoomId = reservation.RoomId,
                RoomName = reservation.RoomName,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status
            };

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.CheckInDate >= dto.CheckOutDate)
                return BadRequest(new { message = "Check-in date must be before check-out date." });

            var reservation = new Reservation
            {
                RoomId = dto.RoomId,
                UserId = dto.UserId,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                TotalPrice = dto.TotalPrice,
                Status = dto.Status
            };

            var created = await _reservationService.CreateReservationAsync(reservation);

            var responseDto = new ReservationResponseDto
            {
                Id = created.Id,
                UserId = created.UserId,
                UserName = created.User?.Username ?? "",
                RoomId = created.RoomId,
                RoomName = created.Room?.Name ?? "",
                CheckInDate = created.CheckInDate,
                CheckOutDate = created.CheckOutDate,
                TotalPrice = created.TotalPrice,
                Status = created.Status
            };

            return CreatedAtAction(nameof(GetReservationById), new { id = created.Id }, responseDto);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateReservation(long id, [FromBody] ReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _reservationService.UpdateReservationAsync(id, new Reservation
            {
                RoomId = dto.RoomId,
                UserId = dto.UserId,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                TotalPrice = dto.TotalPrice,
                Status = dto.Status
            });

            if (updated == null)
                return NotFound(new { message = "Reservation not found" });

            return Ok(new { message = "Reservation updated successfully" });
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteReservation(long id)
        {
            var success = await _reservationService.DeleteReservationAsync(id);
            if (!success)
                return NotFound(new { message = "Reservation not found" });

            return Ok(new { message = "Reservation deleted successfully" });
        }
    }
}
