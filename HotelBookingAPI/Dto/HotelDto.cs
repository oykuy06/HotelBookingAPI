
namespace HotelBookingAPI.Dto
{
    public class HotelDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Photo {  get; set; }
        public string Address { get; set; }
        public List<RoomResponseDto>? Rooms { get;  set; }
    }
}
