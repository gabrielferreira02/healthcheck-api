using System;

namespace HealthCheckApi.Entity;

public class UserEntity
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

    public void UpdateUsername(string username) {
        Username = username;
    }

    public void UpdateEmail(string email) {
        Email = email;
    }
}
