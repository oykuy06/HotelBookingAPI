using HotelBookingAPI.Services;
using Xunit;

namespace HotelBookingAPI.Tests.Services
{
    public class PasswordHashServiceTest
    {
        private readonly PasswordHashService _passwordHashService;

        public PasswordHashServiceTest()
        {
            _passwordHashService = new PasswordHashService();
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedValue()
        {
            // Arrange
            var password = "Test123!";

            // Act
            var hashedPassword = _passwordHashService.HashPassword(password);

            // Assert
            Assert.False(string.IsNullOrEmpty(hashedPassword));
            Assert.Contains(".", hashedPassword); // salt.hash format
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordIsCorrect()
        {
            // Arrange
            var password = "Test123!";
            var hashedPassword = _passwordHashService.HashPassword(password);

            // Act
            var result = _passwordHashService.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsWrong()
        {
            // Arrange
            var password = "Test123!";
            var wrongPassword = "WrongPassword!";
            var hashedPassword = _passwordHashService.HashPassword(password);

            // Act
            var result = _passwordHashService.VerifyPassword(wrongPassword, hashedPassword);

            // Assert
            Assert.False(result);
        }
    }
}
