using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Data
{
    public class HotelBookingContext : DbContext
    {
        public HotelBookingContext(DbContextOptions<HotelBookingContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<PasswordReset> PasswordReset { get; set; }
    }
}
