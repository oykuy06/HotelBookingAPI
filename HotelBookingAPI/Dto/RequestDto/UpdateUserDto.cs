using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Dto.RequestDto
{
    public class UpdateUserDto
    {
        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        [StringLength(20)]
        public string? Role { get; set; }

        public long? HotelId { get; set; }
    }
}