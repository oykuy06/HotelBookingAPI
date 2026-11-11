
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using HotelBookingAPI.Services.Interfaces;

namespace HotelBookingAPI.Services
{
    public class PasswordHashService : IPasswordHashService
    {
        private const int CurrentIterations = 600_000;
        private const int LegacyIterations = 10_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const string Algorithm = "pbkdf2-sha256";

        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hashed = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: CurrentIterations,
                numBytesRequested: HashSize);

            return $"{Algorithm}${CurrentIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hashed)}";
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                var parts = passwordHash.Split('$');

                if (parts.Length == 4 && parts[0] == Algorithm)
                {
                    if (!int.TryParse(parts[1], out int iterations) ||
                        iterations <= 0 || iterations > 1_000_000)
                    {
                        return false;
                    }

                    return VerifyHash(
                        password,
                        parts[2],
                        parts[3],
                        iterations);
                }

                var legacyParts = passwordHash.Split('.');

                if (legacyParts.Length == 2)
                {
                    return VerifyHash(
                        password,
                        legacyParts[0],
                        legacyParts[1],
                        LegacyIterations);
                }

                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public bool NeedsRehash(string passwordHash)
        {
            var parts = passwordHash.Split('$');

            if (parts.Length == 4 && parts[0] == Algorithm)
            {
                if (!int.TryParse(parts[1], out int iterations))
                    return true;

                return iterations < CurrentIterations;
            }

            // Eski format veya tanınmayan format.
            return true;
        }

        private bool VerifyHash(
            string password,
            string saltValue,
            string storedHashValue,
            int iterations)
        {
            byte[] salt = Convert.FromBase64String(saltValue);
            byte[] storedHash = Convert.FromBase64String(storedHashValue);

            if (salt.Length != SaltSize || storedHash.Length != HashSize)
                return false;

            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: iterations,
                numBytesRequested: HashSize);

            return CryptographicOperations.FixedTimeEquals(
                computedHash,
                storedHash);
        }
    }
}
