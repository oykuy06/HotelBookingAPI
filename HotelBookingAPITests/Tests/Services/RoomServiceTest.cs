using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HotelBookingAPI.Tests.Services
{
    public class RoomServiceTest
    {
        private HotelBookingDBContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<HotelBookingDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new HotelBookingDBContext(options);
        }

        private RoomService GetService(HotelBookingDBContext context)
        {
            return new RoomService(context);
        }

        [Fact]
        public async Task CreateRoomAsync_ValidRoom_ReturnsRoom()
        {
            // Arrange
            var context = GetDbContext();
            context.Hotels.Add(new Hotel
            {
                Id = 1,
                Name = "Test Hotel",
                Description = "Test Desc",
                Address = "City",
                Photo = "http://example.com/photo.jpg"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);

            var room = new Room
            {
                Name = "Deluxe Room",
                HotelId = 1,
                RoomNumber = 101,
                Capacity = 2,
                Price = 100,
                Description = "A nice room",
                Photo = "http://example.com/room.jpg"
            };

            // Act
            var result = await service.CreateRoomAsync(room);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Deluxe Room", result.Name);
            Assert.Equal(1, context.Rooms.Count());
        }

        [Fact]
        public async Task CreateRoomAsync_InvalidPhoto_ThrowsException()
        {
            // Arrange
            var context = GetDbContext();
            context.Hotels.Add(new Hotel
            {
                Id = 1,
                Name = "Test Hotel",
                Description = "Desc",
                Address = "City",
                Photo = "http://example.com/photo.jpg"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);

            var room = new Room
            {
                Name = "Bad Room",
                HotelId = 1,
                RoomNumber = 102,
                Capacity = 1,
                Price = 50,
                Description = "Bad room",
                Photo = "not-a-url"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateRoomAsync(room));
        }

        [Fact]
        public async Task GetRoomByIdAsync_RoomExists_ReturnsRoom()
        {
            // Arrange
            var context = GetDbContext();

            context.Hotels.Add(new Hotel
            {
                Id = 1,
                Name = "Hotel",
                Description = "Desc",
                Address = "City",
                Photo = "http://example.com/photo.jpg"
            });
            context.Rooms.Add(new Room
            {
                Id = 1,
                Name = "Room 1",
                HotelId = 1,
                RoomNumber = 101,
                Capacity = 2,
                Price = 100,
                Description = "Nice room",
                Photo = "http://example.com/room1.jpg"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.GetRoomByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Room 1", result.Name);
        }

        [Fact]
        public async Task DeleteRoomAsync_RoomExists_ReturnsTrue()
        {
            // Arrange
            var context = GetDbContext();

            context.Hotels.Add(new Hotel
            {
                Id = 1,
                Name = "Hotel",
                Description = "Desc",
                Address = "City",
                Photo = "http://example.com/photo.jpg"
            });

            context.Rooms.Add(new Room
            {
                Id = 1,
                Name = "Room",
                HotelId = 1,
                RoomNumber = 101,
                Capacity = 2,
                Price = 100,
                Description = "Desc",
                Photo = "http://example.com/photo.jpg"
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.DeleteRoomAsync(1);

            // Assert
            Assert.True(result);
            Assert.Empty(context.Rooms);
        }
    }
}
