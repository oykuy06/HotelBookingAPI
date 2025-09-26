namespace HotelBookingAPI.Dto
{
    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string HotelName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
