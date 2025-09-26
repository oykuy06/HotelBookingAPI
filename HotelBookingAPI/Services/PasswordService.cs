using HotelBookingAPI.Data;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace HotelBookingAPI.Services
{
    public class PasswordService
    {
        private readonly HotelBookingContext _context;

        public PasswordService(HotelBookingContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateResetToken(int userId)
        {
            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordReset
            {
                Token = token,
                UserId = userId,
                Expiration = DateTime.UtcNow.AddHours(1),
            };

            _context.PasswordReset.Add(resetToken);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<User> ValidateUserToken(string token)
        {
            var resetToken = await _context.PasswordReset
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.Expiration > DateTime.UtcNow);

            if (resetToken == null)
                return null;

            resetToken.IsUsed = true;
            await _context.SaveChangesAsync();

            return resetToken.User;
        }

        public async Task SendResetEmail(string email, string token)
        {
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
}
