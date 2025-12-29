using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class PasswordServiceTest
{
    private readonly HotelBookingDBContext _context;
    private readonly PasswordService _passwordService;

    public PasswordServiceTest()
    {
        // InMemory DB
        var options = new DbContextOptionsBuilder<HotelBookingDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HotelBookingDBContext(options);

        // Fake IConfiguration
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Smtp:Host", "fake"},
                {"Smtp:Port", "587"},
                {"Smtp:Username", "fake"},
                {"Smtp:Password", "fake"},
                {"Smtp:SenderEmail", "test@test.com"}
            })
            .Build();

        _passwordService = new PasswordService(_context, config);
    }

    [Fact]
    public async Task GenerateResetTokenAsync_UserExists_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            Email = "test@test.com",
            Username = "test",
            PasswordHash = "hash",
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var token = await _passwordService.GenerateResetTokenAsync(user.Id);

        // Assert
        Assert.NotNull(token);
        Assert.Single(_context.PasswordResets);
    }

    [Fact]
    public async Task GenerateResetTokenAsync_UserDoesNotExist_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _passwordService.GenerateResetTokenAsync(999));
    }

    [Fact]
    public async Task ValidateUserTokenAsync_ValidToken_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@test.com",
            Username = "test",
            PasswordHash = "hash",
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var reset = new PasswordReset
        {
            Token = "valid-token",
            UserId = user.Id,
            Expiration = DateTime.UtcNow.AddMinutes(30),
            IsUsed = false
        };

        _context.PasswordResets.Add(reset);
        await _context.SaveChangesAsync();

        // Act
        var result = await _passwordService.ValidateUserTokenAsync("valid-token");

        // Assert
        Assert.NotNull(result);
        Assert.True(reset.IsUsed);
    }

    [Fact]
    public async Task ValidateUserTokenAsync_InvalidToken_ReturnsNull()
    {
        // Act
        var result = await _passwordService.ValidateUserTokenAsync("invalid-token");

        // Assert
        Assert.Null(result);
    }
}
