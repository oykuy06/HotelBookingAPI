using HotelBookingAPI.Controllers;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;
using System.Reflection;

public class ReservationControllerTests
{
    private readonly Mock<IReservationService> _reservationServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly ReservationController _controller;

    public ReservationControllerTests()
    {
        _reservationServiceMock = new Mock<IReservationService>();
        _userServiceMock = new Mock<IUserService>();

        _controller = new ReservationController(
            _reservationServiceMock.Object,
            _userServiceMock.Object);

        SetUserContext();
    }

    private void SetUserContext(string role = "User", string userId = "10")
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Role, role)
    };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "Test"))
            }
        };
    }

    [Fact]
    public async Task CreateReservation_ReturnsCreated_WhenReservationIsValid()
    {
        // Arrange
        SetUserContext("User", "10");
        var checkIn = DateTime.Today.AddDays(5);
        var checkOut = DateTime.Today.AddDays(8);

        var dto = new CreateReservationDto
        {
            RoomId = 5,
            CheckInDate = checkIn,
            CheckOutDate = checkOut
        };

        Reservation? capturedReservation = null;

        var createdReservation = new Reservation
        {
            Id = 1,
            UserId = 10,
            RoomId = 5,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            TotalPrice = 0,
            Status = "Pending"
        };

        // CreateReservationAsync mock
        _reservationServiceMock
            .Setup(s => s.CreateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => capturedReservation = r)
            .ReturnsAsync(createdReservation);

        // GetReservationByIdAsync mock
        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(1))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = 1,
                UserId = 10,
                UserName = "testuser",
                RoomId = 5,
                RoomName = "Room 5",
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                TotalPrice = 0,
                Status = "Pending"
            });

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<ReservationResponseDto>(createdResult.Value);

        Assert.Equal(10, capturedReservation!.UserId);
        Assert.Equal(5, capturedReservation.RoomId);
        Assert.Equal("Pending", capturedReservation.Status);
        Assert.Equal(0, capturedReservation.TotalPrice);

        Assert.Equal(1, response.Id);
        Assert.Equal("testuser", response.UserName);
        Assert.Equal("Room 5", response.RoomName);

        _reservationServiceMock.Verify(
            s => s.GetReservationByIdAsync(1),
            Times.Once);
    }


    [Fact]
    public async Task CreateReservation_ReturnsBadRequest_WhenCheckInDateIsAfterCheckOutDate()
    {
        // Arrange
        var dto = new CreateReservationDto
        {
            RoomId = 5,
            CheckInDate = DateTime.Today.AddDays(8),
            CheckOutDate = DateTime.Today.AddDays(5)
        };

        _reservationServiceMock
            .Setup(s => s.CreateReservationAsync(It.IsAny<Reservation>()))
            .ThrowsAsync(new ArgumentException("Invalid reservation dates."));

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        _reservationServiceMock.Verify(
            s => s.CreateReservationAsync(It.IsAny<Reservation>()),
            Times.Once);
            }

    [Fact]
    public void GetAllReservations_RequiresAdminOrSuperAdmin()
    {
        // Arrange
        var method = typeof(ReservationController)
            .GetMethod(nameof(ReservationController.GetAllReservations));

        // Act
        var authorizeAttribute = method?
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(authorizeAttribute);
        Assert.Equal("Admin,SuperAdmin", authorizeAttribute.Roles);
    }

    [Fact]
    public async Task GetReservationById_AdminCanAccessOwnHotelReservation()
    {
        // Arrange
        SetUserContext("Admin", "1");

        var reservation = new ReservationResponseDto
        {
            Id = 1,
            UserId = 10,
            RoomId = 5,
            HotelId = 100,
            Status = "Confirmed"
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(100);

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(1))
            .ReturnsAsync(reservation);

        // Act
        var result = await _controller.GetReservationById(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetReservationById_AdminWithoutHotel_ReturnsForbid()
    {
        // Arrange
        SetUserContext("Admin", "1");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync((long?)null);

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(1))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = 1,
                UserId = 10,
                RoomId = 5,
                HotelId = 100,
                Status = "Confirmed"
            });

        // Act
        var result = await _controller.GetReservationById(1);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAllReservations_AdminWithoutHotel_ReturnsForbid()
    {
        // Arrange
        SetUserContext("Admin", "1");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync((long?)null);

        _reservationServiceMock
            .Setup(s => s.GetAllReservationsAsync())
            .ReturnsAsync(new List<ReservationResponseDto>());

        // Act
        var result = await _controller.GetAllReservations();

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetReservationById_AdminCannotAccessOtherHotelReservation()
    {
        // Arrange
        SetUserContext("Admin", "1");

        var reservation = new ReservationResponseDto
        {
            Id = 2,
            UserId = 20,
            RoomId = 5,
            HotelId = 200,
            Status = "Confirmed"
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(100);

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(2))
            .ReturnsAsync(reservation);

        // Act
        var result = await _controller.GetReservationById(2);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetReservationById_SuperAdminCanAccessAnyHotelReservation()
    {
        // Arrange
        SetUserContext("SuperAdmin", "1");

        var reservation = new ReservationResponseDto
        {
            Id = 3,
            UserId = 20,
            RoomId = 5,
            HotelId = 200,
            Status = "Confirmed"
        };

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(3))
            .ReturnsAsync(reservation);

        // Act
        var result = await _controller.GetReservationById(3);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        _userServiceMock.Verify(
            s => s.GetHotelIdByUserIdAsync(It.IsAny<long>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateReservation_AdminCanUpdateOwnHotelReservation()
    {
        // Arrange
        SetUserContext("Admin", "1");

        var checkIn = DateTime.Today.AddDays(5);
        var checkOut = DateTime.Today.AddDays(8);

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(100);

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(1))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = 1,
                UserId = 10,
                RoomId = 5,
                HotelId = 100
            });

        _reservationServiceMock
            .Setup(s => s.UpdateReservationAsync(
                1, It.IsAny<Reservation>()))
            .ReturnsAsync(new Reservation { Id = 1 });

        var dto = new UpdateReservationDto
        {
            RoomId = 5,
            CheckInDate = checkIn,
            CheckOutDate = checkOut
        };

        // Act
        var result = await _controller.UpdateReservation(1, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        _reservationServiceMock.Verify(
            s => s.UpdateReservationAsync(1, It.IsAny<Reservation>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateReservation_AdminCannotUpdateOtherHotelReservation()
    {
        // Arrange
        SetUserContext("Admin", "1");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(100);

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(2))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = 2,
                UserId = 20,
                RoomId = 5,
                HotelId = 200
            });

        var dto = new UpdateReservationDto
        {
            RoomId = 5,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(8)
        };

        // Act
        var result = await _controller.UpdateReservation(2, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _reservationServiceMock.Verify(
            s => s.UpdateReservationAsync(2, It.IsAny<Reservation>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateReservation_SuperAdminCanUpdateAnyHotelReservation()
    {
        // Arrange
        SetUserContext("SuperAdmin", "1");

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(2))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = 2,
                UserId = 20,
                RoomId = 5,
                HotelId = 200
            });

        _reservationServiceMock
            .Setup(s => s.UpdateReservationAsync(
                2, It.IsAny<Reservation>()))
            .ReturnsAsync(new Reservation { Id = 2 });

        var dto = new UpdateReservationDto
        {
            RoomId = 5,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(8)
        };

        // Act
        var result = await _controller.UpdateReservation(2, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        _userServiceMock.Verify(
            s => s.GetHotelIdByUserIdAsync(It.IsAny<long>()),
            Times.Never);

        _reservationServiceMock.Verify(
            s => s.UpdateReservationAsync(2, It.IsAny<Reservation>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllReservations_AdminAccess_ReturnsOk()
    {
        // Arrange
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "Admin")
    };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "Test"))
            }
        };

        _userServiceMock
    .Setup(s => s.GetHotelIdByUserIdAsync(1))
    .ReturnsAsync(100);

        _reservationServiceMock
            .Setup(s => s.GetAllReservationsAsync())
            .ReturnsAsync(new List<ReservationResponseDto>
            {
            new ReservationResponseDto
            {
                Id = 1,
                UserId = 10,
                RoomId = 5,
                HotelId = 100
            }
            });

        // Act
        var result = await _controller.GetAllReservations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAllReservations_SuperAdminAccess_ReturnsOk()
    {
        // Arrange
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "SuperAdmin")
    };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "Test"))
            }
        };

        _reservationServiceMock
            .Setup(s => s.GetAllReservationsAsync())
            .ReturnsAsync(new List<ReservationResponseDto>
            {
            new ReservationResponseDto
            {
                Id = 1,
                UserId = 10,
                RoomId = 5
            }
            });

        // Act
        var result = await _controller.GetAllReservations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetReservationById_ReturnsOk_WhenReservationExists()
    {
        // Arrange
        SetUserContext("User", "10");
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
    public async Task GetReservationById_ReturnsForbid_WhenReservationBelongsToAnotherUser()
    {
        // Arrange
        SetUserContext("User", "10");
        var reservation = new ReservationResponseDto
        {
            Id = 2,
            UserId = 20,
            UserName = "anotheruser",
            RoomId = 5,
            RoomName = "Deluxe",
            TotalPrice = 500,
            Status = "Pending"
        };

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(2))
            .ReturnsAsync(reservation);

        // Act
        var result = await _controller.GetReservationById(2);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAllReservations_AdminOnlySeesOwnHotelReservations()
    {
        // Arrange
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "Admin")
    };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "Test"))
            }
        };

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(100);

        _reservationServiceMock
            .Setup(s => s.GetAllReservationsAsync())
            .ReturnsAsync(new List<ReservationResponseDto>
            {
            new ReservationResponseDto { Id = 1, HotelId = 100 },
            new ReservationResponseDto { Id = 2, HotelId = 200 }
            });

        // Act
        var result = await _controller.GetAllReservations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var reservations = Assert.IsAssignableFrom<IEnumerable<ReservationResponseDto>>(
            okResult.Value);

        Assert.Single(reservations);
        Assert.All(reservations, r => Assert.Equal(100, r.HotelId));
    }

    [Fact]
    public async Task UpdateReservation_UserUpdatesOwnReservation_ReturnsOk()
    {
        // Arrange
        SetUserContext("User", "10");
        var id = 1L;
        var checkIn = DateTime.Today.AddDays(10);
        var checkOut = DateTime.Today.AddDays(15);

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(id))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = id,
                UserId = 10,
                RoomId = 5
            });

        _reservationServiceMock
            .Setup(s => s.UpdateReservationAsync(
                id,
                It.IsAny<Reservation>()))
            .ReturnsAsync(new Reservation
            {
                Id = id,
                UserId = 10,
                RoomId = 5,
                CheckInDate = checkIn,
                CheckOutDate = checkOut
            });

        var dto = new UpdateReservationDto
        {
            RoomId = 5,
            CheckInDate = checkIn,
            CheckOutDate = checkOut
        };

        // Act
        var result = await _controller.UpdateReservation(id, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        _reservationServiceMock.Verify(s =>
            s.UpdateReservationAsync(
                id,
                It.Is<Reservation>(r =>
                    r.RoomId == 5 &&
                    r.CheckInDate == checkIn &&
                    r.CheckOutDate == checkOut)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateReservation_UserUpdatesOtherUsersReservation_ReturnsForbid()
    {
        // Arrange
        SetUserContext("User", "10");
        var id = 1L;

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(id))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = id,
                UserId = 20,
                RoomId = 5
            });

        var dto = new UpdateReservationDto
        {
            RoomId = 5,
            CheckInDate = DateTime.Today.AddDays(10),
            CheckOutDate = DateTime.Today.AddDays(15)
        };

        // Act
        var result = await _controller.UpdateReservation(id, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _reservationServiceMock.Verify(s =>
            s.UpdateReservationAsync(
                It.IsAny<long>(),
                It.IsAny<Reservation>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateReservation_InvalidDates_ReturnsBadRequest()
    {
        // Arrange
        var id = 1L;

        var dto = new UpdateReservationDto
        {
            RoomId = 5,
            CheckInDate = DateTime.Today.AddDays(15),
            CheckOutDate = DateTime.Today.AddDays(10)
        };

        _reservationServiceMock
    .Setup(s => s.GetReservationByIdAsync(id))
    .ReturnsAsync(new ReservationResponseDto
    {
        Id = id,
        UserId = 10,
        RoomId = dto.RoomId
    });

        _reservationServiceMock
            .Setup(s => s.UpdateReservationAsync(
                id,
                It.IsAny<Reservation>()))
            .ThrowsAsync(new ArgumentException("Invalid reservation dates."));

        // Act
        var result = await _controller.UpdateReservation(id, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        _reservationServiceMock.Verify(
    s => s.GetReservationByIdAsync(id),
    Times.Once);

        _reservationServiceMock.Verify(
            s => s.UpdateReservationAsync(
                id,
                It.IsAny<Reservation>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteReservation_ReturnsOk_WhenDeleted()
    {
        // Arrange
        SetUserContext("User", "10");
        var id = 1L;

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(id))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = id,
                UserId = 10,
                RoomId = 5
            });

        _reservationServiceMock
            .Setup(s => s.DeleteReservationAsync(id))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteReservation(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var message = okResult.Value!
            .GetType()
            .GetProperty("message")!
            .GetValue(okResult.Value);

        Assert.Equal("Reservation cancelled successfully", message);
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

    [Fact]
    public async Task DeleteReservation_UserDeletesOtherUsersReservation_ReturnsForbid()
    {
        // Arrange
        SetUserContext("User", "10");
        var id = 1L;

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(id))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = id,
                UserId = 20,
                RoomId = 5
            });

        // Act
        var result = await _controller.DeleteReservation(id);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _reservationServiceMock.Verify(s =>
            s.DeleteReservationAsync(It.IsAny<long>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateReservation_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        // Arrange
        SetUserContext("User", "10");
        var dto = new CreateReservationDto
        {
            RoomId = 999,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(10)
        };

        _reservationServiceMock
            .Setup(s => s.CreateReservationAsync(It.IsAny<Reservation>()))
            .ThrowsAsync(new ArgumentException("Room not found"));

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateReservation_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        // Arrange
        SetUserContext("User", "10");
        var dto = new UpdateReservationDto
        {
            RoomId = 999,
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(10)
        };

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(1))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = 1,
                UserId = 10,
                RoomId = 5
            });

        _reservationServiceMock
            .Setup(s => s.UpdateReservationAsync(1, It.IsAny<Reservation>()))
            .ThrowsAsync(new ArgumentException("Room not found"));

        // Act
        var result = await _controller.UpdateReservation(1, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteReservation_AdminCanCancelOwnHotelReservation()
    {
        // Arrange
        SetUserContext("Admin", "1");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(100);

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(1))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = 1,
                UserId = 10,
                RoomId = 5,
                HotelId = 100,
                Status = "Confirmed"
            });

        _reservationServiceMock
            .Setup(s => s.DeleteReservationAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteReservation(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        _reservationServiceMock.Verify(
            s => s.DeleteReservationAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task DeleteReservation_AdminCannotCancelOtherHotelReservation()
    {
        // Arrange
        SetUserContext("Admin", "1");

        _userServiceMock
            .Setup(s => s.GetHotelIdByUserIdAsync(1))
            .ReturnsAsync(100);

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(2))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = 2,
                UserId = 20,
                RoomId = 5,
                HotelId = 200,
                Status = "Confirmed"
            });

        // Act
        var result = await _controller.DeleteReservation(2);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _reservationServiceMock.Verify(
            s => s.DeleteReservationAsync(2),
            Times.Never);
    }

    [Fact]
    public async Task DeleteReservation_SuperAdminCanCancelAnyHotelReservation()
    {
        // Arrange
        SetUserContext("SuperAdmin", "1");

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(2))
            .ReturnsAsync(new ReservationResponseDto
            {
                Id = 2,
                UserId = 20,
                RoomId = 5,
                HotelId = 200,
                Status = "Confirmed"
            });

        _reservationServiceMock
            .Setup(s => s.DeleteReservationAsync(2))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteReservation(2);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        _userServiceMock.Verify(
            s => s.GetHotelIdByUserIdAsync(It.IsAny<long>()),
            Times.Never);

        _reservationServiceMock.Verify(
            s => s.DeleteReservationAsync(2),
            Times.Once);
    }
}
