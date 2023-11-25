using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using web_signature_web_api.Migrations;
using web_signature_web_api.Models;

namespace signature.Tests.Content;

public static class Helper
{
    public static User newUser = new User
    {
        First_name = "usr1",
        Last_name = "test1",
        Email = "tes1t@test.net",
        Password = "NgB9iXzrfOx7DYZLzBnhS3Uz9hdUJZygkuN2R28VUVvOhpXX1"
    };

    public static User userMail = new User
    {
        First_name = "usr1",
        Last_name = "test1",
        Email = "test@test.net",
        Password = "NgB9iXzrfOx7DYZLzBnhS3Uz9hdUJZygkuN2R28VUVvOhpXX1"
    };

    public static User userNull = new User
    {
        Email = "test123@test.net",
        Password = "NgB9iXzrfOx7DYZLzBnhS3Uz9hdUJZygkuN2R28VUVvOhpXX1"
    };

    public static LoginRequest login = new LoginRequest
    {
        Email = "test@test.net",
        Password = "Kutas123!"
    };

    public static LoginRequest loginBadMail = new LoginRequest
    {
        Email = "test1@test.net",
        Password = "Kutas123!"
    };

    public static LoginRequest loginBadPass = new LoginRequest
    {
        Email = "test@test.net",
        Password = "Kutas123!!"
    };



    public static String jwtKey = "webSignatureSecretKeyTest@";

    public static async Task<AppDbContext> GetDbContext()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseInternalServiceProvider(serviceProvider)
            .Options;

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            var databaseContext = new AppDbContext(options);
            databaseContext.Database.EnsureCreated();

            if (await databaseContext.Users.CountAsync() == 0)
            {
                databaseContext.Users.Add(new User
                {
                    Id = 1,
                    First_name = "usr",
                    Last_name = "test",
                    Email = "test@test.net",
                    Password = "NgB9iXzrfOx7DYZLzBnhS3Uz9hdUJZygkuN2R28VUVvOhpXX"
                });

                databaseContext.Users.Add(new User
                {
                    Id = 2,
                    First_name = "usr2",
                    Last_name = "test2",
                    Email = "test2@test.net",
                    Password = "NgB9iXzrfOx7DYZLzBnhS3Uz9hdUJZygkuN2R28VUVvOhpXX2"
                });

                databaseContext.UserPublicKeys.Add(new UserPublicKey
                {
                    Id = 1,
                    UserId =1,
                    PublicKey = "test",
                    ExpiresAt = DateTime.UtcNow.AddYears(1)
                });
                await databaseContext.SaveChangesAsync();
            }

            return databaseContext;
        }
    }

    public static string publicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ" +
        "8AMIIBCgKCAQEAvgEoubzgok254yJXBySK/lgnXxW5esycfCCN" +
        "j1e6FIfz2J4JkJJxx1ehaol5dsnVOnGVjfp4rjwTFZ2jAVdVa" +
        "MM6QNb9RzyEwJQah1NmXAahA05b9VfwOc9mJRO33r+zRUDQmH" +
        "qUqGXTh/W7eZL0mECZxV2LOF4MjecgMqQTJI7pf6em0yGka" +
        "1yZIOf/SrfWYQUOtfyqrAndOzxjcOzrr1lcq5yI63SM" +
        "zujnA5ybFZGLXHHZNaBpwPqyTVkFL7Ghugev81ps" +
        "qfewxWaFKKGWWcZ+wjw8ZaZ4DF0X61gEUNOsqR+J" +
        "l3lsFtlQHvZrbkdBXSgOpkN78JCw/9kMuKvMrwIDAQAB";
    
    public static PublicKeyRequest keyRequest = new PublicKeyRequest
    {
        UserId = 1,
        PublicKey = publicKey
    };

    public static PublicKeyRequest keyRequestBad = new PublicKeyRequest
    {
        UserId = 2
    };
}