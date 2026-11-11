
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
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
            Assert.Contains("pbkdf2-sha256$600000$", hashedPassword);
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

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenUsingLegacyHash()
        {
            // Arrange
            var password = "Test123!";
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            var legacyHash = $"{Convert.ToBase64String(salt)}.{hash}";

            // Act
            var result = _passwordHashService.VerifyPassword(password, legacyHash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void NeedsRehash_ShouldReturnTrue_WhenUsingLegacyHash()
        {
            // Arrange
            var password = "Test123!";
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            var legacyHash = $"{Convert.ToBase64String(salt)}.{hash}";

            // Act
            var result = _passwordHashService.NeedsRehash(legacyHash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void NeedsRehash_ShouldReturnFalse_WhenUsingCurrentHash()
        {
            // Arrange
            var hashedPassword = _passwordHashService.HashPassword("Test123!");

            // Act
            var result = _passwordHashService.NeedsRehash(hashedPassword);

            // Assert
            Assert.False(result);
        }
    }
}
