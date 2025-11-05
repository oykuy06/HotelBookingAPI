using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services
{
    public class RoomService : IRoomService
    {
        private readonly HotelBookingDBContext _context;

        public RoomService(HotelBookingDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _context.Rooms
                .Include(r => r.Hotel)
                .ToListAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(long id)
        {
            return await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            if (!string.IsNullOrEmpty(room.Photo) && !Uri.IsWellFormedUriString(room.Photo, UriKind.Absolute))
                throw new ArgumentException("Photo must be a valid URL.");

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<Room?> UpdateRoomAsync(long id, Room room)
        {
            var existingRoom = await _context.Rooms.FindAsync(id);
            if (existingRoom == null)
                return null;

            var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == room.HotelId);
            if (!hotelExists)
                throw new Exception("Hotel not found");

            if (!string.IsNullOrEmpty(room.Photo) && !Uri.IsWellFormedUriString(room.Photo, UriKind.Absolute))
                throw new ArgumentException("Photo must be a valid URL.");

            existingRoom.Name = room.Name;
            existingRoom.HotelId = room.HotelId;
            existingRoom.RoomNumber = room.RoomNumber;
            existingRoom.Capacity = room.Capacity;
            existingRoom.Price = room.Price;
            existingRoom.Description = room.Description;
            existingRoom.Photo = room.Photo;

            await _context.SaveChangesAsync();
            return existingRoom;
        }

        public async Task<bool> DeleteRoomAsync(long id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
