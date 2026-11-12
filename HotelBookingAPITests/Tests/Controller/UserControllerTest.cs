
using HotelBookingAPI.Constants;
using HotelBookingAPI.Controllers;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

public class UserControllerTest
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTest()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);
    }

    private void SetUserContext(long userId, string role)
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString()
            ),
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

    [Fact]
    public async Task GetUser_UserExists_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "User");

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
        var returnedUser =
            Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Equal(1, returnedUser.Id);
    }

    [Fact]
    public async Task GetUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        SetUserContext(99, "User");

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
        SetUserContext(1, "Admin");

        var users = new List<UserResponseDto>
        {
            new UserResponseDto
            {
                Id = 1,
                Email = "a@test.com",
                Role = "User"
            },
            new UserResponseDto
            {
                Id = 2,
                Email = "b@test.com",
                Role = "Admin"
            }
        };

        _userServiceMock
    .Setup(s => s.GetAllUsersAsync(It.IsAny<long?>()))
    .ReturnsAsync(users);

        _userServiceMock
    .Setup(s => s.GetHotelIdByUserIdAsync(1))
    .ReturnsAsync(1);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var list = Assert.IsAssignableFrom
            <IEnumerable<UserResponseDto>>(okResult.Value);

        Assert.Equal(2, list.Count());
    }


    [Fact]
    public async Task UpdateUser_AdminAssignsSuperAdmin_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var dto = new HotelBookingAPI.Dto.RequestDto.UserDto
        {
            Email = "target@test.com",
            Password = "Password123",
            Role = "SuperAdmin"
        };

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        // Güvenlik kontrolü nedeniyle servis çağrılmamalı.
        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<long?>()),
            Times.Never);
    }


    [Fact]
    public async Task UpdateUser_AdminUpdatesExistingSuperAdmin_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(new UserResponseDto
            {
                Id = 2,
                Email = "superadmin@test.com",
                Role = "SuperAdmin"
            });

        var dto = new HotelBookingAPI.Dto.RequestDto.UserDto
        {
            Email = "changed@test.com",
            Password = "Password123",
            Role = "User"
        };

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<long?>()),
            Times.Never);
    }



    [Fact]
    public async Task UpdateUser_UserUpdatesAnotherUser_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "User");

        var dto = new HotelBookingAPI.Dto.RequestDto.UserDto
        {
            Email = "other@test.com",
            Password = "Password123",
            Role = "User"
        };

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<long?>()),
            Times.Never);
    }


    [Fact]
    public async Task UpdateUser_UserUpdatesOwnAccount_CannotChangeRole()
    {
        // Arrange
        SetUserContext(1, "User");

        var dto = new HotelBookingAPI.Dto.RequestDto.UserDto
        {
            Email = "updated@test.com",
            Password = "Password123",
            Role = "Admin"
        };

        var updatedUser = new HotelBookingAPI.Entity.Models.User
        {
            Id = 1,
            Email = "updated@test.com",
            Username = "updated",
            PasswordHash = "hashed-password",
            Role = "User"
        };

        _userServiceMock
    .Setup(s => s.UpdateUserAsync(
        1,
        "updated@test.com",
        "Password123",
        null,
        null))
    .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(1, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response =
            Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Equal("updated@test.com", response.Email);
        Assert.Equal("User", response.Role);

        _userServiceMock.Verify(
    s => s.UpdateUserAsync(
        1,
        "updated@test.com",
        "Password123",
        null,
        null),
    Times.Once);
    }

    [Fact]
    public async Task CreateUser_AdminCreatesUser_ReturnsCreated()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var dto = new HotelBookingAPI.Dto.RequestDto.UserDto
        {
            Email = "newuser@test.com",
            Password = "Password123",
            Role = "User"
        };

        var createdUser = new HotelBookingAPI.Entity.Models.User
        {
            Id = 3,
            Email = "newuser@test.com",
            Username = "newuser",
            PasswordHash = "hashed-password",
            Role = "User",
            HotelId = 1
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.CreateUserAsync(
                "newuser@test.com",
                "Password123",
                "User",
                1))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<UserResponseDto>(createdResult.Value);

        Assert.Equal(3, response.Id);
        Assert.Equal("newuser@test.com", response.Email);
        Assert.Equal("User", response.Role);

        _userServiceMock.Verify(
            s => s.CreateUserAsync(
                "newuser@test.com",
                "Password123",
                "User",
                1),
            Times.Once);
    }


    [Fact]
    public async Task CreateUser_InvalidRole_ReturnsBadRequest()
    {
        // Arrange
        SetUserContext(1, "SuperAdmin");

        var dto = new HotelBookingAPI.Dto.RequestDto.UserDto
        {
            Email = "invalidrole@test.com",
            Password = "Password123",
            Role = "SuperMegaAdmin"
        };

        // Act
        var result = await _controller.CreateUser(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        _userServiceMock.Verify(
    s => s.CreateUserAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<long?>()),
    Times.Never);
    }


    [Fact]
    public async Task DeleteUser_UserExists_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var targetUser = new UserResponseDto
        {
            Id = 2,
            Email = "user@test.com",
            Role = UserRoles.User
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(targetUser);

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(2))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.DeleteUserAsync(2))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(2);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        _userServiceMock.Verify(
            s => s.DeleteUserAsync(2),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        SetUserContext(1, "Admin");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(99))
            .ReturnsAsync((UserResponseDto?)null);

        // Act
        var result = await _controller.DeleteUser(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        _userServiceMock.Verify(
            s => s.DeleteUserAsync(99),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUser_UserBelongsToAnotherHotel_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var targetUser = new UserResponseDto
        {
            Id = 2,
            Email = "user@test.com",
            Role = UserRoles.User
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(targetUser);

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(2))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.DeleteUser(2);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.DeleteUserAsync(2),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUser_AdminCannotDeleteAnotherAdmin_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var targetUser = new UserResponseDto
        {
            Id = 2,
            Email = "admin@test.com",
            Role = UserRoles.Admin
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(targetUser);

        // Act
        var result = await _controller.DeleteUser(2);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.DeleteUserAsync(2),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUser_SuperAdminCanDeleteUser_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "SuperAdmin");

        _userServiceMock
            .Setup(s => s.UserExistsAsync(2))
            .ReturnsAsync(true);

        _userServiceMock
            .Setup(s => s.DeleteUserAsync(2))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(2);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        _userServiceMock.Verify(
            s => s.UserExistsAsync(2),
            Times.Once);

        _userServiceMock.Verify(
            s => s.DeleteUserAsync(2),
            Times.Once);
    }


    [Theory]
    [InlineData("admin")]
    [InlineData("ADMIN")]
    [InlineData(" Admin ")]
    public async Task CreateUser_AdminRoleIsNormalized_ReturnsCreated(string role)
    {
        // Arrange
        SetUserContext(1, "SuperAdmin");

        var dto = new HotelBookingAPI.Dto.RequestDto.UserDto
        {
            Email = "newadmin@test.com",
            Password = "Password123",
            Role = role,
            HotelId = 1
        };

        var createdUser = new HotelBookingAPI.Entity.Models.User
        {
            Id = 10,
            Email = "newadmin@test.com",
            Username = "newadmin",
            PasswordHash = "hashed-password",
            Role = UserRoles.Admin,
            HotelId = 1
        };

        _userServiceMock
    .Setup(s => s.CreateUserAsync(
        "newadmin@test.com",
        "Password123",
        UserRoles.Admin,
        1))
    .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<UserResponseDto>(createdResult.Value);

        Assert.Equal("Admin", response.Role);

        _userServiceMock.Verify(
    s => s.CreateUserAsync(
        "newadmin@test.com",
        "Password123",
        "Admin",
        1),
    Times.Once);
    }

    [Fact]
    public async Task UpdateUser_AdminRoleIsNormalized_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "SuperAdmin");

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(new UserResponseDto
            {
                Id = 2,
                Email = "target@test.com",
                Role = "User"
            });

        var dto = new HotelBookingAPI.Dto.RequestDto.UserDto
        {
            Email = "target@test.com",
            Password = "Password123",
            Role = " admin ",
            HotelId = 1
        };

        var updatedUser = new HotelBookingAPI.Entity.Models.User
        {
            Id = 2,
            Email = "target@test.com",
            Username = "target",
            PasswordHash = "hashed-password",
            Role = UserRoles.Admin,
            HotelId = 1
        };

        _userServiceMock
            .Setup(s => s.UpdateUserAsync(
                2,
                "target@test.com",
                "Password123",
                "Admin",
                1))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Equal("Admin", response.Role);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                2,
                "target@test.com",
                "Password123",
                "Admin",
                1),
            Times.Once);
    }

    [Fact]
    public async Task UpdateUser_InvalidRole_ReturnsBadRequest()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var dto = new HotelBookingAPI.Dto.RequestDto.UserDto
        {
            Email = "target@test.com",
            Password = "Password123",
            Role = "SuperMegaAdmin"
        };

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<long?>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUser_UserHasRelatedRecords_ReturnsConflict()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var targetUser = new UserResponseDto
        {
            Id = 2,
            Email = "user@test.com",
            Role = UserRoles.User
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(targetUser);

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(2))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.DeleteUserAsync(2))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUser(2);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);

        _userServiceMock.Verify(
            s => s.DeleteUserAsync(2),
            Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_AdminSeesOnlyOwnHotelUsers_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var users = new List<UserResponseDto>
    {
        new UserResponseDto
        {
            Id = 2,
            Email = "user1@test.com",
            Role = UserRoles.User
        }
    };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetAllUsersAsync(1))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<UserResponseDto>>(
            okResult.Value);

        Assert.Single(response);

        _userServiceMock.Verify(
            s => s.GetAllUsersAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_SuperAdminSeesAllUsers_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "SuperAdmin");

        var users = new List<UserResponseDto>
    {
        new UserResponseDto
        {
            Id = 2,
            Email = "user1@test.com",
            Role = UserRoles.User
        },
        new UserResponseDto
        {
            Id = 3,
            Email = "user2@test.com",
            Role = UserRoles.User
        }
    };

        _userServiceMock
            .Setup(s => s.GetAllUsersAsync(null))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var response = Assert.IsAssignableFrom<IEnumerable<UserResponseDto>>(
            okResult.Value);

        Assert.Equal(2, response.Count());

        _userServiceMock.Verify(
            s => s.GetAllUsersAsync(null),
            Times.Once);
    }

    [Fact]
    public async Task GetUser_AdminCanAccessOwnHotelUser_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var user = new UserResponseDto
        {
            Id = 2,
            Email = "user1@test.com",
            Role = UserRoles.User
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.UserExistsAsync(2))
            .ReturnsAsync(true);

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(2))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUser(2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Equal(2, response.Id);

        _userServiceMock.Verify(
            s => s.GetUserByIdAsync(2),
            Times.Once);
    }

    [Fact]
    public async Task GetUser_AdminCannotAccessAnotherHotelUser_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.UserExistsAsync(2))
            .ReturnsAsync(true);

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(2))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.GetUser(2);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.GetUserByIdAsync(2),
            Times.Never);
    }

    [Fact]
    public async Task GetUser_SuperAdminCanAccessAnyHotelUser_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "SuperAdmin");

        var user = new UserResponseDto
        {
            Id = 2,
            Email = "user2@test.com",
            Role = UserRoles.User
        };

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUser(2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Equal(2, response.Id);

        _userServiceMock.Verify(
            s => s.GetUserByIdAsync(2),
            Times.Once);

        _userServiceMock.Verify(
            s => s.GetHotelIdByUserIdAsync(It.IsAny<long>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUser_UserCannotAccessAnotherUser_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "User");

        // Act
        var result = await _controller.GetUser(2);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.GetUserByIdAsync(It.IsAny<long>()),
            Times.Never);
    }
    [Fact]
    public async Task UpdateUser_AdminCanUpdateOwnHotelUser_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var dto = new UserDto
        {
            Email = "updated@test.com",
            Password = "Password123",
            Role = UserRoles.User
        };

        var targetUser = new UserResponseDto
        {
            Id = 2,
            Email = "old@test.com",
            Role = UserRoles.User
        };

        var updatedUser = new User
        {
            Id = 2,
            Email = "updated@test.com",
            Role = UserRoles.User
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(targetUser);

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(2))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.UpdateUserAsync(
                2, "updated@test.com", "Password123", null, null))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Equal("updated@test.com", response.Email);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                2, "updated@test.com", "Password123", null, null),
            Times.Once);
    }

    [Fact]
    public async Task UpdateUser_AdminCannotUpdateAnotherHotelUser_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var dto = new UserDto
        {
            Email = "updated@test.com",
            Password = "Password123",
            Role = UserRoles.User
        };

        var targetUser = new UserResponseDto
        {
            Id = 2,
            Email = "user@test.com",
            Role = UserRoles.User
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(targetUser);

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(2))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<long?>()),
            Times.Never);
    }
    [Fact]
    public async Task UpdateUser_AdminCannotUpdateSuperAdmin_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var dto = new UserDto
        {
            Email = "updated@test.com",
            Password = "Password123",
            Role = UserRoles.User
        };

        var targetUser = new UserResponseDto
        {
            Id = 2,
            Email = "superadmin@test.com",
            Role = UserRoles.SuperAdmin
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(targetUser);

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<long?>()),
            Times.Never);
    }
    [Fact]
    public async Task UpdateUser_SuperAdminCanUpdateRoleAndHotel_ReturnsOk()
    {
        // Arrange
        SetUserContext(1, "SuperAdmin");

        var dto = new UserDto
        {
            Email = "admin@test.com",
            Password = "Password123",
            Role = UserRoles.Admin,
            HotelId = 2
        };

        var updatedUser = new User
        {
            Id = 3,
            Email = "admin@test.com",
            Role = UserRoles.Admin,
            HotelId = 2
        };

        _userServiceMock
            .Setup(s => s.UpdateUserAsync(
                3, "admin@test.com", "Password123",
                UserRoles.Admin, 2))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(3, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Equal(UserRoles.Admin, response.Role);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                3, "admin@test.com", "Password123",
                UserRoles.Admin, 2),
            Times.Once);
    }
    [Fact]
    public async Task UpdateUser_UserCanUpdateOwnProfile_ReturnsOk()
    {
        // Arrange
        SetUserContext(2, "User");

        var dto = new UserDto
        {
            Email = "updated@test.com",
            Password = "Password123",
            Role = UserRoles.User
        };

        var updatedUser = new User
        {
            Id = 2,
            Email = "updated@test.com",
            Role = UserRoles.User
        };

        _userServiceMock
            .Setup(s => s.UpdateUserAsync(
                2, "updated@test.com", "Password123", null, null))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Equal("updated@test.com", response.Email);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                2, "updated@test.com", "Password123", null, null),
            Times.Once);
    }
    [Fact]
    public async Task UpdateUser_UserCannotUpdateAnotherUser_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "User");

        var dto = new UserDto
        {
            Email = "updated@test.com",
            Password = "Password123",
            Role = UserRoles.User
        };

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<long?>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUser_AdminCannotDeleteAnotherHotelUser_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var targetUser = new UserResponseDto
        {
            Id = 2,
            Email = "user@test.com",
            Role = UserRoles.User
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(targetUser);

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(2))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.DeleteUser(2);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.DeleteUserAsync(It.IsAny<long>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUser_AdminWithoutHotelCannotDeleteUser_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync((long?)null);

        // Act
        var result = await _controller.DeleteUser(2);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.DeleteUserAsync(It.IsAny<long>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUser_AdminCannotUpdateAnotherAdmin_ReturnsForbid()
    {
        // Arrange
        SetUserContext(1, "Admin");

        var dto = new UserDto
        {
            Email = "updated@test.com",
            Password = "Password123",
            Role = UserRoles.User
        };

        var targetUser = new UserResponseDto
        {
            Id = 2,
            Email = "admin@test.com",
            Role = UserRoles.Admin
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(1);

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(2))
            .ReturnsAsync(targetUser);

        // Act
        var result = await _controller.UpdateUser(2, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _userServiceMock.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<long?>()),
            Times.Never);
    }
}
