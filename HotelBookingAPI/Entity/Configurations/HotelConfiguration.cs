using HotelBookingAPI.Entity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingAPI.Entity.Configurations
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            // Unique: HotelName + City
            builder.HasIndex(h => new { h.Name })
                   .IsUnique();

            builder.HasMany(h => h.Rooms)
                   .WithOne(r => r.Hotel)
                   .HasForeignKey(r => r.HotelId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
