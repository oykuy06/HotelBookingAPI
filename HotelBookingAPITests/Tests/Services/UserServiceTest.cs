using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelBookingAPI.Tests.Services
{
    public class UserServiceTest
    {
        private HotelBookingDBContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<HotelBookingDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new HotelBookingDBContext(options);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new UserService(context);

            // Act
            var user = await service.CreateUserAsync(
                email: "test@mail.com",
                password: "123456",
                role: "User"
            );

            // Assert
            Assert.NotNull(user);
            Assert.Equal("test@mail.com", user.Email);
            Assert.Equal("User", user.Role);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var context = CreateDbContext();

            var user = new User
            {
                Email = "test@mail.com",
                Username = "test",
                PasswordHash = "hash",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);

            // Act
            var result = await service.GetUserByIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new UserService(context);

            // Act
            var result = await service.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsers()
        {
            // Arrange
            var context = CreateDbContext();

            context.Users.AddRange(
                new User { Email = "a@mail.com", Username = "a", PasswordHash = "x", Role = "User" },
                new User { Email = "b@mail.com", Username = "b", PasswordHash = "y", Role = "Admin" }
            );

            await context.SaveChangesAsync();

            var service = new UserService(context);

            // Act
            var users = await service.GetAllUsersAsync();

            // Assert
            Assert.Equal(2, users.Count());
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var context = CreateDbContext();

            var user = new User
            {
                Email = "old@mail.com",
                Username = "old",
                PasswordHash = "hash",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);

            // Act
            var updatedUser = await service.UpdateUserAsync(
                user.Id,
                email: "new@mail.com",
                password: "newpass",
                role: "Admin"
            );

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal("new@mail.com", updatedUser.Email);
            Assert.Equal("Admin", updatedUser.Role);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            var context = CreateDbContext();

            var user = new User
            {
                Email = "delete@mail.com",
                Username = "delete",
                PasswordHash = "hash",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);

            // Act
            var result = await service.DeleteUserAsync(user.Id);

            // Assert
            Assert.True(result);
            Assert.Empty(context.Users);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new UserService(context);

            // Act
            var result = await service.DeleteUserAsync(999);

            // Assert
            Assert.False(result);
        }
    }
}
