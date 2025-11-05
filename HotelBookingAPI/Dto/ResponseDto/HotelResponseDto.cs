namespace HotelBookingAPI.Dto.ResponseDto
{
    public class HotelResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Photo { get; set; }

        public List<RoomResponseDto> Rooms { get; set; } = new();
    }
}
