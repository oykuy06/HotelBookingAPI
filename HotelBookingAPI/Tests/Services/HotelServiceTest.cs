using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelBookingAPI.Tests.Services
{
    public class HotelServiceTest
    {
        private HotelBookingDBContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<HotelBookingDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new HotelBookingDBContext(options);
        }

        private HotelService GetService(HotelBookingDBContext context)
        {
            return new HotelService(context);
        }

        [Fact]
        public async Task CreateHotelAsync_ShouldCreateHotel()
        {
            // Arrange
            var context = GetDbContext();
            var service = GetService(context);

            var hotel = new Hotel
            {
                Name = "Test Hotel",
                Description = "Test Description",
                Address = "Istanbul",
                Photo = "photo.jpg"
            };

            // Act
            var result = await service.CreateHotelAsync(hotel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Hotel", result.Name);
            Assert.Equal(1, context.Hotels.Count());
        }

        [Fact]
        public async Task GetHotelByIdAsync_ShouldReturnHotel()
        {
            // Arrange
            var context = GetDbContext();

            var hotel = new Hotel
            {
                Name = "Hotel One",
                Address = "London"
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.GetHotelByIdAsync(hotel.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hotel One", result.Name);
        }

        [Fact]
        public async Task GetHotelByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var context = GetDbContext();
            var service = GetService(context);

            // Act
            var result = await service.GetHotelByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllHotelsAsync_ShouldReturnAllHotels()
        {
            // Arrange
            var context = GetDbContext();

            context.Hotels.AddRange(
                new Hotel { Name = "Hotel A" },
                new Hotel { Name = "Hotel B" }
            );

            await context.SaveChangesAsync();
            var service = GetService(context);

            // Act
            var result = await service.GetAllHotelsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateHotelAsync_ShouldUpdateHotel()
        {
            // Arrange
            var context = GetDbContext();

            var hotel = new Hotel
            {
                Name = "Old Name",
                Address = "Old Address"
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var updatedHotel = new Hotel
            {
                Name = "New Name",
                Address = "New Address"
            };

            // Act
            var result = await service.UpdateHotelAsync(hotel.Id, updatedHotel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
        }

        [Fact]
        public async Task DeleteHotelAsync_ShouldDeleteHotel()
        {
            // Arrange
            var context = GetDbContext();

            var hotel = new Hotel { Name = "Delete Me" };
            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.DeleteHotelAsync(hotel.Id);

            // Assert
            Assert.True(result);
            Assert.Empty(context.Hotels);
        }
    }
}
