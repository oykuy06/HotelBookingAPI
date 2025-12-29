using HotelBookingAPI.Controllers;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HotelBookingAPI.Tests.Controllers
{
    public class HotelControllerTest
    {
        private readonly Mock<IHotelService> _hotelServiceMock;
        private readonly HotelController _controller;

        public HotelControllerTest()
        {
            _hotelServiceMock = new Mock<IHotelService>();
            _controller = new HotelController(_hotelServiceMock.Object);
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
        public async Task DeleteHotel_WhenSuccess_ReturnsOk()
        {
            // Arrange
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
                .Setup(s => s.DeleteHotelAsync(99))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteHotel(99);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
