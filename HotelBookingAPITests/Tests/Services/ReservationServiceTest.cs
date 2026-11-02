using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ReservationServiceTest
{
    private HotelBookingDBContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<HotelBookingDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HotelBookingDBContext(options);
    }

    private ReservationService GetService(HotelBookingDBContext context)
    {
        return new ReservationService(context);
    }

    [Fact]
    public async Task GetAllReservationsAsync_Returns_List()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Deluxe Room",
            HotelId = 1,
            RoomNumber = 202,
            Capacity = 2,
            Price = 500,
            Description = "Deluxe room description",
            Photo = "http://example.com/photo.jpg"
        });

        context.Reservations.Add(new Reservation
        {
            Id = 1,
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(2),
            TotalPrice = 500,
            Status = "Confirmed"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        // Act
        var result = await service.GetAllReservationsAsync();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetReservationByIdAsync_Returns_Reservation()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Deluxe Room",
            HotelId = 1,
            RoomNumber = 203,
            Capacity = 2,
            Price = 400,
            Description = "Deluxe room description",
            Photo = "http://example.com/photo2.jpg"
        });

        context.Reservations.Add(new Reservation
        {
            Id = 10,
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1),
            TotalPrice = 300,
            Status = "Pending"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        // Act
        var result = await service.GetReservationByIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
    }

    [Fact]
    public async Task CreateReservationAsync_Creates_Reservation()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed_password",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Standard Room",
            HotelId = 1,
            RoomNumber = 101,
            Capacity = 2,
            Price = 300,
            Description = "Standard room description",
            Photo = "http://example.com/standard.jpg"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var reservation = new Reservation
        {
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(3),
            TotalPrice = 0,
            Status = "Confirmed"
        };

        // Act
        var result = await service.CreateReservationAsync(reservation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(900m, result.TotalPrice);
    }

    [Fact]
    public async Task DeleteReservationAsync_Returns_True_When_Exists()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Room 1",
            HotelId = 1,
            RoomNumber = 201,
            Capacity = 2,
            Price = 200,
            Description = "Test room",
            Photo = "http://example.com/room1.jpg"
        });

        context.Reservations.Add(new Reservation
        {
            Id = 5,
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1),
            TotalPrice = 100,
            Status = "Pending"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        // Act
        var result = await service.DeleteReservationAsync(5);

        // Assert
        Assert.True(result);
        Assert.Empty(context.Reservations);
    }

    [Fact]
    public async Task DeleteReservationAsync_Returns_False_When_NotExists()
    {
        // Arrange
        var context = GetDbContext();
        var service = GetService(context);

        // Act
        var result = await service.DeleteReservationAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateReservationAsync_ThrowsException_WhenDatesOverlap()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Standard Room",
            HotelId = 1,
            RoomNumber = 101,
            Capacity = 2,
            Price = 300,
            Description = "Standard room",
            Photo = "photo.jpg"
        });

        context.Reservations.Add(new Reservation
        {
            Id = 1,
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(10),
            TotalPrice = 1500,
            Status = "Confirmed"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var newReservation = new Reservation
        {
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(7),
            CheckOutDate = DateTime.Today.AddDays(12),
            TotalPrice = 1500,
            Status = "Pending"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateReservationAsync(newReservation));
    }

    [Fact]
    public async Task CreateReservationAsync_CreatesReservation_WhenDatesAreAdjacent()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Standard Room",
            HotelId = 1,
            RoomNumber = 101,
            Capacity = 2,
            Price = 300,
            Description = "Standard room",
            Photo = "photo.jpg"
        });

        context.Reservations.Add(new Reservation
        {
            Id = 1,
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(10),
            TotalPrice = 1500,
            Status = "Confirmed"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var newReservation = new Reservation
        {
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(10),
            CheckOutDate = DateTime.Today.AddDays(15),
            TotalPrice = 1500,
            Status = "Pending"
        };

        // Act
        var result = await service.CreateReservationAsync(newReservation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, await context.Reservations.CountAsync());
    }

    [Fact]
    public async Task CreateReservationAsync_AllowsReservation_WhenPreviousReservationIsCancelled()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Standard Room",
            HotelId = 1,
            RoomNumber = 101,
            Capacity = 2,
            Price = 300,
            Description = "Standard room",
            Photo = "photo.jpg"
        });

        context.Reservations.Add(new Reservation
        {
            Id = 1,
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(10),
            TotalPrice = 1500,
            Status = "Cancelled"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var newReservation = new Reservation
        {
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(7),
            CheckOutDate = DateTime.Today.AddDays(12),
            TotalPrice = 1500,
            Status = "Pending"
        };

        // Act
        var result = await service.CreateReservationAsync(newReservation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, await context.Reservations.CountAsync());
    }

    [Fact]
    public async Task UpdateReservationAsync_ThrowsException_WhenDatesOverlap()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Standard Room",
            HotelId = 1,
            RoomNumber = 101,
            Capacity = 2,
            Price = 300,
            Description = "Standard room",
            Photo = "photo.jpg"
        });

        context.Reservations.AddRange(
            new Reservation
            {
                Id = 1,
                UserId = 1,
                RoomId = 1,
                CheckInDate = DateTime.Today.AddDays(5),
                CheckOutDate = DateTime.Today.AddDays(10),
                TotalPrice = 1500,
                Status = "Confirmed"
            },
            new Reservation
            {
                Id = 2,
                UserId = 1,
                RoomId = 1,
                CheckInDate = DateTime.Today.AddDays(15),
                CheckOutDate = DateTime.Today.AddDays(20),
                TotalPrice = 1500,
                Status = "Confirmed"
            });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var updatedReservation = new Reservation
        {
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(7),
            CheckOutDate = DateTime.Today.AddDays(12)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateReservationAsync(2, updatedReservation));
    }

    [Fact]
    public async Task UpdateReservationAsync_UpdatesTotalPrice()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Standard Room",
            HotelId = 1,
            RoomNumber = 101,
            Capacity = 2,
            Price = 300,
            Description = "Standard room",
            Photo = "photo.jpg"
        });

        context.Reservations.Add(new Reservation
        {
            Id = 1,
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(7),
            TotalPrice = 600,
            Status = "Confirmed"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var updatedReservation = new Reservation
        {
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(9)
        };

        // Act
        var result = await service.UpdateReservationAsync(1, updatedReservation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1200m, result.TotalPrice);
    }

    [Fact]
    public async Task UpdateReservationAsync_UpdatesReservation_WhenNoOtherConflictExists()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Standard Room",
            HotelId = 1,
            RoomNumber = 101,
            Capacity = 2,
            Price = 300,
            Description = "Standard room",
            Photo = "photo.jpg"
        });

        context.Reservations.Add(new Reservation
        {
            Id = 1,
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(10),
            TotalPrice = 1500,
            Status = "Confirmed"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var updatedReservation = new Reservation
        {
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(6),
            CheckOutDate = DateTime.Today.AddDays(11)
        };

        // Act
        var result = await service.UpdateReservationAsync(1, updatedReservation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DateTime.Today.AddDays(6), result.CheckInDate);
        Assert.Equal(DateTime.Today.AddDays(11), result.CheckOutDate);

        // Fiyat, durum ve kullanıcı değişmemeli.
        Assert.Equal(1, result.UserId);
        Assert.Equal(1500, result.TotalPrice);
        Assert.Equal("Confirmed", result.Status);
    }

    [Fact]
    public async Task CreateReservationAsync_ThrowsException_WhenUserDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Standard Room",
            HotelId = 1,
            RoomNumber = 101,
            Capacity = 2,
            Price = 300,
            Description = "Standard room",
            Photo = "photo.jpg"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var reservation = new Reservation
        {
            UserId = 999,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(10)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateReservationAsync(reservation));
    }

    [Fact]
    public async Task CreateReservationAsync_ThrowsException_WhenRoomDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var reservation = new Reservation
        {
            UserId = 1,
            RoomId = 999,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(10)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateReservationAsync(reservation));
    }

    [Fact]
    public async Task UpdateReservationAsync_ThrowsException_WhenRoomDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Id = 1,
            Username = "oki",
            Email = "oki@test.com",
            PasswordHash = "hashed",
            Role = "User"
        });

        context.Rooms.Add(new Room
        {
            Id = 1,
            Name = "Standard Room",
            HotelId = 1,
            RoomNumber = 101,
            Capacity = 2,
            Price = 300,
            Description = "Standard room",
            Photo = "photo.jpg"
        });

        context.Reservations.Add(new Reservation
        {
            Id = 1,
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(10),
            TotalPrice = 1500,
            Status = "Confirmed"
        });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var updatedReservation = new Reservation
        {
            RoomId = 999,
            CheckInDate = DateTime.Today.AddDays(6),
            CheckOutDate = DateTime.Today.AddDays(11)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateReservationAsync(1, updatedReservation));
    }
}
