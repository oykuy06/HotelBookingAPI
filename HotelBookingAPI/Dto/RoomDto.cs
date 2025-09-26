using HotelBookingAPI.Data;

namespace HotelBookingAPI.Dto
{
    public class RoomDto
    {
        public string Name { get; set; }
        public int HotelId { get; set; }
        public int RoomNumber { get; set; }
        public int Capacity { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }
    }
}