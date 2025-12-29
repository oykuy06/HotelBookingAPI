using HotelBookingAPI.Controllers;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HotelBookingAPI.Tests.Controllers
{
    public class AuthControllerTest
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTest()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Fact] //not parameter
        public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
        {
            // arrange
            var dto = new UserDto
            {
                Email = "test@test.com",
                Password = "123456",
                Role = "User"
            };

            _authServiceMock
                .Setup(x => x.RegisterAsync(dto))
                .ReturnsAsync((User?)null);

            // act
            var result = await _controller.Register(dto);

            // assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_Success_ReturnsOk()
        {
            var dto = new UserDto
            {
                Email = "test@test.com",
                Password = "123456",
                Role = "User"
            };

            _authServiceMock
                .Setup(x => x.RegisterAsync(dto))
                .ReturnsAsync(new User { Id = 1 });

            var result = await _controller.Register(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var dto = new LoginDto
            {
                Email = "test@test.com",
                Password = "wrong"
            };

            _authServiceMock
                .Setup(x => x.LoginAsync(dto))
                .ReturnsAsync((AuthResponseDto?)null);

            var result = await _controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var dto = new LoginDto
            {
                Email = "test@test.com",
                Password = "123456"
            };

            _authServiceMock
                .Setup(x => x.LoginAsync(dto))
                .ReturnsAsync(new AuthResponseDto
                {
                    AccessToken = "access",
                    RefreshToken = "refresh"
                });

            var result = await _controller.Login(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task RefreshToken_Invalid_ReturnsUnauthorized()
        {
            _authServiceMock
                .Setup(x => x.RefreshAccessTokenAsync("invalid"))
                .ReturnsAsync((string?)null);

            var result = await _controller.RefreshToken("invalid");

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RefreshToken_Valid_ReturnsOk()
        {
            _authServiceMock
                .Setup(x => x.RefreshAccessTokenAsync("valid"))
                .ReturnsAsync("newAccessToken");

            var result = await _controller.RefreshToken("valid");

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
