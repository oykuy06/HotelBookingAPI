using HotelBookingAPI.Entity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingAPI.Entity.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            // Unique roomNumber
            builder.HasIndex(r => new { r.HotelId, r.RoomNumber })
                   .IsUnique();

            builder.HasMany(r => r.Reservations)
                   .WithOne(b => b.Room)
                   .HasForeignKey(b => b.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
