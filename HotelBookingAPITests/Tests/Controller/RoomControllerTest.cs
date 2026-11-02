
using HotelBookingAPI.Controllers;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

public class RoomControllerTest
{
    private readonly Mock<IRoomService> _roomServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly RoomController _controller;

    public RoomControllerTest()
    {
        _roomServiceMock = new Mock<IRoomService>();
        _userServiceMock = new Mock<IUserService>();

        _controller = new RoomController(
            _roomServiceMock.Object,
            _userServiceMock.Object);
    }

    private void SetUser(string role, long userId = 1)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    private RoomDto CreateRoomDto(long hotelId = 1)
    {
        return new RoomDto
        {
            Name = "Deluxe Room",
            HotelId = hotelId,
            RoomNumber = 101,
            Capacity = 2,
            Price = 500,
            Description = "Test room",
            Photo = null
        };
    }

    private RoomResponseDto CreateRoomResponse(long hotelId = 1)
    {
        return new RoomResponseDto
        {
            Id = 1,
            Name = "Deluxe Room",
            HotelId = hotelId,
            HotelName = "Test Hotel",
            RoomNumber = 101,
            Capacity = 2,
            Price = 500,
            Description = "Test room"
        };
    }

    private Room CreateRoom(long hotelId = 1)
    {
        return new Room
        {
            Id = 1,
            Name = "Deluxe Room",
            HotelId = hotelId,
            RoomNumber = 101,
            Capacity = 2,
            Price = 500,
            Description = "Test room"
        };
    }

    [Fact]
    public async Task GetAllRooms_ReturnsOkResult()
    {
        var rooms = new List<RoomResponseDto>
        {
            CreateRoomResponse()
        };

        _roomServiceMock
            .Setup(s => s.GetAllRoomsAsync())
            .ReturnsAsync(rooms);

        var result = await _controller.GetAllRooms();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsAssignableFrom<IEnumerable<RoomResponseDto>>(okResult.Value);

        Assert.Single(value);
    }

    [Fact]
    public async Task GetRoomById_RoomNotFound_ReturnsNotFound()
    {
        _roomServiceMock
            .Setup(s => s.GetRoomByIdAsync(It.IsAny<long>()))
            .ReturnsAsync((RoomResponseDto?)null);

        var result = await _controller.GetRoomById(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetRoomById_RoomExists_ReturnsOk()
    {
        var room = CreateRoomResponse();

        _roomServiceMock
            .Setup(s => s.GetRoomByIdAsync(1))
            .ReturnsAsync(room);

        var result = await _controller.GetRoomById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<RoomResponseDto>(okResult.Value);

        Assert.Equal(1, value.Id);
    }

    [Fact]
    public async Task CreateRoom_AdminCreatesRoomForOwnHotel_ReturnsCreated()
    {
        SetUser("Admin");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1L);

        _roomServiceMock
            .Setup(s => s.CreateRoomAsync(It.IsAny<Room>()))
            .ReturnsAsync((Room room) => room);

        var result = await _controller.CreateRoom(CreateRoomDto(1));

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(1L, ((RoomResponseDto)createdResult.Value!).HotelId);

        _roomServiceMock.Verify(
            s => s.CreateRoomAsync(It.Is<Room>(r => r.HotelId == 1)),
            Times.Once);
    }

    [Fact]
    public async Task CreateRoom_AdminHasNoHotel_ReturnsForbid()
    {
        SetUser("Admin");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync((long?)null);

        var result = await _controller.CreateRoom(CreateRoomDto());

        Assert.IsType<ForbidResult>(result);

        _roomServiceMock.Verify(
            s => s.CreateRoomAsync(It.IsAny<Room>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateRoom_SuperAdminCanCreateRoom_ReturnsCreated()
    {
        SetUser("SuperAdmin");

        _roomServiceMock
            .Setup(s => s.CreateRoomAsync(It.IsAny<Room>()))
            .ReturnsAsync((Room room) => room);

        var result = await _controller.CreateRoom(CreateRoomDto(2));

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(2L, ((RoomResponseDto)createdResult.Value!).HotelId);
    }

    [Fact]
    public async Task UpdateRoom_AdminUpdatesOwnHotelRoom_ReturnsOk()
    {
        SetUser("Admin");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1L);

        _roomServiceMock
            .Setup(s => s.GetRoomByIdAsync(1))
            .ReturnsAsync(CreateRoomResponse(1));

        _roomServiceMock
            .Setup(s => s.UpdateRoomAsync(1, It.IsAny<Room>()))
            .ReturnsAsync((long id, Room room) => room);

        var result = await _controller.UpdateRoom(1, CreateRoomDto(1));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateRoom_AdminCannotUpdateAnotherHotelsRoom_ReturnsForbid()
    {
        SetUser("Admin");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1L);

        _roomServiceMock
            .Setup(s => s.GetRoomByIdAsync(1))
            .ReturnsAsync(CreateRoomResponse(2));

        var result = await _controller.UpdateRoom(1, CreateRoomDto(2));

        Assert.IsType<ForbidResult>(result);

        _roomServiceMock.Verify(
            s => s.UpdateRoomAsync(It.IsAny<long>(), It.IsAny<Room>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRoom_SuperAdminCanUpdateRoom_ReturnsOk()
    {
        SetUser("SuperAdmin");

        _roomServiceMock
            .Setup(s => s.GetRoomByIdAsync(1))
            .ReturnsAsync(CreateRoomResponse(2));

        _roomServiceMock
            .Setup(s => s.UpdateRoomAsync(1, It.IsAny<Room>()))
            .ReturnsAsync((long id, Room room) => room);

        var result = await _controller.UpdateRoom(1, CreateRoomDto(2));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteRoom_AdminCannotDeleteAnotherHotelsRoom_ReturnsForbid()
    {
        SetUser("Admin");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1L);

        _roomServiceMock
            .Setup(s => s.GetRoomByIdAsync(1))
            .ReturnsAsync(CreateRoomResponse(2));

        var result = await _controller.DeleteRoom(1);

        Assert.IsType<ForbidResult>(result);

        _roomServiceMock.Verify(
            s => s.DeleteRoomAsync(It.IsAny<long>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteRoom_AdminDeletesOwnHotelRoom_ReturnsOk()
    {
        SetUser("Admin");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1L);

        _roomServiceMock
            .Setup(s => s.GetRoomByIdAsync(1))
            .ReturnsAsync(CreateRoomResponse(1));

        _roomServiceMock
            .Setup(s => s.DeleteRoomAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteRoom(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteRoom_SuperAdminCanDeleteRoom_ReturnsOk()
    {
        SetUser("SuperAdmin");

        _roomServiceMock
            .Setup(s => s.DeleteRoomAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteRoom(1);

        Assert.IsType<OkObjectResult>(result);
    }
}
