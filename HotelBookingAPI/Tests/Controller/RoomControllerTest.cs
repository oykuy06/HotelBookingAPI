using HotelBookingAPI.Controllers;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class RoomControllerTest
{
    private readonly Mock<IRoomService> _roomServiceMock;
    private readonly RoomController _controller;

    public RoomControllerTest()
    {
        _roomServiceMock = new Mock<IRoomService>();
        _controller = new RoomController(_roomServiceMock.Object);
    }

    [Fact]
    public async Task GetAllRooms_ReturnsOkResult()
    {
        // Arrange
        var rooms = new List<RoomResponseDto>
        {
            new RoomResponseDto
            {
                Id = 1,
                Name = "Deluxe Room",
                HotelId = 1,
                HotelName = "Test Hotel"
            }
        };

        _roomServiceMock
            .Setup(s => s.GetAllRoomsAsync())
            .ReturnsAsync(rooms);

        // Act
        var result = await _controller.GetAllRooms();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsAssignableFrom<IEnumerable<RoomResponseDto>>(okResult.Value);
        Assert.Single(value);
    }

    [Fact]
    public async Task GetRoomById_RoomNotFound_ReturnsNotFound()
    {
        // Arrange
        _roomServiceMock
            .Setup(s => s.GetRoomByIdAsync(It.IsAny<long>()))
            .ReturnsAsync((RoomResponseDto?)null);

        // Act
        var result = await _controller.GetRoomById(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetRoomById_RoomExists_ReturnsOk()
    {
        // Arrange
        var room = new RoomResponseDto
        {
            Id = 1,
            Name = "Suite"
        };

        _roomServiceMock
            .Setup(s => s.GetRoomByIdAsync(1))
            .ReturnsAsync(room);

        // Act
        var result = await _controller.GetRoomById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<RoomResponseDto>(okResult.Value);
        Assert.Equal(1, value.Id);
    }
}
