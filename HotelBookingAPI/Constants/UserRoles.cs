namespace HotelBookingAPI.Constants
{
    public static class UserRoles
    {
        public const string User = "User";
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";

        public static string Normalize(string? role)
        {
            return role?.Trim().ToLowerInvariant() switch
            {
                "user" => User,
                "admin" => Admin,
                "superadmin" => SuperAdmin,
                _ => throw new ArgumentException("Invalid role", nameof(role))
            };
        }
    }
}