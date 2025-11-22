using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Entity.Models
{
    public class Reservation : BaseEntity
    {
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }
        public virtual User User { get; set; } //Lazy Loading

        [Required]
        public long RoomId { get; set; }
        public virtual Room Room { get; set; } //Lazy Loading

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
    }
}
