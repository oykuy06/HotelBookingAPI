using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Dto.RequestDto
{
    public class UpdateReservationDto
    {
        [Range(1, long.MaxValue)]
        public long RoomId { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }
    }
}