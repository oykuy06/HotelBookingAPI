using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Entity.Models
{
    public class PasswordReset : BaseEntity
    {
        public long Id { get; set; }
        [Required]
        public long UserId { get; set; }
        public User User { get; set; }

        [Required]
        [StringLength(100)]
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; }
    }
}
