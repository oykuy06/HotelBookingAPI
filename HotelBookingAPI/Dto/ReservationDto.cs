using HotelBookingAPI.Data;

namespace HotelBookingAPI.Dto
{
    public class ReservationDto
    {
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}