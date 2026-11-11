using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

public class AuthServiceTest
{
    private readonly HotelBookingDBContext _context;
    private readonly Mock<IPasswordHashService> _passwordHashServiceMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTest()
    {
        // InMemory DB
        var options = new DbContextOptionsBuilder<HotelBookingDBContext>()
            .UseInMemoryDatabase(databaseName: "AuthServiceTestDb")
            .Options;

        _context = new HotelBookingDBContext(options);

        // Mock services
        _passwordHashServiceMock = new Mock<IPasswordHashService>();
        _passwordServiceMock = new Mock<IPasswordService>();

        // JWT config 
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Jwt:Key", "super_secret_test_key_1234567890!" },
            { "Jwt:Issuer", "HotelBookingAPI" },
            { "Jwt:Audience", "HotelBookingAPIUsers" },
            { "Jwt:ExpiresInHours", "2" },
            { "Jwt:RefreshTokenExpiresInDays", "7" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _authService = new AuthService(
            _context,
            _configuration,
            _passwordHashServiceMock.Object,
            _passwordServiceMock.Object
        );
    }

    // REGISTER TEST
    [Fact]
    public async Task RegisterAsync_NewUser_ShouldCreateUser()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "test@test.com",
            Password = "123456"
        };

        _passwordHashServiceMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("hashed_password", result.PasswordHash);
    }


    [Fact]
    public async Task RegisterAsync_AdminRoleRequested_ShouldCreateUserWithUserRole()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "admin@test.com",
            Password = "123456"
        };

        _passwordHashServiceMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("User", result.Role);
    }


    // LOGIN TEST
    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var user = new User
        {
            Email = "login@test.com",
            Username = "loginuser",
            PasswordHash = "hashed_password",
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _passwordHashServiceMock
            .Setup(x => x.VerifyPassword("123456", "hashed_password"))
            .Returns(true);

        var dto = new LoginDto
        {
            Email = "login@test.com",
            Password = "123456"
        };

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    // LOGIN FAIL TEST
    [Fact]
    public async Task LoginAsync_WrongPassword_ShouldReturnNull()
    {
        // Arrange
        var user = new User
        {
            Email = "fail@test.com",
            Username = "loginuser",
            PasswordHash = "hashed_password",
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _passwordHashServiceMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var dto = new LoginDto
        {
            Email = "fail@test.com",
            Password = "wrongpassword"
        };

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task LoginAsync_LegacyPasswordHash_ShouldRehashPassword()
    {
        // Arrange
        var user = new User
        {
            Email = "legacy@test.com",
            Username = "legacyuser",
            PasswordHash = "legacy_hash",
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _passwordHashServiceMock
            .Setup(x => x.VerifyPassword("123456", "legacy_hash"))
            .Returns(true);

        _passwordHashServiceMock
            .Setup(x => x.NeedsRehash("legacy_hash"))
            .Returns(true);

        _passwordHashServiceMock
            .Setup(x => x.HashPassword("123456"))
            .Returns("pbkdf2-sha256$600000$new_salt$new_hash");

        var dto = new LoginDto
        {
            Email = "legacy@test.com",
            Password = "123456"
        };

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(
            "pbkdf2-sha256$600000$new_salt$new_hash",
            user.PasswordHash);

        _passwordHashServiceMock.Verify(
            x => x.HashPassword("123456"),
            Times.Once);
    }

}
