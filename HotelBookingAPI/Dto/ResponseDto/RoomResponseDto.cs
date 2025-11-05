using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Dto.ResponseDto
{
    public class RoomResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long HotelId { get; set; }
        public string HotelName { get; set; }
        public int RoomNumber { get; set; }
        public int Capacity { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        [Url]
        public string? Photo { get; set; }
    }
}
