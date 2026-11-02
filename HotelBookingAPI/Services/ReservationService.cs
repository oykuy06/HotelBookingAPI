using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services
{
    public class ReservationService : IReservationService
    {
        private readonly HotelBookingDBContext _context;

        public ReservationService(HotelBookingDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync()
        {
            return await _context.Reservations
                .AsNoTracking()
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.Username,
                    RoomId = r.RoomId,
                    RoomName = r.Room.Name,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalPrice = (decimal)r.TotalPrice,
                    Status = r.Status
                })
                .ToListAsync();
        }

        public async Task<ReservationResponseDto?> GetReservationByIdAsync(long id)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.Username,
                    RoomId = r.RoomId,
                    RoomName = r.Room.Name,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalPrice = (decimal)r.TotalPrice,
                    Status = r.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == reservation.UserId);

            if (!userExists)
                throw new ArgumentException("User not found");

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == reservation.RoomId);

            if (room == null)
                throw new ArgumentException("Room not found");

            if (reservation.CheckInDate >= reservation.CheckOutDate)
                throw new ArgumentException("Check-out date must be after check-in date.");

            var turkeyTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            DateTime.UtcNow,
            "Turkey Standard Time");

            if (reservation.CheckInDate.Date < turkeyTime.Date)
                throw new ArgumentException("Check-in date cannot be in the past.");

            var numberOfNights = (reservation.CheckOutDate - reservation.CheckInDate).Days;

            reservation.TotalPrice = room.Price * numberOfNights;

            var hasConflict = await _context.Reservations
                .AnyAsync(r =>
                    r.RoomId == reservation.RoomId &&
                    r.Status != "Cancelled" &&
                    reservation.CheckInDate < r.CheckOutDate &&
                    reservation.CheckOutDate > r.CheckInDate);

            if (hasConflict)
                throw new InvalidOperationException(
                    "Room is already reserved for the selected dates.");

            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<Reservation?> UpdateReservationAsync(
    long id,
    Reservation updatedReservation)
        {
            var existing = await _context.Reservations.FindAsync(id);

            if (existing == null)
                return null;

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == updatedReservation.RoomId);

            if (room == null)
                throw new ArgumentException("Room not found");

            if (updatedReservation.CheckInDate >= updatedReservation.CheckOutDate)
                throw new ArgumentException("Check-out date must be after check-in date.");

            var turkeyTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "Turkey Standard Time");

            if (updatedReservation.CheckInDate.Date < turkeyTime.Date)
                throw new ArgumentException("Check-in date cannot be in the past.");

            var numberOfNights =
                (updatedReservation.CheckOutDate - updatedReservation.CheckInDate).Days;

            var hasConflict = await _context.Reservations
                .AnyAsync(r =>
                    r.Id != id &&
                    r.RoomId == updatedReservation.RoomId &&
                    r.Status != "Cancelled" &&
                    updatedReservation.CheckInDate < r.CheckOutDate &&
                    updatedReservation.CheckOutDate > r.CheckInDate);

            if (hasConflict)
                throw new InvalidOperationException(
                    "Room is already reserved for the selected dates.");

            existing.RoomId = updatedReservation.RoomId;
            existing.CheckInDate = updatedReservation.CheckInDate;
            existing.CheckOutDate = updatedReservation.CheckOutDate;
            existing.TotalPrice = room.Price * numberOfNights;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteReservationAsync(long id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return false;

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
