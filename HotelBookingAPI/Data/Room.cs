namespace HotelBookingAPI.Data
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; }
        public string Photo { get; set; }
        public int RoomNumber { get; set; }
        public int Capacity { get; set; }
        public double Price { get; set; }
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public List<Reservation> Reservations { get; set; }
    }
}
