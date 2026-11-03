using HotelBookingAPI.Dto.ResponseDto;
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

        public async Task<IEnumerable<RoomResponseDto>> GetAllRoomsAsync()
        {
            return await _context.Rooms
                .AsNoTracking()
                .Select(r => new RoomResponseDto
                {
                    Id = r.Id,
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
        }

        public async Task<RoomResponseDto?> GetRoomByIdAsync(long id)
        {
            return await _context.Rooms
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RoomResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    HotelId = r.HotelId,
                    HotelName = r.Hotel.Name,
                    RoomNumber = r.RoomNumber,
                    Capacity = r.Capacity,
                    Price = r.Price,
                    Description = r.Description,
                    Photo = r.Photo
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == room.HotelId);

            if (hotel == null)
                throw new ArgumentException("Hotel not found.");

            if (!string.IsNullOrEmpty(room.Photo) &&
                !Uri.IsWellFormedUriString(room.Photo, UriKind.Absolute))
            {
                throw new ArgumentException("Photo must be a valid URL.");
            }

            room.Hotel = hotel;

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return room;
        }


        public async Task<Room?> UpdateRoomAsync(long id, Room room)
        {
            var existingRoom = await _context.Rooms.FindAsync(id);

            if (existingRoom == null)
                return null;

            if (!string.IsNullOrEmpty(room.Photo) &&
                !Uri.IsWellFormedUriString(room.Photo, UriKind.Absolute))
            {
                throw new ArgumentException("Photo must be a valid URL.");
            }

            existingRoom.Name = room.Name;
            existingRoom.RoomNumber = room.RoomNumber;
            existingRoom.Capacity = room.Capacity;
            existingRoom.Price = room.Price;
            existingRoom.Description = room.Description;
            if (!string.IsNullOrWhiteSpace(room.Photo))
            {
                existingRoom.Photo = room.Photo;
            }

            await _context.SaveChangesAsync();

            return existingRoom;
        }


        public async Task<bool> DeleteRoomAsync(long id)
        {
            var room = await _context.Rooms
                .Include(r => r.Reservations)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return false;

            if (room.Reservations.Any())
                throw new InvalidOperationException(
                    "Room cannot be deleted because it has existing reservations.");

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
