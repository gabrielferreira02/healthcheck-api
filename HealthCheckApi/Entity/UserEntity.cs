using System;

namespace HealthCheckApi.Entity;

internal class UserEntity
{
    public Guid Id { get; init; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public DateTime CreatedAt { get; init; }

    public UserEntity(
        string username,
        string email,
        string password)
    {
        Id = Guid.NewGuid();
        Username = username;
        Email = email;
        Password = password;
        CreatedAt = DateTime.Now;
    }
}
