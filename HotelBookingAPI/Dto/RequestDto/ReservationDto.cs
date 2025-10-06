using HotelBookingAPI.Dto.RequestDto;
using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Dto.RequestDto
{
    public class ReservationDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}