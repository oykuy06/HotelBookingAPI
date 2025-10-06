using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
namespace HotelBookingAPI.Services.Interfaces;

public class PasswordService : IPasswordService
{
    private readonly HotelBookingDBContext _context;
    private readonly IConfiguration _config;

    public PasswordService(HotelBookingDBContext context, IConfiguration config)
    {
        _context = context;
        _config = config; //SMTP
    }

    public async Task<string> GenerateResetTokenAsync(long userId)
    {

        var userExist = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExist)
        {
            throw new ArgumentException("User not found");
        }

        var token = Guid.NewGuid().ToString();

        var resetToken = new PasswordReset
        {
            Token = token,
            UserId = userId,
            Expiration = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };

        _context.PasswordResets.Add(resetToken);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task<User?> ValidateUserTokenAsync(string token)
    {
        var resetToken = await _context.PasswordResets
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.Expiration > DateTime.UtcNow);

        if (resetToken == null)
            return null;

        resetToken.IsUsed = true;
        await _context.SaveChangesAsync();

        return resetToken.User;
    }

    public async Task SendResetEmailAsync(string email, string token)
    {
        var smtpHost = _config["Smtp:Host"];
        var stmpPort = int.Parse(_config["Stmp:Port"]);
        var stmpUser = _config["Smtp:Username"];
        var smtpPass = _config["Stmp:Password"];
        var senderEmail = _config["Smtp:SenderEmail"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Hotel Booking", "noreply@hotelbooking.com"));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Password Reset Request";

        var resetLink = $"https://frontend/reset-password?token={token}";
        message.Body = new TextPart("plain")
        {
            Text = $"To reset your password, click the following link: {resetLink}"
        };

        using (var client = new SmtpClient()) //SMTP user info
        {
            await client.ConnectAsync("smtp.yourmailserver.com", 587, false);
            await client.AuthenticateAsync("username", "password"); 
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
