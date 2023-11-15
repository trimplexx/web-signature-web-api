using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using web_signature_web_api.Models;

namespace web_signature_web_api.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class AESDecryptionController : ControllerBase
{
    [HttpPost("aes-decrypt")]
    public IActionResult AESdecrypt([FromBody] EncryptedFile encryptedFileUpload)
    {
        try
        {
            // Convert base64 strings to byte arrays
            byte[] encryptedPdfFile = Convert.FromBase64String(encryptedFileUpload.EncryptedPdfFileBase64);
            byte[] encryptedAESKey = Convert.FromBase64String(encryptedFileUpload.EncryptedAESKeyBase64);
            byte[] privateKeyFile = Convert.FromBase64String(encryptedFileUpload.PrivateKeyFileBase64);

            // Decrypt AES key with RSA private key
            byte[] aesKey = DecryptWithRsa(encryptedAESKey, privateKeyFile);

            // Decrypt PDF with AES
            byte[] decryptedPdf = DecryptWithAes(encryptedPdfFile, aesKey);

            // Return the decrypted PDF as a downloadable file
            return File(decryptedPdf, "application/pdf", "decrypted-file.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Error", message = ex.Message });
        }
    }

    private byte[] DecryptWithAes(byte[] encryptedFile, byte[] key)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;

        using MemoryStream ms = new MemoryStream(encryptedFile);
        // Read IV from the start of the stream
        byte[] iv = new byte[16];
        ms.Read(iv, 0, iv.Length);
        aes.IV = iv;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

        var result = new MemoryStream();
        cs.CopyTo(result);
        return result.ToArray();
    }

    private byte[] DecryptWithRsa(byte[] encryptedKey, byte[] privateKeyFile)
    {
        string jwkJson = Encoding.UTF8.GetString(privateKeyFile);
        var jsonWebKey = new JsonWebKey(jwkJson);

        if (!jsonWebKey.Kty.Equals("RSA"))
        {
            throw new ArgumentException("Invalid key type. Expected 'RSA'");
        }

        var rsaParameters = new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(jsonWebKey.N),
            Exponent = Base64UrlEncoder.DecodeBytes(jsonWebKey.E),
            D = Base64UrlEncoder.DecodeBytes(jsonWebKey.D),
            P = Base64UrlEncoder.DecodeBytes(jsonWebKey.P),
            Q = Base64UrlEncoder.DecodeBytes(jsonWebKey.Q),
            DP = Base64UrlEncoder.DecodeBytes(jsonWebKey.DP),
            DQ = Base64UrlEncoder.DecodeBytes(jsonWebKey.DQ),
            InverseQ = Base64UrlEncoder.DecodeBytes(jsonWebKey.QI)
        };

        using RSA rsa = RSA.Create();
        rsa.ImportParameters(rsaParameters);

        return rsa.Decrypt(encryptedKey, RSAEncryptionPadding.Pkcs1);
    }
}
}