
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly IUserService _userService;

        public ReservationController(IReservationService reservationService, IUserService userService)
        {
            _reservationService = reservationService;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _reservationService.GetAllReservationsAsync();

            if (User.IsInRole("Admin"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!long.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(userId);

                if (adminHotelId == null)
                    return Forbid();

                reservations = reservations
                    .Where(r => r.HotelId == adminHotelId.Value)
                    .ToList();
            }

            return Ok(reservations);
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = "User,Admin,SuperAdmin")]
        public async Task<IActionResult> GetReservationById(long id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (User.IsInRole("Admin"))
            {
                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(userId);

                if (adminHotelId == null || reservation.HotelId != adminHotelId.Value)
                    return Forbid();
            }
            else if (User.IsInRole("User") && reservation.UserId != userId)
            {
                return Forbid();
            }

            return Ok(reservation);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,SuperAdmin")]
        public async Task<IActionResult> CreateReservation(
            [FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var reservation = new Reservation
            {
                UserId = userId,
                RoomId = dto.RoomId,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                TotalPrice = 0,
                Status = ReservationStatus.Pending
            };

            Reservation created;

            try
            {
                created = await _reservationService.CreateReservationAsync(reservation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

            var responseDto =
                await _reservationService.GetReservationByIdAsync(created.Id);

            return CreatedAtAction(
                nameof(GetReservationById),
                new { id = created.Id },
                responseDto);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "User,Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateReservation(
            long id,
            [FromBody] UpdateReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var existingReservation =
                await _reservationService.GetReservationByIdAsync(id);

            if (existingReservation == null)
                return NotFound(new { message = "Reservation not found" });

            if (User.IsInRole("Admin"))
            {
                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(userId);

                if (adminHotelId == null || existingReservation.HotelId != adminHotelId.Value)
                    return Forbid();
            }
            else if (User.IsInRole("User") && existingReservation.UserId != userId)
            {
                return Forbid();
            }

            Reservation? updated;

            try
            {
                updated = await _reservationService.UpdateReservationAsync(
                    id,
                    new Reservation
                    {
                        RoomId = dto.RoomId,
                        CheckInDate = dto.CheckInDate,
                        CheckOutDate = dto.CheckOutDate
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

            if (updated == null)
                return NotFound(new { message = "Reservation not found" });

            return Ok(new { message = "Reservation updated successfully" });
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "User,Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteReservation(long id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (User.IsInRole("Admin"))
            {
                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(userId);

                if (adminHotelId == null || reservation.HotelId != adminHotelId.Value)
                    return Forbid();
            }
            else if (User.IsInRole("User") && reservation.UserId != userId)
            {
                return Forbid();
            }

            var success = await _reservationService.DeleteReservationAsync(id);

            if (!success)
                return NotFound(new { message = "Reservation not found" });

            return Ok(new { message = "Reservation cancelled successfully" });
        }
    }
}
