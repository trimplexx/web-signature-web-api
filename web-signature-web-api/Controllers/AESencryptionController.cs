using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using web_signature_web_api.Models;

namespace web_signature_web_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AESencryptionController : ControllerBase
    {
        [HttpPost("aes-encrypt")]
        public IActionResult AESencrypt([FromBody] FileUpload fileUpload)
        {
            try
            {
                // Przekształć pliki z Base64
                byte[] pdfFileBytes = Convert.FromBase64String(fileUpload.PdfFileBase64);
                byte[] publicKeyBytes = Convert.FromBase64String(fileUpload.PublicKeyFileBase64);

                // Import public key
                using RSA rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                // Utwórz klucz AES
                using Aes aes = Aes.Create();
                byte[] aesKey = aes.Key;

                // Szyfruj plik PDF z kluczem AES
                using ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] encryptedPdfFileBytes;
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Write the IV at the start of the stream
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(pdfFileBytes, 0, pdfFileBytes.Length);
                        cs.FlushFinalBlock(); // Make sure to flush the CryptoStream
                    }

                    encryptedPdfFileBytes = ms.ToArray();
                }

                // Szyfruj klucz AES z kluczem publicznym
                byte[] encryptedAesKeyBytes = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);

                // Utwórz plik ZIP
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

                // Zwróć plik ZIP jako rezultat
                return File(zipFileBytes, "application/zip", "encrypted.zip");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error", message = ex.Message });
            }
        }
    }
}