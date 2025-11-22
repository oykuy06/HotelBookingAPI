using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services
{
    public class HotelService : IHotelService
    {
        private readonly HotelBookingDBContext _context;

        public HotelService(HotelBookingDBContext context)
        {
            _context = context;
        }

        public async Task<HotelResponseDto?> GetHotelByIdAsync(long id)
        {
            return await _context.Hotels
                .AsNoTracking()
                .Where(h => h.Id == id)
                .Select(h => new HotelResponseDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Description = h.Description,
                    Address = h.Address,
                    Photo = h.Photo,
                    Rooms = h.Rooms.Select(r => new RoomResponseDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        HotelId = h.Id,
                        HotelName = h.Name,
                        RoomNumber = r.RoomNumber,
                        Capacity = r.Capacity,
                        Price = r.Price,
                        Description = r.Description,
                        Photo = r.Photo
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<HotelResponseDto>> GetAllHotelsAsync()
        {
            return await _context.Hotels
                .AsNoTracking()
                .Select(h => new HotelResponseDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Description = h.Description,
                    Address = h.Address,
                    Photo = h.Photo,
                    Rooms = h.Rooms.Select(r => new RoomResponseDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        HotelId = h.Id,
                        HotelName = h.Name,
                        RoomNumber = r.RoomNumber,
                        Capacity = r.Capacity,
                        Price = r.Price,
                        Description = r.Description,
                        Photo = r.Photo
                    }).ToList()
                })
                .ToListAsync();
        }


        public async Task<Hotel> CreateHotelAsync(Hotel hotel)
        {
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }

        public async Task<Hotel?> UpdateHotelAsync(long id, Hotel hotel)
        {
            var existingHotel = await _context.Hotels.FindAsync(id);
            if (existingHotel == null)
                return null;

            existingHotel.Name = hotel.Name;
            existingHotel.Description = hotel.Description;
            existingHotel.Address = hotel.Address;
            existingHotel.Photo = hotel.Photo;

            await _context.SaveChangesAsync();
            return existingHotel;
        }

        public async Task<bool> DeleteHotelAsync(long id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
                return false;

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
