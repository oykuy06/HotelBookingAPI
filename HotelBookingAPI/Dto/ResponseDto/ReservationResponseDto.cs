namespace HotelBookingAPI.Dto.ResponseDto
{
    public class ReservationResponseDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long RoomId { get; set; }
        public string RoomName { get; set; }
        public string HotelName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
