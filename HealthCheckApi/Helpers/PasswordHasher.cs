namespace HealthCheckApi.Helpers;
using BCrypt.Net;

public static class PasswordHasher
{
    public static string HashPassword(string password) {
        return BCrypt.HashPassword(password, BCrypt.GenerateSalt(12));
    }
}
