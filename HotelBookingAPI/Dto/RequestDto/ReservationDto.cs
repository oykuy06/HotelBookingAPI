using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Dto.RequestDto
{
    public class ReservationDto
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public long RoomId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";
    }
}
