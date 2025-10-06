using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Entity.Models
{
    public class Reservation : BaseEntity
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }

        [Required]
        public long RoomId { get; set; }
        public Room Room { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
