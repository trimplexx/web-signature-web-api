using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_signature_web_api.Interfaces;
using web_signature_web_api.Migrations;
using web_signature_web_api.Models;

namespace web_signature_web_api.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly AppDbContext _context;

        public CryptoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> UploadPublicKey(PublicKeyRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return new NotFoundObjectResult(new { error = "UserNotFound", message = "Nie znaleziono użytkownika" });
            }

            var userPublicKey = new UserPublicKey
            {
                UserId = request.UserId,
                PublicKey = request.PublicKey,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(1)
            };

            _context.UserPublicKeys.Add(userPublicKey);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Klucz publiczny przesłany pomyślnie." });
        }

        public async Task<IActionResult> CheckKeyExists(int userId)
        {
            var keyExists = await _context.UserPublicKeys
                .AnyAsync(k => k.UserId == userId && k.ExpiresAt > DateTime.UtcNow);
            return new OkObjectResult(new { keyExists });
        }

        public async Task<IActionResult> DeleteKey(int id)
        {
            try
            {
                var existingKey = await _context.UserPublicKeys.FirstOrDefaultAsync(k => k.UserId == id);
                if (existingKey != null)
                {
                    _context.UserPublicKeys.Remove(existingKey);
                    await _context.SaveChangesAsync();
                    return new OkObjectResult(new { message = "Klucz publiczny usunięty pomyślnie." });
                }
                else
                {
                    return new NotFoundObjectResult(new { message = "Klucz publiczny nie został znaleziony." });
                }
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> DownloadPublicKey(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new NotFoundObjectResult(new { error = "UserNotFound", message = "Nie znaleziono użytkownika" });
            }

            var userPublicKey = await _context.UserPublicKeys.FirstOrDefaultAsync(k => k.UserId == userId);
            if (userPublicKey == null)
            {
                return new NotFoundObjectResult(new
                { error = "PublicKeyNotFound", message = "Nie znaleziono klucza publicznego dla użytkownika" });
            }

            var publicKeyBytes = Convert.FromBase64String(userPublicKey.PublicKey);
            return new FileContentResult(publicKeyBytes, "application/octet-stream")
            {
                FileDownloadName = "publicKey.spki"
            };
        }
    }
}
