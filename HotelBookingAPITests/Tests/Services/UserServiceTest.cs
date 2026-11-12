using HotelBookingAPI.Constants;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
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

            public bool NeedsRehash(string passwordHash)
            {
                return false;
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
            var users = await service.GetAllUsersAsync(null);

            // Assert
            Assert.Equal(2, users.Count());
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            using var context = CreateDbContext();

            var hotel = new Hotel
            {
                Name = "Test Hotel",
                Description = "Test Description",
                Address = "Test Address",
                Photo = "test.jpg"
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

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
                role: "Admin",
                hotelId: hotel.Id
            );

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal("new@mail.com", updatedUser.Email);
            Assert.Equal("Admin", updatedUser.Role);
            Assert.Equal(hotel.Id, updatedUser.HotelId);
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

        [Theory]
        [InlineData("admin")]
        [InlineData("ADMIN")]
        [InlineData(" Admin ")]
        [InlineData("superadmin")]
        [InlineData("USER")]
        public async Task CreateUserAsync_ShouldNormalizeRole(string role)
        {
            // Arrange
            // Arrange
            using var context = CreateDbContext();
            var service = CreateUserService(context);

            var hotel = new Hotel
            {
                Name = "Test Hotel",
                Description = "Test Description",
                Address = "Test Address",
                Photo = "test.jpg"
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            long? hotelId = UserRoles.Normalize(role) == UserRoles.Admin
                ? hotel.Id
                : null;

            // Act
            var user = await service.CreateUserAsync(
    email: "test@mail.com",
    password: "123456",
    role: role,
    hotelId: hotelId
);

            // Assert
            var expectedRole = role.Trim().ToLowerInvariant() switch
            {
                "admin" => "Admin",
                "superadmin" => "SuperAdmin",
                "user" => "User",
                _ => throw new InvalidOperationException()
            };

            Assert.Equal(expectedRole, user.Role);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldNormalizeRole()
        {
            // Arrange
            using var context = CreateDbContext();

            var hotel = new Hotel
            {
                Name = "Test Hotel",
                Description = "Test Description",
                Address = "Test Address",
                Photo = "test.jpg"
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

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
                email: "old@mail.com",
                role: " admin ",
                hotelId: hotel.Id
            );

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal("Admin", updatedUser.Role);
            Assert.Equal(hotel.Id, updatedUser.HotelId);
        }

        [Fact]
        public async Task GetAllUsersAsync_FiltersByHotelId()
        {
            // Arrange
            using var context = CreateDbContext();

            var users = new List<User>
    {
        new User
        {
            Email = "user1@test.com",
            Username = "user1",
            PasswordHash = "hash",
            Role = UserRoles.User,
            HotelId = 1
        },
        new User
        {
            Email = "user2@test.com",
            Username = "user2",
            PasswordHash = "hash",
            Role = UserRoles.User,
            HotelId = 2
        }
    };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            var service = CreateUserService(context);

            // Act
            var result = await service.GetAllUsersAsync(1);

            // Assert
            var userList = result.ToList();

            Assert.Single(userList);
            Assert.Equal("user1@test.com", userList[0].Email);
        }

        

    }
}
