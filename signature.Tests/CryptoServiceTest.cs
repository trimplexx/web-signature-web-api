using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using signature.Tests.Content;
using web_signature_web_api.Models;
using web_signature_web_api.Services;

namespace signature.Tests;

public class CryptoServiceTest
{
    [Fact]
    public async Task UploadPublicKey_ReturnOkObjectResult()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var cryptoService = new CryptoService(context);
            // Act
            var result = await cryptoService.UploadPublicKey(Helper.keyRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Klucz publiczny przesłany pomyślnie.", 
                okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value));
        }
    }

    [Fact]
    public async Task UploadPublicKey_NotFoundObjectResult()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var cryptoService = new CryptoService(context);
            // Act
            var result = await cryptoService.UploadPublicKey(new PublicKeyRequest());

            // Assert
            var badRequestResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("UserNotFound", 
                badRequestResult.Value.GetType().GetProperty("error")?.GetValue(badRequestResult.Value));
            Assert.Equal("Nie znaleziono użytkownika",
                badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
        }
    }

    [Fact]
    public async Task CheckKeyExists_ReturnOkObjectResult()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var cryptoService = new CryptoService(context);

            // Act
            var result = await cryptoService.CheckKeyExists(Helper.keyRequest.UserId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;
            bool keyExists = false;

            if (value != null)
            {
                string res = value.ToString();
                if (res.Contains("True", StringComparison.OrdinalIgnoreCase))
                {
                    keyExists = true;
                }
            }
            Assert.True(keyExists);
        }
    }

    [Fact]
    public async Task DeleteKey_ReturnOkObjectResult()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var cryptoService = new CryptoService(context);
            // Act
            var result = await cryptoService.DeleteKey(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Klucz publiczny usunięty pomyślnie.",
                okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value));
        }
    }

    [Fact]
    public async Task DeleteKey_NotFoundObjectResult()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var cryptoService = new CryptoService(context);
            // Act
            var result = await cryptoService.DeleteKey(9999);

            // Assert
            var badRequestResult = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal("Klucz publiczny nie został znaleziony.",
                badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
        }
    }

    [Fact]
    public async Task DeleteKey_StatusCodeResult()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var mockSet = new Mock<DbSet<UserPublicKey>>();
            context.UserPublicKeys = mockSet.Object;

            var cryptoService = new CryptoService(context);

            // Symuluj wyjątek podczas usuwania klucza
            mockSet.Setup(m => m.FindAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await cryptoService.DeleteKey(1);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }

    [Fact]
    public async Task DownloadPublicKey_ReturnOkObjectResult()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var cryptoService = new CryptoService(context);
            // Act
            var result = await cryptoService.DownloadPublicKey(Helper.keyRequest.UserId);

            // Assert
            var okResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/octet-stream", okResult.ContentType);
            Assert.Equal("publicKey.spki", okResult.FileDownloadName); ;
        }
    }

    [Fact]
    public async Task DownloadPublicKey_NotFoundObjectResultUser()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var cryptoService = new CryptoService(context);
            // Act
            var result = await cryptoService.DownloadPublicKey(9999);

            // Assert
            var badRequestResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("UserNotFound",
                badRequestResult.Value.GetType().GetProperty("error")?.GetValue(badRequestResult.Value));
            Assert.Equal("Nie znaleziono użytkownika",
                badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
        }
    }

    [Fact]
    public async Task DownloadPublicKey_NotFoundObjectResultKey()
    {
        // Arrange
        using (var context = await Helper.GetDbContext())
        {
            var cryptoService = new CryptoService(context);

            // Act
            var result = await cryptoService.DownloadPublicKey(Helper.keyRequestBad.UserId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("PublicKeyNotFound",
                notFoundResult.Value.GetType().GetProperty("error")?.GetValue(notFoundResult.Value));
            Assert.Equal("Nie znaleziono klucza publicznego dla użytkownika",
                notFoundResult.Value.GetType().GetProperty("message")?.GetValue(notFoundResult.Value));
        }
    }

}
