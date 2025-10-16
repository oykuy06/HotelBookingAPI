using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IHotelService
    {
        Task<Hotel?> GetHotelByIdAsync(long id);
        Task<IEnumerable<Hotel>> GetAllHotelsAsync();
        Task<Hotel> CreateHotelAsync(Hotel hotel);
        Task<Hotel?> UpdateHotelAsync(long id, Hotel hotel);
        Task<bool> DeleteHotelAsync(long id);
    }
}
