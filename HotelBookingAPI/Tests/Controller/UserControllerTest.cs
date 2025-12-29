using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HotelBookingAPI.Controllers;
using HotelBookingAPI.Services.Interfaces;
using HotelBookingAPI.Dto.ResponseDto;

public class UserControllerTest
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTest()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);
    }

    [Fact]
    public async Task GetUser_UserExists_ReturnsOk()
    {
        // Arrange
        var userDto = new UserResponseDto
        {
            Id = 1,
            Email = "test@test.com",
            Role = "User"
        };

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(1))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUser(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Equal(1, returnedUser.Id);
    }

    [Fact]
    public async Task GetUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(99))
            .ReturnsAsync((UserResponseDto?)null);

        // Act
        var result = await _controller.GetUser(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOk()
    {
        // Arrange
        var users = new List<UserResponseDto>
        {
            new UserResponseDto { Id = 1, Email = "a@test.com", Role = "User" },
            new UserResponseDto { Id = 2, Email = "b@test.com", Role = "Admin" }
        };

        _userServiceMock
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<UserResponseDto>>(okResult.Value);

        Assert.Equal(2, list.Count());
    }
}
