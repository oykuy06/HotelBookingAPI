using HotelBookingAPI.Entity;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace HotelBookingAPI.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly HotelBookingDBContext _context;
        private readonly IConfiguration _config;

        public PasswordService(HotelBookingDBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string> GenerateResetTokenAsync(long userId)
        {
            var userExist = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExist)
                throw new ArgumentException("User not found");

            var token = Guid.NewGuid().ToString();

            var previousTokens = await _context.PasswordResets
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ToListAsync();

            foreach (var previousToken in previousTokens)
            {
                previousToken.IsUsed = true;
            }

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

        public async Task<PasswordReset?> GetValidResetTokenAsync(string token)
        {
            return await _context.PasswordResets
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    !t.IsUsed &&
                    t.Expiration > DateTime.UtcNow);
        }

        public async Task SendResetEmailAsync(string email, string token)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];
            var senderEmail = _config["Smtp:SenderEmail"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(smtpUser) ||
                string.IsNullOrWhiteSpace(smtpPass) ||
                string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new InvalidOperationException("SMTP settings are incomplete.");
            }

            if (!int.TryParse(_config["Smtp:Port"], out var smtpPort))
                throw new InvalidOperationException("SMTP port is not configured correctly.");

            var resetPasswordUrl = _config["Frontend:ResetPasswordUrl"];

            if (string.IsNullOrWhiteSpace(resetPasswordUrl))
                throw new InvalidOperationException("Frontend reset password URL is not configured.");

            var separator = resetPasswordUrl.Contains('?') ? "&" : "?";
            var resetLink = $"{resetPasswordUrl}{separator}token={Uri.EscapeDataString(token)}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hotel Booking", senderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Password Reset Request";
            message.Body = new TextPart("html")
            {
                Text = $"<p>To reset your password, click the link below:</p>" +
                       $"<a href='{resetLink}'>{resetLink}</a>"
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
