using HotelBookingAPI.Entity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingAPI.Entity.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            // Reservation - Room
            builder.HasOne(b => b.Room)
                   .WithMany(r => r.Reservations)
                   .HasForeignKey(b => b.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}