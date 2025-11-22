using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBookingAPI.Entity.Models
{
    public class RefreshToken
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public DateTime Expiration { get; set; }

        public bool IsRevoked { get; set; } = false;
    }
}
