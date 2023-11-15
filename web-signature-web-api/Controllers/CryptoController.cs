using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using web_signature_web_api.Migrations;
using web_signature_web_api.Models;


namespace web_signature_web_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CryptoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload-public-key")]
        public async Task<IActionResult> UploadPublicKey([FromBody] PublicKeyRequest request)
        {
            // Wyszukanie użytkownika o podanym Id
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { error = "UserNotFound", message = "Nie znaleziono użytkownika" });
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

            return Ok(new { message = "Klucz publiczny przesłany pomyślnie." });
        }

        // Metoda do sprawdzenia, czy klucz istnieje
        [HttpGet("check-key-exists")]
        public async Task<IActionResult> CheckKeyExists([FromQuery] int userId)
        {
            // Sprawdzenie, czy użytkownik posiada klucz
            var keyExists = await _context.UserPublicKeys
                .AnyAsync(k => k.UserId == userId && k.ExpiresAt > DateTime.UtcNow);
            return Ok(new { keyExists });
        }

        [HttpDelete("delete-key")]
        public async Task<IActionResult> DeleteKey([FromQuery] int id)
        {
            try
            {
                var existingKey = await _context.UserPublicKeys.FirstOrDefaultAsync(k => k.UserId == id);
                if (existingKey != null)
                {
                    _context.UserPublicKeys.Remove(existingKey);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Klucz publiczny usunięty pomyślnie." });
                }
                else
                {
                    return NotFound(new { message = "Klucz publiczny nie został znaleziony." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "Wystąpił błąd podczas usuwania klucza.", exception = ex.Message });
            }
        }

        [HttpGet("download-public-key/{userId}")]
        public async Task<IActionResult> DownloadPublicKey(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { error = "UserNotFound", message = "Nie znaleziono użytkownika" });
            }

            var userPublicKey = await _context.UserPublicKeys.FirstOrDefaultAsync(k => k.UserId == userId);
            if (userPublicKey == null)
            {
                return NotFound(new
                    { error = "PublicKeyNotFound", message = "Nie znaleziono klucza publicznego dla użytkownika" });
            }

            // Zakładam, że twój klucz publiczny jest zakodowany w Base64
            var publicKeyBytes = Convert.FromBase64String(userPublicKey.PublicKey);
            return File(publicKeyBytes, "application/octet-stream", "publicKey.spki");
        }
    }
}