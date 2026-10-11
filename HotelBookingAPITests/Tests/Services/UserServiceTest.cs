using System;
using System.Linq;
using System.Threading.Tasks;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services;
using HotelBookingAPI.Services.Interfaces;
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

        private UserService CreateUserService(HotelBookingDBContext context)
        {
            var passwordHashService = new TestPasswordHashService();

            return new UserService(context, passwordHashService);
        }

        private class TestPasswordHashService : IPasswordHashService
        {
            public string HashPassword(string password)
            {
                return "hashed_" + password;
            }

            public bool VerifyPassword(string password, string passwordHash)
            {
                return passwordHash == "hashed_" + password;
            }
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser()
        {
            // Arrange
            using var context = CreateDbContext();
            var service = CreateUserService(context);

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
            Assert.Equal("hashed_123456", user.PasswordHash);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Email = "test@mail.com",
                Username = "test",
                PasswordHash = "hash",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateUserService(context);

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
            using var context = CreateDbContext();
            var service = CreateUserService(context);

            // Act
            var result = await service.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsers()
        {
            // Arrange
            using var context = CreateDbContext();

            context.Users.AddRange(
                new User
                {
                    Email = "a@mail.com",
                    Username = "a",
                    PasswordHash = "x",
                    Role = "User"
                },
                new User
                {
                    Email = "b@mail.com",
                    Username = "b",
                    PasswordHash = "y",
                    Role = "Admin"
                }
            );

            await context.SaveChangesAsync();

            var service = CreateUserService(context);

            // Act
            var users = await service.GetAllUsersAsync();

            // Assert
            Assert.Equal(2, users.Count());
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Email = "old@mail.com",
                Username = "old",
                PasswordHash = "hash",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateUserService(context);

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
            Assert.Equal("hashed_newpass", updatedUser.PasswordHash);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            using var context = CreateDbContext();

            var user = new User
            {
                Email = "delete@mail.com",
                Username = "delete",
                PasswordHash = "hash",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateUserService(context);

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
            using var context = CreateDbContext();
            var service = CreateUserService(context);

            // Act
            var result = await service.DeleteUserAsync(999);

            // Assert
            Assert.False(result);
        }
    }
}
