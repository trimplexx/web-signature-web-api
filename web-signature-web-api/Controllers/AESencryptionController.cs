using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using web_signature_web_api.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

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
// Convert base64 strings to byte arrays
                byte[] pdfFile = Convert.FromBase64String(fileUpload.PdfFileBase64);
                byte[] publicKeyFile = Convert.FromBase64String(fileUpload.PublicKeyFileBase64);

                // Generate AES key
                using Aes aes = Aes.Create();
                byte[] aesKey = aes.Key;

                // Encrypt PDF with AES
                byte[] encryptedPdf = EncryptWithAes(pdfFile, aesKey, aes.IV);

                // Encrypt AES key with RSA public key
                byte[] encryptedAesKey = EncryptWithRsa(aesKey, publicKeyFile);

                // Write encrypted PDF file
                string pdfPath = "encrypted.pdf";
                System.IO.File.WriteAllBytes(pdfPath, encryptedPdf);

                // Write encrypted AES key
                string keyPath = "key.aes";
                System.IO.File.WriteAllBytes(keyPath, encryptedAesKey);

                // Stworzenie zip z kluczem oraz zaszyfrowanym pdf.
                string zipPath = "encrypted_files.zip";
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(pdfPath, Path.GetFileName(pdfPath));
                    zip.CreateEntryFromFile(keyPath, Path.GetFileName(keyPath));
                }
            
                var memory = new MemoryStream();
                using (var stream = new FileStream(zipPath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;

                return File(memory, "application/zip", Path.GetFileName(zipPath));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error", message = ex.Message });
            }
        }

        
        private byte[] EncryptWithAes(byte[] file, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream();
            // Write IV at the start of the stream
            ms.Write(iv, 0, iv.Length);
            using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(file, 0, file.Length);
            cs.Close();

            return ms.ToArray();
        }

        private byte[] EncryptWithRsa(byte[] key, byte[] publicKeyFile)
        {
            string jwkJson = Encoding.UTF8.GetString(publicKeyFile);
            var jsonWebKey = new JsonWebKey(jwkJson);

            if (!jsonWebKey.Kty.Equals("RSA"))
            {
                throw new ArgumentException("Invalid key type. Expected 'RSA'");
            }

            var rsaParameters = new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(jsonWebKey.N),
                Exponent = Base64UrlEncoder.DecodeBytes(jsonWebKey.E)
            };

            using RSA rsa = RSA.Create();
            rsa.ImportParameters(rsaParameters);

            return rsa.Encrypt(key, RSAEncryptionPadding.Pkcs1);
        }
        
    }
}