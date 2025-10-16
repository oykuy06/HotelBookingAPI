using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> GetAllReservationsAsync();
        Task<Reservation?> GetReservationByIdAsync(long id);
        Task<Reservation> CreateReservationAsync(Reservation reservation);
        Task<Reservation?> UpdateReservationAsync(long id, Reservation reservation);
        Task<bool> DeleteReservationAsync(long id);
    }
}
