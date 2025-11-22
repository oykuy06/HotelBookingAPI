using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync();
        Task<ReservationResponseDto?> GetReservationByIdAsync(long id);
        Task<Reservation> CreateReservationAsync(Reservation reservation);
        Task<Reservation?> UpdateReservationAsync(long id, Reservation reservation);
        Task<bool> DeleteReservationAsync(long id);
    }
}
