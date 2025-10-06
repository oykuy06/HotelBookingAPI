using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Dto.RequestDto
{
    public class ResetPasswordDto
    {
        public string Token { get; set; }

        [Required]
        [MinLength(3)]
        public string NewPassword { get; set; }
    }
}
