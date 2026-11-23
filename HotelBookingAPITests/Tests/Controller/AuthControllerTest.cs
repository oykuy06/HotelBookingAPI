using HotelBookingAPI.Controllers;
using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace HotelBookingAPI.Tests.Controllers
{
    public class AuthControllerTest
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;
        private readonly Mock<ILogger<AuthController>> _loggerMock;

        public AuthControllerTest()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<AuthController>>();

            _controller = new AuthController(
                _authServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact] //not parameter
        public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
        {
            // arrange
            var dto = new RegisterDto
            {
                Email = "test@test.com",
                Password = "123456"
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
            var dto = new RegisterDto
            {
                Email = "test@test.com",
                Password = "123456"
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
            var dto = new RefreshTokenDto
            {
                RefreshToken = "invalid"
            };

            _authServiceMock
                .Setup(x => x.RefreshAccessTokenAsync(dto.RefreshToken))
                .ReturnsAsync((AuthResponseDto?)null);

            var result = await _controller.RefreshToken(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RefreshToken_Valid_ReturnsOk()
        {
            var dto = new RefreshTokenDto
            {
                RefreshToken = "valid"
            };

            _authServiceMock
                .Setup(x => x.RefreshAccessTokenAsync(dto.RefreshToken))
                .ReturnsAsync(new AuthResponseDto
                {
                    AccessToken = "newAccessToken",
                    RefreshToken = "newRefreshToken"
                });

            var result = await _controller.RefreshToken(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_UserExists_ReturnsOk()
        {
            // Arrange
            var dto = new ForgotPasswordDto
            {
                Email = "user@test.com"
            };

            _authServiceMock
                .Setup(s => s.ForgotPasswordAsync(dto.Email))
                .ReturnsAsync("reset-token");

            // Act
            var result = await _controller.ForgotPassword(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response = Assert.IsType<string>(
                okResult.Value.GetType()
                    .GetProperty("message")!
                    .GetValue(okResult.Value));

            Assert.Equal(
                "If the account exists, a reset link has been sent.",
                response);
        }

        [Fact]
        public async Task ForgotPassword_EmailSendingFails_ReturnsOk()
        {
            // Arrange
            var dto = new ForgotPasswordDto
            {
                Email = "user@test.com"
            };

            _authServiceMock
                .Setup(s => s.ForgotPasswordAsync(dto.Email))
                .ThrowsAsync(new Exception("SMTP connection failed"));

            // Act
            var result = await _controller.ForgotPassword(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("Failed to send password reset email.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_UserDoesNotExist_ReturnsOk()
        {
            // Arrange
            var dto = new ForgotPasswordDto
            {
                Email = "unknown@test.com"
            };

            _authServiceMock
                .Setup(s => s.ForgotPasswordAsync(dto.Email))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _controller.ForgotPassword(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response = Assert.IsType<string>(
                okResult.Value.GetType()
                    .GetProperty("message")!
                    .GetValue(okResult.Value));

            Assert.Equal(
                "If the account exists, a reset link has been sent.",
                response);
        }
    }
}
