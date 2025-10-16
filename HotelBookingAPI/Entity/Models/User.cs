using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Entity.Models
{
    public class User : BaseEntity
    {
        public long Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        public List<Reservation> Reservations { get; set; } = new(); //return empty list
    }
}
