using HotelBookingAPI.Dto.RequestDto;
using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Dto.RequestDto
{
    public class RoomDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        public long HotelId { get; set; }
        [Required]
        public int RoomNumber { get; set; }
        [Required]
        [Range(1, 10)]
        public int Capacity { get; set; }
        [Required]
        public double Price { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        public string? Photo { get; set; }
    }
}