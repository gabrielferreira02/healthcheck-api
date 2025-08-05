namespace HealthCheckApi.Tests.Repository;

using Bogus;
using HealthCheckApi.Data;
using HealthCheckApi.Entity;
using Microsoft.EntityFrameworkCore;

public class UserRepositoryTest
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ShouldCreateUser()
    {
        var context = CreateContext();
        var faker = new Faker();

        var user = new UserEntity(
            faker.Name.FullName(),
            faker.Internet.Email(),
            faker.Internet.Password()
        );
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var userCreated = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(userCreated);
    }

    [Fact]
    public async Task ShouldDeleteUser()
    {
        var context = CreateContext();
        var faker = new Faker();

        var newName = faker.Name.FullName();
        var user = new UserEntity(
            faker.Name.FullName(),
            faker.Internet.Email(),
            faker.Internet.Password()
        );
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var userCreated = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        Assert.NotNull(userCreated);
        context.Users.Remove(userCreated);
        await context.SaveChangesAsync();
        var users = await context.Users.ToListAsync();

        Assert.Empty(users);
    }

    [Fact]
    public async Task ShouldGetUserById()
    {
        var context = CreateContext();
        var faker = new Faker();

        var newName = faker.Name.FullName();
        var user = new UserEntity(
            faker.Name.FullName(),
            faker.Internet.Email(),
            faker.Internet.Password()
        );
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var userCreated = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        Assert.NotNull(userCreated);
        Assert.Equal(user.Email, userCreated.Email);
        Assert.Equal(user.Username, userCreated.Username);
    }

    [Fact]
    public async Task ShouldGetUserNotFoundById()
    {
        var context = CreateContext();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == Guid.NewGuid());

        Assert.Null(user);
    }

    [Fact]
    public async Task ShouldGetUserByEmail()
    {
        var context = CreateContext();
        var faker = new Faker();

        var newName = faker.Name.FullName();
        var user = new UserEntity(
            faker.Name.FullName(),
            faker.Internet.Email(),
            faker.Internet.Password()
        );
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var userCreated = await context.Users.FirstOrDefaultAsync(u => u.Email.Equals(user.Email));

        Assert.NotNull(userCreated);
        Assert.Equal(user.Id, userCreated.Id);
        Assert.Equal(user.Username, userCreated.Username);
    }

    [Fact]
    public async Task ShouldGetUserNotFoundByEmail()
    {
        var context = CreateContext();
        var faker = new Faker();
        string email = faker.Internet.Email();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email));

        Assert.Null(user);
    }
}
