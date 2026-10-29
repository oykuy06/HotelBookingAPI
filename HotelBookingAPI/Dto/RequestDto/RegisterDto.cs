using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Dto.RequestDto
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
    }
}
