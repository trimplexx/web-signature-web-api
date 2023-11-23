using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Security.Cryptography;
using web_signature_web_api.Interfaces;
using web_signature_web_api.Models;

namespace web_signature_web_api.Services
{
    public class AESencryptionService : IAESencryptionService
    {
        public async Task<IActionResult> AESencrypt(FileUpload fileUpload)
        {
            try
            {
                byte[] pdfFileBytes = Convert.FromBase64String(fileUpload.PdfFileBase64);
                byte[] publicKeyBytes = Convert.FromBase64String(fileUpload.PublicKeyFileBase64);

                using RSA rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                using Aes aes = Aes.Create();
                byte[] aesKey = aes.Key;

                using ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] encryptedPdfFileBytes;
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); 
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(pdfFileBytes, 0, pdfFileBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    encryptedPdfFileBytes = ms.ToArray();
                }

                byte[] encryptedAesKeyBytes = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);

                using var memoryStream = new MemoryStream();
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var pdfEntry = archive.CreateEntry("encrypted.pdf", CompressionLevel.Fastest);
                    using (var entryStream = pdfEntry.Open())
                    using (var writer = new BinaryWriter(entryStream))
                    {
                        writer.Write(encryptedPdfFileBytes);
                    }

                    var aesKeyEntry = archive.CreateEntry("encrypted.aeskey", CompressionLevel.Fastest);
                    using (var entryStream = aesKeyEntry.Open())
                    using (var writer = new BinaryWriter(entryStream))
                    {
                        writer.Write(encryptedAesKeyBytes);
                    }
                }

                byte[] zipFileBytes = memoryStream.ToArray();
                return new FileContentResult(zipFileBytes, "application/zip")
                {
                    FileDownloadName = "encrypted.zip"
                };
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { error = "Error", message = ex.Message });
            }
        }
    }
}
