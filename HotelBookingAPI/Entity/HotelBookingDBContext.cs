using HotelBookingAPI.Entity.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Entity
{
    public class HotelBookingDBContext : DbContext
    {
        public HotelBookingDBContext(DbContextOptions<HotelBookingDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
