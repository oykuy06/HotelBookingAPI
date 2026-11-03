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


        [Fact]
        public async Task UpdateRoomAsync_UpdatesRoomDetails()
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

            context.Rooms.Add(new Room
            {
                Id = 1,
                Name = "Old Room",
                HotelId = 1,
                RoomNumber = 101,
                Capacity = 2,
                Price = 100,
                Description = "Old description",
                Photo = "http://example.com/room.jpg"
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            var updatedRoom = new Room
            {
                Name = "Updated Room",
                HotelId = 1,
                RoomNumber = 102,
                Capacity = 3,
                Price = 200,
                Description = "Updated description",
                Photo = "http://example.com/updated-room.jpg"
            };

            // Act
            var result = await service.UpdateRoomAsync(1, updatedRoom);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Room", result.Name);
            Assert.Equal(102, result.RoomNumber);
            Assert.Equal(200, result.Price);
            Assert.Equal(1, result.HotelId);
        }

        [Fact]
        public async Task UpdateRoomAsync_KeepsExistingPhoto_WhenNewPhotoIsEmpty()
        {
            var context = GetDbContext();

            context.Hotels.Add(new Hotel
            {
                Id = 1,
                Name = "Test Hotel",
                Description = "Description",
                Address = "Ankara",
                Photo = "http://example.com/hotel.jpg"
            });

            context.Rooms.Add(new Room
            {
                Id = 1,
                Name = "Standard Room",
                HotelId = 1,
                RoomNumber = 101,
                Capacity = 2,
                Price = 1500,
                Description = "Description",
                Photo = "http://example.com/old-room.jpg"
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            var updatedRoom = new Room
            {
                Name = "Updated Room",
                HotelId = 1,
                RoomNumber = 102,
                Capacity = 3,
                Price = 1800,
                Description = "Updated description",
                Photo = null
            };

            var result = await service.UpdateRoomAsync(1, updatedRoom);

            Assert.NotNull(result);
            Assert.Equal("http://example.com/old-room.jpg", result.Photo);
            Assert.Equal("Updated Room", result.Name);
        }

        [Fact]
        public async Task UpdateRoomAsync_DoesNotChangeHotelId()
        {
            // Arrange
            var context = GetDbContext();

            context.Hotels.AddRange(
                new Hotel
                {
                    Id = 1,
                    Name = "Hotel One",
                    Description = "Desc",
                    Address = "City",
                    Photo = "http://example.com/1.jpg"
                },
                new Hotel
                {
                    Id = 2,
                    Name = "Hotel Two",
                    Description = "Desc",
                    Address = "City",
                    Photo = "http://example.com/2.jpg"
                });

            context.Rooms.Add(new Room
            {
                Id = 1,
                Name = "Room",
                HotelId = 1,
                RoomNumber = 101,
                Capacity = 2,
                Price = 100,
                Description = "Description",
                Photo = "http://example.com/room.jpg"
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            var updatedRoom = new Room
            {
                Name = "Updated Room",
                HotelId = 2,
                RoomNumber = 102,
                Capacity = 3,
                Price = 200,
                Description = "Updated description",
                Photo = "http://example.com/updated-room.jpg"
            };

            // Act
            var result = await service.UpdateRoomAsync(1, updatedRoom);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.HotelId);
            Assert.Equal("Updated Room", result.Name);
        }

        [Fact]
        public async Task UpdateRoomAsync_RoomDoesNotExist_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var service = GetService(context);

            var room = new Room
            {
                Name = "Room",
                HotelId = 1,
                RoomNumber = 101,
                Capacity = 2,
                Price = 100,
                Description = "Description"
            };

            // Act
            var result = await service.UpdateRoomAsync(99, room);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteRoomAsync_ShouldReturnFalse_WhenRoomNotFound()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.DeleteRoomAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteRoomAsync_ShouldThrowException_WhenRoomHasReservations()
        {
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
                RoomNumber = 101,
                Capacity = 2,
                Price = 1500,
                Photo = "room-photo",
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

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteRoomAsync(room.Id));
        }

        [Fact]
        public async Task CreateRoomAsync_HotelNotFound_ThrowsException()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var room = new Room
            {
                Name = "Deluxe Room",
                HotelId = 999,
                RoomNumber = 101,
                Capacity = 2,
                Price = 500,
                Description = "Test room"
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateRoomAsync(room));

            Assert.Empty(context.Rooms);
        }
    }
}
