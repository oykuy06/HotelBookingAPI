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
                Photo = "http://example.com/photo.jpg"
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
                Description = "Nice hotel",
                Address = "London",
                Photo = "http://example.com/photo1.jpg"
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
                new Hotel
                {
                    Name = "Hotel A",
                    Description = "Description A",
                    Address = "City A",
                    Photo = "http://example.com/a.jpg"
                },
                new Hotel
                {
                    Name = "Hotel B",
                    Description = "Description B",
                    Address = "City B",
                    Photo = "http://example.com/b.jpg"
                }
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
                Description = "Old Desc",
                Address = "Old Address",
                Photo = "http://example.com/old.jpg"
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var updatedHotel = new Hotel
            {
                Name = "New Name",
                Description = "New Desc",
                Address = "New Address",
                Photo = "http://example.com/new.jpg"
            };

            // Act
            var result = await service.UpdateHotelAsync(hotel.Id, updatedHotel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            Assert.Equal("New Address", result.Address);
        }

        [Fact]
        public async Task DeleteHotelAsync_ShouldDeleteHotel()
        {
            // Arrange
            var context = GetDbContext();

            var hotel = new Hotel
            {
                Name = "Delete Me",
                Description = "To be deleted",
                Address = "City",
                Photo = "http://example.com/delete.jpg"
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.DeleteHotelAsync(hotel.Id);

            // Assert
            Assert.True(result);
            Assert.Empty(context.Hotels);
            Assert.Empty(context.Rooms);
        }

        [Fact]
        public async Task DeleteHotelAsync_ShouldReturnFalse_WhenHotelNotFound()
        {
            // Arrange
            var context = GetDbContext();
            var service = GetService(context);

            // Act
            var result = await service.DeleteHotelAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteHotelAsync_ShouldThrowException_WhenHotelHasReservations()
        {
            // Arrange
            var context = GetDbContext();

            var hotel = new Hotel
            {
                Name = "Test Hotel",
                Description = "Description",
                Address = "Ankara",
                Photo = "photo"
            };

            var room = new Room
            {
                Name = "Standard Room",
                Description = "Description",
                Photo = "room-photo",
                RoomNumber = 101,
                Capacity = 2,
                Price = 1500,
                Hotel = hotel
            };

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "test-hash",
                Role = "User"
            };

            var reservation = new Reservation
            {
                User = user,
                Room = room,
                CheckInDate = new DateTime(2026, 12, 1),
                CheckOutDate = new DateTime(2026, 12, 5),
                TotalPrice = 6000,
                Status = "Confirmed"
            };

            context.Add(reservation);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteHotelAsync(hotel.Id));
        }

    }
}
