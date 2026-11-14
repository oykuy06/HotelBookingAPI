using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync();
        Task<IEnumerable<ReservationResponseDto>> GetReservationsByHotelIdAsync(long hotelId);
        Task<ReservationResponseDto?> GetReservationByIdAsync(long id);
        Task<Reservation> CreateReservationAsync(
    Reservation reservation,
    long? allowedHotelId = null);
        Task<Reservation?> UpdateReservationAsync(
    long id,
    Reservation reservation,
    long? allowedHotelId = null);
        Task<bool> DeleteReservationAsync(long id);
    }
}
