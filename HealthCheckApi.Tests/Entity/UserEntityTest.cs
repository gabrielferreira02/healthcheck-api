using Bogus;

namespace HealthCheckApi.Entity;


public class UserEntityTest
{
    [Fact]
    public void ShouldCreateUserEntityWithSuccess()
    {
        // Given
        var faker = new Faker<UserEntity>()
            .CustomInstantiator(f => new UserEntity(
                f.Name.FullName(),
                f.Internet.Email(),
                f.Internet.Password()
            ));

        // When
        var user = faker.Generate();

        // Then
        Assert.NotNull(user);
        Assert.Contains("@", user.Email);
        Assert.NotEmpty(user.Username);
        Assert.NotEmpty(user.Password);
        Assert.NotEqual(default, user.CreatedAt);
    }

    [Fact]
    public void ShouldFailOnCreateUserEntityWithUsernameEmpty()
    {
        var faker = new Faker();

        Assert.Throws<ArgumentException>(() =>
        {
            var user = new UserEntity("", faker.Internet.Email(), faker.Internet.Password());
        });
    }

    [Fact]
    public void ShouldFailOnCreateUserEntityWithEmailEmpty()
    {
        var faker = new Faker();

        Assert.Throws<ArgumentException>(() =>
        {
            var user = new UserEntity(faker.Name.FullName(), "", faker.Internet.Password());
        });
    }

    [Fact]
    public void ShouldFailOnCreateUserEntityWithPasswordEmpty()
    {
        var faker = new Faker();

        Assert.Throws<ArgumentException>(() =>
        {
            var user = new UserEntity(faker.Name.FullName(), faker.Internet.Email(), "");
        });
    }

    [Fact]
    public void ShouldUpdateUsernameWithSuccess()
    {
        var faker = new Faker();

        string newUsername = faker.Name.FullName();
        var user = new UserEntity(faker.Name.FirstName(), faker.Internet.Email(), faker.Internet.Password());

        user.UpdateUsername(newUsername);

        Assert.Equal(newUsername, user.Username);
    }

    [Fact]
    public void ShouldUpdateEmaillWithSuccess()
    {
        var faker = new Faker();

        string newEmail = faker.Internet.Email();
        var user = new UserEntity(faker.Name.FirstName(), faker.Internet.Email(), faker.Internet.Password());

        user.UpdateEmail(newEmail);

        Assert.Equal(newEmail, user.Email);
    }
}
