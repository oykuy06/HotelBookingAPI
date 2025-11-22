using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Entity.Models
{
    public class Hotel : BaseEntity
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Address { get; set; }
        public string Photo { get; set; }

        public virtual List<Room> Rooms { get; set; } = new List<Room>(); //return empty list, lazy loading
    }
}
