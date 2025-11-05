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

        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
        {
            return await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<Reservation?> GetReservationByIdAsync(long id)
        {
            return await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == reservation.UserId);
            if (!userExists)
                throw new ArgumentException("User not found");

            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == reservation.RoomId);
            if (!roomExists)
                throw new ArgumentException("Room not found");

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<Reservation?> UpdateReservationAsync(long id, Reservation updatedReservation)
        {
            var existing = await _context.Reservations.FindAsync(id);
            if (existing == null)
                return null;

            var userExists = await _context.Users.AnyAsync(u => u.Id == updatedReservation.UserId);
            if (!userExists) throw new ArgumentException("User not found");

            existing.RoomId = updatedReservation.RoomId;
            existing.UserId = updatedReservation.UserId;
            existing.CheckInDate = updatedReservation.CheckInDate;
            existing.CheckOutDate = updatedReservation.CheckOutDate;
            existing.TotalPrice = updatedReservation.TotalPrice;
            existing.Status = updatedReservation.Status;

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
