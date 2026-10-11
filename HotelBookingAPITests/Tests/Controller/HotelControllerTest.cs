using HotelBookingAPI.Controllers;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace HotelBookingAPI.Tests.Controllers
{
    public class HotelControllerTest
    {
        private readonly Mock<IHotelService> _hotelServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly HotelController _controller;

        public HotelControllerTest()
        {
            _hotelServiceMock = new Mock<IHotelService>();
            _userServiceMock = new Mock<IUserService>();

            _controller = new HotelController(
                _hotelServiceMock.Object,
                _userServiceMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
            new ClaimsIdentity())
                }
            };
        }

        [Fact]
        public async Task GetAllHotels_ReturnsOkResult()
        {
            // Arrange
            var hotels = new List<HotelResponseDto>
            {
                new HotelResponseDto
                {
                    Id = 1,
                    Name = "Test Hotel",
                    Description = "Test Desc",
                    Address = "Test Address",
                    Photo = "http://photo.com",
                    Rooms = new List<RoomResponseDto>()
                }
            };

            _hotelServiceMock
                .Setup(s => s.GetAllHotelsAsync())
                .ReturnsAsync(hotels);

            // Act
            var result = await _controller.GetAllHotels();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedHotels = Assert.IsAssignableFrom<IEnumerable<HotelResponseDto>>(okResult.Value);
            Assert.Single(returnedHotels);
        }

        [Fact]
        public async Task GetHotelById_WhenHotelExists_ReturnsOk()
        {
            // Arrange
            var hotel = new HotelResponseDto
            {
                Id = 1,
                Name = "Test Hotel",
                Description = "Desc",
                Address = "Address",
                Photo = "photo",
                Rooms = new List<RoomResponseDto>()
            };

            _hotelServiceMock
                .Setup(s => s.GetHotelByIdAsync(1))
                .ReturnsAsync(hotel);

            // Act
            var result = await _controller.GetHotelById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedHotel = Assert.IsType<HotelResponseDto>(okResult.Value);
            Assert.Equal(1, returnedHotel.Id);
        }

        [Fact]
        public async Task GetHotelById_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            _hotelServiceMock
                .Setup(s => s.GetHotelByIdAsync(It.IsAny<long>()))
                .ReturnsAsync((HotelResponseDto?)null);

            // Act
            var result = await _controller.GetHotelById(99);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateHotel_ReturnsCreatedAtAction()
        {
            // Arrange
            var hotel = new Hotel
            {
                Id = 1,
                Name = "Created Hotel",
                Description = "Desc",
                Address = "Address",
                Photo = "photo"
            };

            _hotelServiceMock
                .Setup(s => s.CreateHotelAsync(It.IsAny<Hotel>()))
                .ReturnsAsync(hotel);

            // Act
            var result = await _controller.CreateHotel(new HotelBookingAPI.Dto.RequestDto.HotelDto
            {
                Name = "Created Hotel",
                Description = "Desc",
                Address = "Address",
                Photo = "photo"
            });

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var value = Assert.IsType<HotelResponseDto>(createdResult.Value);
            Assert.Equal("Created Hotel", value.Name);
        }

        [Fact]
        public async Task UpdateHotel_WhenHotelExists_ReturnsOk()
        {
            // Arrange
            _hotelServiceMock
                .Setup(s => s.GetHotelByIdAsync(1))
                .ReturnsAsync(new HotelResponseDto
                {
                    Id = 1,
                    Name = "Test Hotel",
                    Description = "Old Description",
                    Address = "Old Address",
                    Photo = "old-photo",
                    Rooms = new List<RoomResponseDto>()
                });

            _hotelServiceMock
                .Setup(s => s.UpdateHotelAsync(
                    1,
                    It.IsAny<Hotel>()))
                .ReturnsAsync(new Hotel
                {
                    Id = 1,
                    Name = "Updated Hotel",
                    Description = "Updated Description",
                    Address = "Updated Address",
                    Photo = "updated-photo"
                });

            var dto = new HotelBookingAPI.Dto.RequestDto.HotelDto
            {
                Name = "Updated Hotel",
                Description = "Updated Description",
                Address = "Updated Address",
                Photo = "updated-photo"
            };

            // Act
            var result = await _controller.UpdateHotel(1, dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateHotel_WhenAdminTriesOtherHotel_ReturnsForbid()
        {
            // Arrange
            var identity = new ClaimsIdentity(
                new[]
                {
            new Claim(ClaimTypes.NameIdentifier, "9"),
            new Claim(ClaimTypes.Role, "Admin")
                },
                "TestAuth");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            _hotelServiceMock
                .Setup(s => s.GetHotelByIdAsync(2))
                .ReturnsAsync(new HotelResponseDto
                {
                    Id = 2,
                    Name = "Another Hotel",
                    Description = "Description",
                    Address = "Address",
                    Photo = "photo",
                    Rooms = new List<RoomResponseDto>()
                });

            _userServiceMock
                .Setup(s => s.GetHotelIdByUserIdAsync(9))
                .ReturnsAsync((long?)1);

            var dto = new HotelBookingAPI.Dto.RequestDto.HotelDto
            {
                Name = "Updated Hotel",
                Description = "Updated Description",
                Address = "Updated Address",
                Photo = "updated-photo"
            };

            // Act
            var result = await _controller.UpdateHotel(2, dto);

            // Assert
            Assert.IsType<ForbidResult>(result);

            _hotelServiceMock.Verify(
                s => s.UpdateHotelAsync(2, It.IsAny<Hotel>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateHotel_WhenSuperAdminUpdatesOtherHotel_ReturnsOk()
        {
            // Arrange
            var identity = new ClaimsIdentity(
                new[]
                {
            new Claim(ClaimTypes.NameIdentifier, "10"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
                },
                "TestAuth");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            _hotelServiceMock
                .Setup(s => s.GetHotelByIdAsync(2))
                .ReturnsAsync(new HotelResponseDto
                {
                    Id = 2,
                    Name = "Another Hotel",
                    Description = "Description",
                    Address = "Address",
                    Photo = "photo",
                    Rooms = new List<RoomResponseDto>()
                });

            _hotelServiceMock
                .Setup(s => s.UpdateHotelAsync(2, It.IsAny<Hotel>()))
                .ReturnsAsync(new Hotel
                {
                    Id = 2,
                    Name = "Updated Hotel",
                    Description = "Updated Description",
                    Address = "Updated Address",
                    Photo = "updated-photo"
                });

            var dto = new HotelBookingAPI.Dto.RequestDto.HotelDto
            {
                Name = "Updated Hotel",
                Description = "Updated Description",
                Address = "Updated Address",
                Photo = "updated-photo"
            };

            // Act
            var result = await _controller.UpdateHotel(2, dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteHotel_WhenAdminTriesOtherHotel_ReturnsForbid()
        {
            // Arrange
            var identity = new ClaimsIdentity(
                new[]
                {
            new Claim(ClaimTypes.NameIdentifier, "9"),
            new Claim(ClaimTypes.Role, "Admin")
                },
                "TestAuth");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            _hotelServiceMock
                .Setup(s => s.GetHotelByIdAsync(2))
                .ReturnsAsync(new HotelResponseDto
                {
                    Id = 2,
                    Name = "Another Hotel",
                    Description = "Description",
                    Address = "Address",
                    Photo = "photo",
                    Rooms = new List<RoomResponseDto>()
                });

            _userServiceMock
                .Setup(s => s.GetHotelIdByUserIdAsync(9))
                .ReturnsAsync((long?)1);

            // Act
            var result = await _controller.DeleteHotel(2);

            // Assert
            Assert.IsType<ForbidResult>(result);

            _hotelServiceMock.Verify(
                s => s.DeleteHotelAsync(2),
                Times.Never);
        }

        [Fact]
        public async Task DeleteHotel_WhenSuperAdminDeletesOtherHotel_ReturnsOk()
        {
            // Arrange
            var identity = new ClaimsIdentity(
                new[]
                {
            new Claim(ClaimTypes.NameIdentifier, "10"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
                },
                "TestAuth");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            _hotelServiceMock
                .Setup(s => s.GetHotelByIdAsync(2))
                .ReturnsAsync(new HotelResponseDto
                {
                    Id = 2,
                    Name = "Another Hotel",
                    Description = "Description",
                    Address = "Address",
                    Photo = "photo",
                    Rooms = new List<RoomResponseDto>()
                });

            _hotelServiceMock
                .Setup(s => s.DeleteHotelAsync(2))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteHotel(2);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            _hotelServiceMock.Verify(
                s => s.DeleteHotelAsync(2),
                Times.Once);
        }

        [Fact]
        public async Task DeleteHotel_WhenSuccess_ReturnsOk()
        {
            // Arrange
            _hotelServiceMock
                .Setup(s => s.GetHotelByIdAsync(1))
                .ReturnsAsync(new HotelResponseDto
                {
                    Id = 1,
                    Name = "Test Hotel"
                });

            _hotelServiceMock
                .Setup(s => s.DeleteHotelAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteHotel(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteHotel_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            _hotelServiceMock
                .Setup(s => s.GetHotelByIdAsync(99))
                .ReturnsAsync((HotelResponseDto?)null);

            // Act
            var result = await _controller.DeleteHotel(99);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);

            _hotelServiceMock.Verify(
                s => s.DeleteHotelAsync(99),
                Times.Never);
        }
    }
}
