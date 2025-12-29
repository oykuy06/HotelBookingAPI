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

        context.Users.Add(new User { Id = 1, Username = "oki" });
        context.Rooms.Add(new Room { Id = 1, Name = "Deluxe Room" });

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

        context.Users.Add(new User { Id = 1, Username = "oki" });
        context.Rooms.Add(new Room { Id = 1, Name = "Deluxe Room" });

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

        context.Users.Add(new User { Id = 1, Username = "oki" });
        context.Rooms.Add(new Room { Id = 1, Name = "Standard Room" });

        await context.SaveChangesAsync();

        var service = GetService(context);

        var reservation = new Reservation
        {
            UserId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(3),
            TotalPrice = 900,
            Status = "Confirmed"
        };

        // Act
        var result = await service.CreateReservationAsync(reservation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, context.Reservations.Count());
    }

    [Fact]
    public async Task DeleteReservationAsync_Returns_True_When_Exists()
    {
        // Arrange
        var context = GetDbContext();

        context.Reservations.Add(new Reservation { Id = 5 });
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
}
