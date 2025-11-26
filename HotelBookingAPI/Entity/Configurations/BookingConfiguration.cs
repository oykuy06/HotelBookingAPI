using HotelBookingAPI.Entity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingAPI.Entity.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            // Unique booking for same room & same date range
            builder.HasIndex(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate })
                   .IsUnique();

            builder.HasOne(b => b.User)
                   .WithMany(u => u.Reservations)
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
