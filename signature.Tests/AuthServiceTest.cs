using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using signature.Tests.Content;
using web_signature_web_api.Services;

namespace signature.Tests;

public class AuthServiceTests
{


    [Fact]
    public async Task Register_ReturnsOkResult()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var authService = new AuthService(context, Mock.Of<IConfiguration>());

            // Act
            var result = await authService.Register(Helper.newUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Użytkownik został pomyślnie zarejestrowany", okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value));
        }
    }

    [Fact]
    public async Task Register_ReturnsBadRequestMail()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var authService = new AuthService(context, Mock.Of<IConfiguration>());

            // Act
            var result = await authService.Register(Helper.userMail);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("EmailAlreadyExists", badRequestResult.Value.GetType().GetProperty("error")?.GetValue(badRequestResult.Value));
            Assert.Equal("Użytkownik o podanym adresie e-mail już istnieje",
                badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
        }
    }

    [Fact]
    public async Task Register_ReturnsBadRequest()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var authService = new AuthService(context, Mock.Of<IConfiguration>());

            // Act
            var result = await authService.Register(Helper.userNull);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Wystąpił błąd podczas rejestracji użytkownika.",
                badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
        }
    }

    [Fact]
    public async Task Login_ReturnsOkResult()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["Jwt:SecretKey"]).Returns(Helper.jwtKey);
            var authService = new AuthService(context, configurationMock.Object);
            // Act
            var result = await authService.Login(Helper.login);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var token = okResult.Value.GetType().GetProperty("token")?.GetValue(okResult.Value) as string;

            Assert.NotNull(token);
        }
    }

    [Fact]
    public async Task Login_ReturnsBadRequestMail()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["Jwt:SecretKey"]).Returns(Helper.jwtKey);
            var authService = new AuthService(context, configurationMock.Object);
            // Act
            var result = await authService.Login(Helper.loginBadMail);

            // Assert
            var badRequestResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("UserNotFound", badRequestResult.Value.GetType().GetProperty("error")?.GetValue(badRequestResult.Value));
            Assert.Equal("Użytkownik o podanym adresie e-mail nie istnieje",
                badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
        }
    }

    [Fact]
    public async Task Login_ReturnsBadRequestLogin()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["Jwt:SecretKey"]).Returns(Helper.jwtKey);
            var authService = new AuthService(context, configurationMock.Object);
            // Act
            var result = await authService.Login(Helper.loginBadPass);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("InvalidCredentials", badRequestResult.Value.GetType().GetProperty("error")?.GetValue(badRequestResult.Value));
            Assert.Equal("Wprowadzono nieprawidłowe hasło",
                badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
        }
    }
}