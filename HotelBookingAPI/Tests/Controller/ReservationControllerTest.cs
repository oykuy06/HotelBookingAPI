using HotelBookingAPI.Controllers;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class ReservationControllerTests
{
    private readonly Mock<IReservationService> _reservationServiceMock;
    private readonly ReservationController _controller;

    public ReservationControllerTests()
    {
        _reservationServiceMock = new Mock<IReservationService>();
        _controller = new ReservationController(_reservationServiceMock.Object);
    }

    [Fact]
    public async Task GetReservationById_ReturnsOk_WhenReservationExists()
    {
        // Arrange
        var reservationId = 1;

        var reservation = new ReservationResponseDto
        {
            Id = reservationId,
            UserId = 10,
            UserName = "oki",
            RoomId = 5,
            RoomName = "Deluxe",
            TotalPrice = 500,
            Status = "Confirmed"
        };

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(reservation);

        // Act
        var result = await _controller.GetReservationById(reservationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedReservation = Assert.IsType<ReservationResponseDto>(okResult.Value);

        Assert.Equal(reservationId, returnedReservation.Id);
        Assert.Equal("Confirmed", returnedReservation.Status);
    }

    [Fact]
    public async Task GetReservationById_ReturnsNotFound_WhenReservationDoesNotExist()
    {
        // Arrange
        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(It.IsAny<long>()))
            .ReturnsAsync((ReservationResponseDto?)null);

        // Act
        var result = await _controller.GetReservationById(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteReservation_ReturnsOk_WhenDeleted()
    {
        // Arrange
        _reservationServiceMock
            .Setup(s => s.DeleteReservationAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteReservation(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task DeleteReservation_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        _reservationServiceMock
            .Setup(s => s.DeleteReservationAsync(1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteReservation(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
