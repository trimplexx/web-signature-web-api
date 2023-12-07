using Microsoft.AspNetCore.Mvc;
using signature.Tests.Content;
using web_signature_web_api.Models;
using web_signature_web_api.Services;

namespace signature.Tests;

public class AESEncryptionServiceTest
{
    [Fact]
    public async Task AESencrypt_ValidFile_ReturnsFileContentResult()
    {
        // Arrange
        var filePath = Path.Combine("..", "..", "..", "Content", "TestFile.pdf");

        var pdfFileBase64 = Convert.ToBase64String(File.ReadAllBytes(filePath));

        var fileUpload = new FileUpload
        {
            PdfFileBase64 = pdfFileBase64,
            PublicKeyFileBase64 = Helper.publicKey
        };

        var aes = new AESencryptionService();

        // Act
        var result = await aes.AESencrypt(fileUpload);

        // Assert
        var fileContentResult = Assert.IsType<FileContentResult>(result);
        Assert.NotNull(pdfFileBase64);
        Assert.Equal("application/zip", fileContentResult.ContentType);
        Assert.Equal("encrypted.zip", fileContentResult.FileDownloadName);
    }
}
