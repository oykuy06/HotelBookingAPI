using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Entity.Models
{
    public class Room : BaseEntity
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }
        public string Photo { get; set; }
        [Required]
        public int RoomNumber { get; set; }
        [Required]
        [Range(1, 5)]
        public int Capacity { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public long HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public List<Reservation> Reservations { get; set; }
    }
}
